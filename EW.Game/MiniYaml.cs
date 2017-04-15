using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EW.FileSystem;
namespace EW
{

    /// <summary>
    /// 
    /// </summary>
    public class MiniYamlNode
    {
        public struct SourceLocation
        {
            public string Filename;
            public int Line;
            public override string ToString()
            {
                return "{0}:{1}".F(Filename, Line);
            }
        }

        public SourceLocation Location;
        public string Key;
        public MiniYaml Value;

        public MiniYamlNode(string k,MiniYaml v)
        {
            Key = k;
            Value = v;
        }

        public MiniYamlNode(string k,MiniYaml v,SourceLocation loc):this(k,v)
        {
            Location = loc;
        }
        
        public MiniYamlNode(string k,string v) : this(k, v, null) { }

        public MiniYamlNode(string k,string v,List<MiniYamlNode> n):this(k,new MiniYaml(v, n)) { }
        
        public MiniYamlNode(string k, string v, List<MiniYamlNode> n, SourceLocation location):this(k,new MiniYaml(v, n), location) { }

    }


    /// <summary>
    /// 
    /// </summary>
    public class MiniYaml
    {
        const int SpacesPerLevel = 4;
        public string Value;
        public List<MiniYamlNode> Nodes;

        static readonly Func<MiniYaml, MiniYaml> MiniYamlIdentity = my => my;
        static readonly Func<string, string> StringIdentity = s => s;
        public MiniYaml(string value):this(value,null) { }

        public MiniYaml(string value,List<MiniYamlNode> nodes)
        {
            Value = value;
            Nodes = nodes ?? new List<MiniYamlNode>();
        }

        public Dictionary<string,MiniYaml> ToDictionary()
        {
            return ToDictionary(MiniYamlIdentity);
        }

        public Dictionary<string,TElement> ToDictionary<TElement>(Func<MiniYaml,TElement> elementSelector)
        {
            return ToDictionary(StringIdentity, elementSelector);
        }

        public Dictionary<TKey,TElement> ToDictionary<TKey,TElement>(Func<string,TKey> keySelector,Func<MiniYaml,TElement> elementSelector)
        {
            var ret = new Dictionary<TKey, TElement>();
            foreach(var y in Nodes)
            {
                var key = keySelector(y.Key);
                var element = elementSelector(y.Value);
                try
                {
                    ret.Add(key, element);
                }
                catch(ArgumentException ex)
                {
                    throw new InvalidDataException("Duplicate Key '{0}' in {1}".F(y.Key, y.Location), ex);
                }
            }
            return ret;
        }
        

        static List<MiniYamlNode> FromLines(IEnumerable<string> lines,string filename)
        {
            var levels = new List<List<MiniYamlNode>>();
            levels.Add(new List<MiniYamlNode>());

            var lineNo = 0;

            foreach(var ll in lines)
            {
                var line = ll;
                ++lineNo;

                //×¢ÊÍÐÐ
                var commentIndex = line.IndexOf('#');
                if (commentIndex != -1)
                    line = line.Substring(0, commentIndex).TrimEnd(' ','\t');
                if (line.Length == 0)
                    continue;

                var charPosition = 0;
                var level = 0;
                var spaces = 0;
                var textStart = false;
                var currChar = line[charPosition];

                while (!(currChar =='\n' || currChar == '\r') && charPosition < line.Length && !textStart)
                {
                    currChar = line[charPosition];
                    switch (currChar)
                    {
                        case ' ':
                            spaces++;
                            if(spaces>= SpacesPerLevel)
                            {
                                spaces = 0;
                                level++;
                            }
                            charPosition++;
                            break;
                        case '\t':
                            level++;
                            charPosition++;
                            break;
                        default:
                            textStart = true;
                            break;
                    }
                }
                var realText = line.Substring(charPosition);
                if (realText.Length == 0)
                    continue;

                var location = new MiniYamlNode.SourceLocation { Filename = filename, Line = lineNo };
                if (levels.Count <= level)
                    throw new YamlException("Bad indent in miniyaml at {0}".F(location));

                while (levels.Count > level + 1)
                    levels.RemoveAt(levels.Count - 1);

                var d = new List<MiniYamlNode>();
                var rhs = SplitAtColon(ref realText);
                levels[level].Add(new MiniYamlNode(realText,rhs,d,location));

                levels.Add(d);
            }
            return levels[0];
        }


        static string SplitAtColon(ref string realText)
        {
            var colon = realText.IndexOf(':');
            if (colon == -1)
                return null;

            var ret = realText.Substring(colon + 1).Trim();
            if (ret.Length == 0)
                ret = null;
            realText = realText.Substring(0, colon).Trim();
            return ret;
        }

        public static List<MiniYamlNode> Load(IReadOnlyFileSystem fileSystem,IEnumerable<string> files,MiniYaml mapRules)
        {
            if(mapRules != null && mapRules.Value != null)
            {
                var mapFiles = FieldLoader.GetValue<string[]>("value", mapRules.Value);
                files = files.Append(mapFiles);

            }
        }

        public static List<MiniYamlNode> FromString(string text,string fileName="<no filename available>")
        {
            return FromLines(text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None), fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<MiniYamlNode> FromStream(Stream s,string fileName="<no filename available>")
        {
            using (var reader = new StreamReader(s))
                return FromString(reader.ReadToEnd(),fileName);
        }

        public static Dictionary<string,MiniYaml> DictFromStream(Stream stream,string fileName = "<no filename available>")
        {
            return FromStream(stream, fileName).ToDictionary(x=>x.Key,x=>x.Value);
        }

        public static List<MiniYamlNode> NodesOrEmpty(MiniYaml y,string s)
        {
            var nd = y.ToDictionary();
            return nd.ContainsKey(s) ? nd[s].Nodes : new List<MiniYamlNode>();
        }
    }

    [Serializable]
    public class YamlException : Exception
    {
        public YamlException(string s) : base(s) { }
    }
}