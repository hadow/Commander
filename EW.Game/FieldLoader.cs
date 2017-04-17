using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Globalization;
using EW.Primitives;
namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public static class FieldLoader
    {

        public class MissingFieldsException : YamlException
        {
            public readonly string[] Missing;
            public readonly string Header;

            public override string Message
            {
                get
                {
                    return (string.IsNullOrEmpty(Header) ? "" : Header + ": ") + Missing[0] + string.Concat(Missing.Skip(1).Select(m => ", " + m));
                }
            }

            public MissingFieldsException(string[] missing,string header = null,string headerSingle = null) : base(null)
            {
                Header = missing.Length > 1 ? header : headerSingle ?? header;
                Missing = missing;
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("Missing", Missing);
                info.AddValue("Header", Header);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class FieldLoadInfo
        {
            public readonly FieldInfo Field;
            public readonly SerializeAttribute Attribute;
            public readonly string YamlName;
            public readonly Func<MiniYaml, object> Loader;

            internal FieldLoadInfo(FieldInfo field,SerializeAttribute attr,string yamlName,Func<MiniYaml,object> loader = null)
            {
                Field = field;
                Attribute = attr;
                YamlName = yamlName;
                Loader = loader;
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public class SerializeAttribute : Attribute
        {
            public static readonly SerializeAttribute Default = new SerializeAttribute(true);

            public bool IsDefault { get { return this == Default; } }

            public readonly bool Serialize;

            public string YamlName;

            public string Loader;

            public bool FromYamlKey;

            public bool DictionaryFromYamlKey;

            public bool Required;

            public SerializeAttribute(bool serialize = true,bool required = false)
            {
                Serialize = serialize;
                Required = required;
            }
            
            internal Func<MiniYaml,object> GetLoader(Type type)
            {
                const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                if (!string.IsNullOrEmpty(Loader))
                {
                    var method = type.GetMethod(Loader, Flags);
                    if (method == null)
                        throw new InvalidOperationException("{0} does not specify a loader function '{1}'".F(type.Name, Loader));

                    return (Func<MiniYaml,object>)Delegate.CreateDelegate(typeof(Func<MiniYaml, object>), method);
                }
                return null;
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public sealed class LoadUsingAttribute : SerializeAttribute
        {
            public LoadUsingAttribute(string loader,bool required = false)
            {
                Loader = loader;
                Required = required;
            }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public sealed class RequireAttribute : SerializeAttribute
        {
            public RequireAttribute() : base(true, true) { }
        }

        [AttributeUsage(AttributeTargets.Field)]
        public sealed class IgnoreAttribute : SerializeAttribute
        {
            public IgnoreAttribute() : base(false) { }
        }

        static readonly ConcurrentCache<Type, FieldLoadInfo[]> TypeLoadInfo = new ConcurrentCache<Type, FieldLoadInfo[]>(BuildTypeLoadInfo);

        public static Func<string, Type, string, object> InvalidValueAction = (s, t, f) =>
           {
               throw new YamlException("FieldLoader: Cannot pars '{0}' into '{1}.{2}'".F(s, f, t));
           };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static FieldLoadInfo[] BuildTypeLoadInfo(Type type)
        {
            var ret = new List<FieldLoadInfo>();
            foreach(var ff in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var field = ff;

                var sa = field.GetCustomAttributes<SerializeAttribute>(false).DefaultIfEmpty(SerializeAttribute.Default).First();
                if (!sa.Serialize)
                    continue;

                var yamlName = string.IsNullOrEmpty(sa.YamlName) ? field.Name : sa.YamlName;
                var loader = sa.GetLoader(type);

                if (loader == null && sa.FromYamlKey)
                    loader = yaml => GetValue(yamlName, field.FieldType, yaml, field);

                var fli = new FieldLoadInfo(field, sa, yamlName, loader);
                ret.Add(fli);
            }
            return ret.ToArray();
        }

        public static object GetValue(string fieldName,Type fieldType,string value,MemberInfo field)
        {
            return GetValue(fieldName, fieldType, new MiniYaml(value), field);
        }

        public static object GetValue(string fieldName,Type fieldType,MiniYaml yaml,MemberInfo field)
        {
            var value = yaml.Value;
            if (value != null)
                value = value.Trim();

            if(fieldType == typeof(int))
            {
                int res;
                if (Exts.TryParseIntegerInvariant(value, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(ushort))
            {
                ushort res;
                if (ushort.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(long))
            {
                long res;
                if (long.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(WPos))
            {
                if (value != null)
                {
                    var parts = value.Split(',');
                    if(parts.Length == 3)
                    {
                        WDist rx, ry, rz;
                        if (WDist.TryParse(parts[0], out rx) && WDist.TryParse(parts[1], out ry) && WDist.TryParse(parts[2], out rz))
                            return new WPos(rx, ry, rz);
                    }
                }
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(CPos))
            {
                if(value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new CPos(Exts.ParseIntegerInvariant(parts[0]), Exts.ParseIntegerInvariant(parts[1]));
                }
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(CVec))
            {
                if(value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new CVec(Exts.ParseIntegerInvariant(parts[0]), Exts.ParseIntegerInvariant(parts[1]));
                }
            }
            return null;
        }

        public static T GetValue<T>(string field,string value)
        {
            return (T)GetValue(field, typeof(T), value, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="my"></param>
        public static void Load(object self,MiniYaml my)
        {
            var loadInfo = TypeLoadInfo[self.GetType()];
            var missing = new List<string>();

            Dictionary<string, MiniYaml> md = null;

            foreach(var fli in loadInfo)
            {
                object val;
                if (md == null)
                    md = my.ToDictionary();
                if(fli.Loader != null)
                {
                    if (!fli.Attribute.Required || md.ContainsKey(fli.YamlName))
                        val = fli.Loader(my);
                    else
                    {
                        missing.Add(fli.YamlName);
                        continue;
                    }
                }
                else
                {
                    if(!TryGetValueFromYaml(fli.YamlName,fli.Field,md,out val))
                    {

                        if (fli.Attribute.Required)
                            missing.Add(fli.YamlName);
                        continue;
                    }
                }
                fli.Field.SetValue(self, val);
            }

            if (missing.Any())
                throw new MissingFieldsException(missing.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void LoadField(object target,string key,string value)
        {
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            key = key.Trim();

            var field = target.GetType().GetField(key, Flags);
            if (field != null)
            {
                var sa = field.GetCustomAttributes<SerializeAttribute>(false).DefaultIfEmpty(SerializeAttribute.Default).First();
                if (!sa.FromYamlKey)
                    field.SetValue(target, GetValue(field.Name, field.FieldType, value, field));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yamlName"></param>
        /// <param name="field"></param>
        /// <param name="md"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        static bool TryGetValueFromYaml(string yamlName,FieldInfo field,Dictionary<string,MiniYaml> md,out object ret)
        {
            ret = null;

            MiniYaml yaml;
            if (!md.TryGetValue(yamlName, out yaml))
                return false;
            ret = GetValue(field.Name, field.FieldType, yaml, field);

            return true;
        }

        public static T Load<T>(MiniYaml y) where T : new()
        {
            var t = new T();
            Load(t, y);
            return t;
        }
    }
}