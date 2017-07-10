using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Diagnostics;
using EW.Graphics;
using EW.Primitives;
using EW.Traits;
using Eluant;
namespace EW.Scripting
{
    public interface IScriptNotifyBind
    {
        void OnScriptBind(ScriptContext context);
    }

    public sealed class ExposedForDestroyedActors : Attribute { }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ScriptPropertyGroupAttribute : Attribute
    {
        public readonly string Category;

        public ScriptPropertyGroupAttribute(string category) { Category = category; }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class ScriptActorProperties
    {
        protected readonly Actor Self;
        protected readonly ScriptContext Context;

        public ScriptActorProperties(ScriptContext context,Actor self)
        {
            Self = self;
            Context = context;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class ScriptPlayerProperties
    {
        protected readonly Player Player;
        protected readonly ScriptContext Context;

        public ScriptPlayerProperties(ScriptContext context,Player player)
        {
            Player = player;
            Context = context;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ScriptContext:IDisposable
    {

        const int MaxUserScriptMemory = 50 * 1024 * 1024;//脚本最大内存占用量
        const int MaxUserScriptInstructions = 1000000;//脚本指令数量
        public World World { get; private set; }
        public WorldRenderer WorldRenderer { get; private set;}

        readonly MemoryConstrainedLuaRuntime runtime;

        readonly LuaFunction tick;

        readonly Type[] knownActorCommands;

        public readonly Type[] PlayerCommands;

        public readonly Cache<ActorInfo, Type[]> ActorCommands;

        

        static readonly object[] NoArguments = new object[0];
        public ScriptContext(World world,WorldRenderer worldRenderer,IEnumerable<string> scripts)
        {
            LuaRuntime.LoadAndroidAsset += (string filename) =>
              {
                  using (StreamReader sr = new StreamReader(Android.App.Application.Context.Assets.Open(Platform.ResolvePath(".", "lua", filename))))
                  {
                      var luaContent = sr.ReadToEnd();
                      if (!string.IsNullOrEmpty(luaContent))
                      {
                          runtime.LoadBuffer(luaContent, filename);
                          
                      }
                  }
              };
            runtime = new MemoryConstrainedLuaRuntime();
            
            this.World = world;
            this.WorldRenderer = worldRenderer;

            knownActorCommands = WarGame.ModData.ObjectCreator.GetTypesImplementing<ScriptActorProperties>().ToArray();
            
            ActorCommands = new Cache<ActorInfo, Type[]>(FilterActorCommands);


            var knownPlayerCommands = WarGame.ModData.ObjectCreator.GetTypesImplementing<ScriptPlayerProperties>().ToArray();

            PlayerCommands = FilterCommands(world.Map.Rules.Actors["player"], knownPlayerCommands);

            runtime.Globals["GameDir"] = Platform.SupportDir;
            
            //var directory = Directory.GetCurrentDirectory();
            //var directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //var directory = Android.App.Application.Context.ApplicationContext.PackageResourcePath;
            var directory = Platform.ResolvePath(".", "lua", "scriptwrapper.lua");
            string content;
            using (StreamReader sr = new StreamReader(Android.App.Application.Context.Assets.Open(Platform.ResolvePath(".", "lua", "scriptwrapper.lua"))))
            {
                content = sr.ReadToEnd();
            }
            //runtime.DoBuffer(File.Open(directory, FileMode.Open, FileAccess.Read).ReadAllText(), "scriptwrapper.lua").Dispose();
            runtime.DoBuffer(content, "scriptwrapper.lua").Dispose();
            tick = (LuaFunction)runtime.Globals["Tick"];

            //Register globals
            using(var fatalError = runtime.CreateFunctionFromDelegate((Action<string>)FatalError))
            {
                runtime.Globals["FatalError"] = fatalError;
            }
            runtime.Globals["MaxUserScriptInstructions"] = MaxUserScriptInstructions;

            using (var registerGlobal = (LuaFunction)runtime.Globals["RegisterSandboxedGlobal"])
            {
                using(var log = runtime.CreateFunctionFromDelegate((Action<string>)LogDebugMessage))
                {
                    registerGlobal.Call("print", log).Dispose();
                }

                //Register global tables
                var bindings = WarGame.ModData.ObjectCreator.GetTypesImplementing<ScriptGlobal>();
                foreach(var bind in bindings)
                {
                    var ctor = bind.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(c => {

                        var p = c.GetParameters();
                        return p.Length == 1 && p.First().ParameterType == typeof(ScriptContext);
                    });


                    if (ctor == null)
                        throw new InvalidOperationException("{0} must define a constructor that takes a ScriptContext context parameter");

                    var binding = (ScriptGlobal)ctor.Invoke(new[] { this });
                    using (var obj = binding.ToLuaValue(this))
                        registerGlobal.Call(binding.Name, obj).Dispose();

                }
            }

            //System functions do not count towards the memory limit
            //系统函数不计入内存限制
            runtime.MaxMemoryUse = runtime.MemoryUse + MaxUserScriptMemory;
            using (var loadScript = (LuaFunction)runtime.Globals["ExecuteSandboxedScript"])
            {
                foreach (var s in scripts)
                {
                    loadScript.Call(s, world.Map.Open(s).ReadAllText()).Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="a"></param>
        public void RegisterMapActor(string name,Actor a)
        {
            using(var registerGlobal = (LuaFunction)runtime.Globals["RegisterSandboxedGlobal"])
            {
                if (runtime.Globals.ContainsKey(name))
                    throw new LuaException("The global name '{0}' is reserved,and may not be used by a map actor".F(name));

                using (var obj = a.ToLuaValue(this))
                {
                    registerGlobal.Call(name, obj).Dispose();
                        
                }
            }

            
                
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searcher"></param>
        /// <param name="index"></param>
        

        Type[] FilterActorCommands(ActorInfo ai)
        {
            return FilterCommands(ai, knownActorCommands);
        }

        Type[] FilterCommands(ActorInfo ai,Type[] knownCommands)
        {
            var method = typeof(ActorInfo).GetMethod("HasTraitInfo");
            return knownActorCommands.Where(c => ExtractRequiredTypes(c).All(t => (bool)method.MakeGenericMethod(t).Invoke(ai, NoArguments))).ToArray();
        }


        static IEnumerable<Type> ExtractRequiredTypes(Type t)
        {
            var outer = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Requires<>));
            return outer.SelectMany(i => i.GetGenericArguments());
        }


        public void Tick(Actor actor)
        {
           
           tick.Call().Dispose();
        }

        public void WorldLoaded()
        {
            if (FatalErrorOccurred)
                return;

            using (var worldLoaded = (LuaFunction)runtime.Globals["WorldLoaded"])
                worldLoaded.Call().Dispose();

        }

        public bool FatalErrorOccurred { get; private set; }
        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="message"></param>
        public void FatalError(string message)
        {
            var stacktrace = new StackTrace().ToString();

            FatalErrorOccurred = true;

            World.AddFrameEndTask(w =>
            {
                World.EndGame();
                World.SetPauseState(true);
                World.PauseStateLocked = true;
            });
        }

        private void LogDebugMessage(string message)
        {

        }

        public LuaTable CreateTable() { return runtime.CreateTable(); }


        public void Dispose() { }
    }
}