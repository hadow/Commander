using System;
using System.Collections.Generic;
using System.Reflection;
using EW.FileSystem;
using EW.Primitives;
using System.Linq;
namespace EW
{
    /// <summary>
    /// 创建自定义YAML对象
    /// </summary>
    public class ObjectCreator
    {
        static readonly Dictionary<string, Assembly> ResolvedAssemblies = new Dictionary<string, Assembly>();
        readonly Pair<Assembly, string>[] assemblies;
        readonly Cache<string, Type> typeCache;
        readonly Cache<Type, ConstructorInfo> ctorCache;

        public static Action<string> MissingTypeAction = s =>
        {
            throw new InvalidOperationException("Cannot locate type:{0}".F(s));
        };

        public ObjectCreator(Assembly a)
        {
            ctorCache = new Cache<Type, ConstructorInfo>(GetCtor);
            typeCache = new Cache<string, Type>(FindType);
            assemblies = a.GetNamespaces().Select(ns => Pair.New(a, ns)).ToArray();
        }

        public ObjectCreator(Manifest manifest,FileSystem.FileSystem modeFiles)
        {
            typeCache = new Cache<string, Type>(FindType);
            ctorCache = new Cache<Type, ConstructorInfo>(GetCtor);

            var assemblyList = new List<Assembly>() { typeof(WarGame).Assembly };

            foreach(var path in manifest.Assemblies)
            {
                var data = modeFiles.Open(path).ReadAllBytes();
                
                var hash = CryptoUtil.SHA1Hash(data);

                Assembly assembly;
                if(!ResolvedAssemblies.TryGetValue(hash,out assembly))
                {
#if DEBUG
                    //var pdbPath = path.Replace(".dll", ".pdb");
                    //var pdbData = modeFiles.Open(pdbPath).ReadAllBytes();

                    //assembly = Assembly.Load(data, pdbData);
                    assembly = Assembly.Load(data);
                    

                    //var path2 = "file:///android_asset/Content/mods/common/" + path.Split('|')[1];
                    //var path2 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    //var fileName = System.IO.Path.Combine(path2, "Content/mods/common/"+path.Split('|')[1]);
                    //var assembly2 = Assembly.LoadFrom(fileName);

                    //assembly = Assembly.Load(data);

                    //string path2 = Android.OS.Environment.ExternalStorageDirectory.Path;    
                    //string path2 = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    //string path2 = Android.App.Application.Context.GetExternalFilesDir("").Path;
                    //string filePath = System.IO.Path.Combine(path2, path.Split('|')[1]);
                    //string filePath2 = System.IO.Path.Combine(path2, path.Split('|')[1].Replace(".dll", ".pdb"));
                    //System.IO.File.WriteAllBytes(filePath, data);
                    //System.IO.File.WriteAllBytes(filePath2, pdbData);
                    //if (System.IO.File.Exists(filePath))
                    //{
                    //    Android.Util.Log.Debug("", "");
                    //    assembly = Assembly.LoadFile(filePath);
                    //}

#else
                    assembly = Assembly.Load(data);
#endif
                    ResolvedAssemblies.Add(hash, assembly);
                }

                assemblyList.Add(assembly);
            }

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            assemblies = assemblyList.SelectMany(asm => asm.GetNamespaces().Select(ns => Pair.New(asm, ns))).ToArray();
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Assembly ResolveAssembly(object sender,ResolveEventArgs e)
        {
            foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(a.FullName == e.Name)
                {
                    return a;
                }
            }
            return assemblies.Select(a => a.First).FirstOrDefault(a => a.FullName == e.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public Type FindType(string className)
        {
            return assemblies.Select(pair => pair.First.GetType(pair.Second + "." + className, false)).FirstOrDefault(t => t != null);
        }

        public ConstructorInfo GetCtor(Type type)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var ctors = type.GetConstructors(flags).Where(x => x.HasAttribute<UseCtorAttribute>());
            if (ctors.Count() > 1)
                throw new InvalidOperationException("ObjectCreator:UseCtor on multiple constructors;invalid.");
            return ctors.FirstOrDefault();
        }


        public IEnumerable<Type> GetTypesImplementing<T>()
        {
            var it = typeof(T);
            return GetTypes().Where(t => t != it && it.IsAssignableFrom(t));
        }


        public IEnumerable<Type> GetTypes()
        {
            return assemblies.Select(ma => ma.First).Distinct().SelectMany(ma => ma.GetTypes());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="className"></param>
        /// <returns></returns>
        public T CreateObject<T>(string className)
        {
            return CreateObject<T>(className, new Dictionary<string, object>());
        }

        public T CreateObject<T>(string className,Dictionary<string,object> args)
        {
            var type = typeCache[className];
            if(type == null)
            {
                MissingTypeAction(className);
                return default(T);
            }

            var ctor = ctorCache[type];
            if (ctor == null)
                return (T)CreateBasic(type);
            else
                return (T)CreateUsingArgs(ctor, args);
            
        }

        public object CreateBasic(Type type)
        {
            return type.GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        public object CreateUsingArgs(ConstructorInfo ctor,Dictionary<string,object> args)
        {
            var p = ctor.GetParameters();
            var a = new object[p.Length];
            for(var i = 0; i < p.Length; i++)
            {
                var key = p[i].Name;
                if (!args.ContainsKey(key))
                    throw new InvalidOperationException("ObjectCreator: key '{0}' not found".F(key));
                a[i] = args[key];
            }

            return ctor.Invoke(a);
        }

        [AttributeUsage(AttributeTargets.Constructor)]
        public sealed class UseCtorAttribute : Attribute { }
    }
}