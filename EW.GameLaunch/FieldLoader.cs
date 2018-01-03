using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Globalization;
using EW.Primitives;
using EW.Framework;
using EW.Graphics;
using EW.Support;
namespace EW
{
    /// <summary>
    /// 字段反射
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

        /// <summary>
        /// 成员变量是否带有转化属性
        /// </summary>
        static readonly ConcurrentCache<MemberInfo, bool> MemberHasTranslateAttribute = new ConcurrentCache<MemberInfo, bool>(member => member.HasAttribute<TranslateAttribute>());

        public static Func<string, Type, string, object> InvalidValueAction = (s, t, f) =>
           {
               throw new YamlException("FieldLoader: Cannot pars '{0}' into '{1}.{2}'".F(s, f, t));
           };

        public static Action<string, Type> UnknownFieldAction = (s, f) =>
         {
             throw new NotImplementedException("FieldLoader:Missing field '{0}' on '{1}'".F(s, f.Name));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldType"></param>
        /// <param name="yaml"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static object GetValue(string fieldName,Type fieldType,MiniYaml yaml,MemberInfo field)
        {
            var value = yaml.Value;
            if (value != null)
                value = value.Trim();

            if (fieldType == typeof(int))
            {
                int res;
                if (Exts.TryParseIntegerInvariant(value, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(ushort))
            {
                ushort res;
                if (ushort.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(long))
            {
                long res;
                if (long.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(string))
            {
                if (field != null && MemberHasTranslateAttribute[field] && value != null)
                {

                }
                return value;
            }
            else if(fieldType == typeof(WRot))
            {
                if(value != null)
                {
                    var parts = value.Split(',');
                    if(parts.Length == 3)
                    {
                        int rr, rp, ry;
                        if (Exts.TryParseIntegerInvariant(parts[0], out rr) &&
                            Exts.TryParseIntegerInvariant(parts[1], out rp) &&
                            Exts.TryParseIntegerInvariant(parts[2], out ry))
                            return new WRot(new WAngle(rr), new WAngle(rp), new WAngle(ry));
                    }
                }

                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(WAngle))
            {
                int res;
                if (Exts.TryParseIntegerInvariant(value, out res))
                    return new WAngle(res);
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(WVec))
            {
                if(value != null)
                {
                    var parts = value.Split(',');
                    if(parts.Length == 3)
                    {
                        WDist rx, ry, rz;
                        if (WDist.TryParse(parts[0], out rx) && WDist.TryParse(parts[1], out ry) && WDist.TryParse(parts[2], out rz))
                            return new WVec(rx, ry, rz);
                    }
                }
            }
            else if(fieldType == typeof(WVec[]))
            {
                if(value != null)
                {
                    var parts = value.Split(',');
                    if (parts.Length % 3 != 0)
                        return InvalidValueAction(value, fieldType, fieldName);

                    var vecs = new WVec[parts.Length / 3];
                    for(var i = 0; i < vecs.Length; i++)
                    {
                        WDist rx, ry, rz;
                        if (WDist.TryParse(parts[3 * i], out rx) && WDist.TryParse(parts[3 * i + 1], out ry) && WDist.TryParse(parts[3 * i + 2], out rz))
                            vecs[i] = new WVec(rx, ry, rz);

                    }
                    return vecs;
                }

                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(WDist))
            {
                WDist res;
                if (WDist.TryParse(value, out res))
                    return res;
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(WPos))
            {
                if (value != null)
                {
                    var parts = value.Split(',');
                    if (parts.Length == 3)
                    {
                        WDist rx, ry, rz;
                        if (WDist.TryParse(parts[0], out rx) && WDist.TryParse(parts[1], out ry) && WDist.TryParse(parts[2], out rz))
                            return new WPos(rx, ry, rz);
                    }
                }
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(CPos))
            {
                if (value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new CPos(Exts.ParseIntegerInvariant(parts[0]), Exts.ParseIntegerInvariant(parts[1]));
                }
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(CVec))
            {
                if (value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new CVec(Exts.ParseIntegerInvariant(parts[0]), Exts.ParseIntegerInvariant(parts[1]));
                }
            }
            else if (fieldType.IsEnum)
            {
                try
                {
                    return Enum.Parse(fieldType, value, true);
                }
                catch (ArgumentException)
                {
                    return InvalidValueAction(value, fieldType, fieldName);
                }
            }
            else if (fieldType == typeof(Size))
            {
                if (value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new Size(Exts.ParseIntegerInvariant(parts[0]), Exts.ParseIntegerInvariant(parts[1]));
                }
            }
            else if (fieldType.IsArray && fieldType.GetArrayRank() == 1) //获取Rank属性,例如，一维数组返回1，二维数组返回2
            {
                if (value == null)
                    return Array.CreateInstance(fieldType.GetElementType(), 0);
                var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                var ret = Array.CreateInstance(fieldType.GetElementType(), parts.Length);
                for (var i = 0; i < parts.Length; i++)
                {
                    ret.SetValue(GetValue(fieldName, fieldType.GetElementType(), parts[i].Trim(), field), i);
                }
                return ret;
            }
            else if (fieldType == typeof(EW.Framework.Rectangle))
            {
                if (value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new EW.Framework.Rectangle(
                        Exts.ParseIntegerInvariant(parts[0]),
                        Exts.ParseIntegerInvariant(parts[1]),
                        Exts.ParseIntegerInvariant(parts[2]),
                        Exts.ParseIntegerInvariant(parts[3]));
                }
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if (fieldType == typeof(EW.Framework.Point))
            {
                if (value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    return new EW.Framework.Point(Exts.ParseIntegerInvariant(parts[0]), Exts.ParseIntegerInvariant(parts[1]));
                }
            }
            else if (fieldType == typeof(bool))
                return ParseYesNo(value, fieldType, fieldName);
            else if(fieldType == typeof(Vector2))
            {
                if (value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    float xx = 0;
                    float yy = 0;
                    float res;
                    if (float.TryParse(parts[0].Replace("%", ""), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out res))
                        xx = res * (parts[0].Contains('%') ? 0.01f : 1f);
                    if (float.TryParse(parts[1].Replace("%", ""), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out res))
                        yy = res * (parts[1].Contains('%') ? 0.01f : 1f);
                    return new Vector2(xx, yy);
                }
            }
            else if(fieldType == typeof(Vector3))
            {
                if(value != null)
                {
                    var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    float x, y,z = 0;
                    float.TryParse(parts[0], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out x);
                    float.TryParse(parts[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out y);

                    if (parts.Length > 2)
                        float.TryParse(parts[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out z);

                    return new Vector3(x, y, z);
                }
            }
            else if(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var set = Activator.CreateInstance(fieldType);
                if (value == null)
                    return set;

                var parts = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var addMethod = fieldType.GetMethod("Add", fieldType.GetGenericArguments());
                for (var i = 0; i < parts.Length; i++)
                    addMethod.Invoke(set, new[] { GetValue(fieldName, fieldType.GetGenericArguments()[0], parts[i].Trim(), field) });
                return set;
            }
            else if(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var dict = Activator.CreateInstance(fieldType);
                var arguments = fieldType.GetGenericArguments();
                var addMethod = fieldType.GetMethod("Add", arguments);
                foreach(var node in yaml.Nodes)
                {
                    var key = GetValue(fieldName, arguments[0], node.Key, field);
                    var val = GetValue(fieldName, arguments[1], node.Value, field);
                    addMethod.Invoke(dict, new[] { key, val });
                }
                return dict;

            }
            else if(fieldType == typeof(System.Drawing.Color))
            {
                System.Drawing.Color color;
                if (value != null && HSLColor.TryParseRGB(value, out color))
                    return color;

                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(System.Drawing.Color[]))
            {
                if(value != null)
                {
                    var parts = value.Split(',');
                    var colors = new System.Drawing.Color[parts.Length];

                    for(var i = 0; i < parts.Length; i++)
                    {
                        if (!HSLColor.TryParseRGB(parts[i], out colors[i]))
                            return InvalidValueAction(value, fieldType, fieldName);
                    }
                    return colors;
                }
            }
            else if (fieldType == typeof(HSLColor))
            {
                if (value != null)
                {
                    System.Drawing.Color rgb;
                    if (HSLColor.TryParseRGB(value, out rgb))
                        return new HSLColor(rgb);

                    //Allowed old HSLColor / ColorRamp formats to be parsed as HSLColor
                    var parts = value.Split(',');
                    if (parts.Length == 3 || parts.Length == 4)
                        return new HSLColor((byte)Exts.ParseIntegerInvariant(parts[0]).Clamp(0, 255),
                            (byte)Exts.ParseIntegerInvariant(parts[1]).Clamp(0,255),
                            (byte)Exts.ParseIntegerInvariant(parts[2]).Clamp(0,255));
                }

                return InvalidValueAction(value, fieldType, fieldName);
            }
            else if(fieldType == typeof(BooleanExpression))
            {
                if(value != null)
                {
                    try
                    {
                        return new BooleanExpression(value);
                    }
                    catch(InvalidDataException e)
                    {
                        throw new YamlException(e.Message);
                    }
                }
                return InvalidValueAction(value, fieldType, fieldName);
            }
            else
            {
                var conv = TypeDescriptor.GetConverter(fieldType);
                if (conv.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        return conv.ConvertFromInvariantString(value);
                    }
                    catch
                    {
                        return InvalidValueAction(value, fieldType, fieldName);
                    }
                }
            }

            UnknownFieldAction("[Type]{0}".F(value), fieldType);
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
                return;
            }

            var prop = target.GetType().GetProperty(key, Flags);
            if (prop != null)
            {
                var sa = prop.GetCustomAttributes<SerializeAttribute>(false).DefaultIfEmpty(SerializeAttribute.Default).First();
                if (!sa.FromYamlKey)
                {
                    prop.SetValue(target, GetValue(prop.Name, prop.PropertyType, value, prop), null);
                }
                return;
            }

            UnknownFieldAction(key, target.GetType());
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

        static object ParseYesNo(string p,Type fieldType,string field)
        {
            if (string.IsNullOrEmpty(p))
                return InvalidValueAction(p, fieldType, field);

            p = p.ToLowerInvariant();
            if (p == "yes" || p == "true") return true;

            if (p == "no" || p == "false") return false;

            return InvalidValueAction(p, fieldType, field);
        }
    }

    /// <summary>
    /// 转化属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class TranslateAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FieldFromYamlKeyAttribute : FieldLoader.SerializeAttribute
    {
        public FieldFromYamlKeyAttribute()
        {
            FromYamlKey = true;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DictionaryFromYamlKeyAttribute : FieldLoader.SerializeAttribute
    {
        public DictionaryFromYamlKeyAttribute()
        {
            FromYamlKey = true;
            DictionaryFromYamlKey = true;
        }
    }


    [AttributeUsage(AttributeTargets.All)]
    public sealed class DescAttribute : Attribute
    {
        public readonly string[] Lines;

        public DescAttribute(params string[] lines) { Lines = lines; }
    }
}