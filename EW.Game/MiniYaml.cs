using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EW.FileSystem;

namespace EW
{
    using MiniYamlNodes = List<MiniYamlNode>;

    public static class MiniYamlExts
    {
        public static IEnumerable<string> ToLines(this MiniYamlNodes y,bool lowest)
        {
            foreach(var kv in y)
            {
                foreach (var line in kv.Value.ToLines(kv.Key))
                    yield return line;

                if (lowest)
                {
                    yield return "";
                }
            }
        }
    }
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

        public MiniYamlNode Clone()
        {
            return new MiniYamlNode(Key, Value);
        }


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

        public MiniYaml Clone()
        {
            return new MiniYaml(Value, Nodes.Select(n => n.Clone()).ToList());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<string> ToLines(string name)
        {
            yield return name + ":" + Value;
            if(Nodes!=null)
            {
                foreach(var line in Nodes.ToLines(false))
                {
                    yield return "\t" + line;
                }
            }
        }

        public Dictionary<string,MiniYaml> ToDictionary()
        {
            return ToDictionary(MiniYamlIdentity);
        }

        public Dictionary<string,TElement> ToDictionary<TElement>(Func<MiniYaml,TElement> elementSelector)
        {
            return ToDictionary(StringIdentity, elementSelector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"></param>
        /// <returns></returns>
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

        public static Dictionary<string,MiniYaml> DictFromFile(string path)
        {
            return FromFile(path).ToDictionary(x => x.Key, x => x.Value);
        }

        public static List<MiniYamlNode> FromFile(string path)
        {
            return FromLines(File.ReadAllLines(path), path);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="files"></param>
        /// <param name="mapRules"></param>
        /// <returns></returns>
        public static List<MiniYamlNode> Load(IReadOnlyFileSystem fileSystem,IEnumerable<string> files,MiniYaml mapRules)
        {
            if(mapRules != null && mapRules.Value != null)
            {
                var mapFiles = FieldLoader.GetValue<string[]>("value", mapRules.Value);
                files = files.Append(mapFiles);

            }

            var yaml = files.Select(s => MiniYaml.FromStream(fileSystem.Open(s), s));
            if (mapRules != null && mapRules.Nodes.Any())
                yaml = yaml.Append(mapRules.Nodes);

            return Merge(yaml);
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

        /// <summary>
        /// ºÏ²¢
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static List<MiniYamlNode> Merge(IEnumerable<List<MiniYamlNode>> sources)
        {
            if (!sources.Any())
                return new List<MiniYamlNode>();

            var tree = sources.Where(s => s != null).Aggregate(MergePartial).ToDictionary(n=>n.Key,n=>n.Value);

            var resolved = new Dictionary<string, MiniYaml>();
            foreach(var kv in tree)
            {
                var inherited = new Dictionary<string, MiniYamlNode.SourceLocation>();
                inherited.Add(kv.Key, new MiniYamlNode.SourceLocation());

                var children = ResolveInherits(kv.Key, kv.Value, tree, inherited);
                resolved.Add(kv.Key, new MiniYaml(kv.Value.Value, children));
            }

            var nodes = new MiniYaml("", resolved.Select(kv => new MiniYamlNode(kv.Key, kv.Value)).ToList());
            
            return ResolveInherits("",nodes,tree,new Dictionary<string, MiniYamlNode.SourceLocation>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="existingNodes"></param>
        /// <param name="overrideNodes"></param>
        /// <returns></returns>
        static List<MiniYamlNode> MergePartial(List<MiniYamlNode> existingNodes,List<MiniYamlNode> overrideNodes)
        {
            if (existingNodes.Count == 0)
                return overrideNodes;

            if (overrideNodes.Count == 0)
                return existingNodes;

            var ret = new List<MiniYamlNode>();

            var existingDict = existingNodes.ToDictionaryWithConflictLog(x => x.Key, "MiniYaml.Merge", null, x => "{0} (at {1})".F(x.Key, x.Location));
            var overrideDict = overrideNodes.ToDictionaryWithConflictLog(x => x.Key, "MiniYaml.Merge", null, x => "{0} (at {1})".F(x.Key, x.Location));

            var allKeys = existingDict.Keys.Union(overrideDict.Keys);

            foreach(var key in allKeys)
            {
                MiniYamlNode existingNode, overrideNode;
                existingDict.TryGetValue(key, out existingNode);
                overrideDict.TryGetValue(key, out overrideNode);

                var loc = overrideNode == null ? default(MiniYamlNode.SourceLocation) : overrideNode.Location;

                var merged = (existingNode == null || overrideNode == null) ? overrideNode ?? existingNode : new MiniYamlNode(key, MergePartial(existingNode.Value, overrideNode.Value),loc);
                ret.Add(merged);
            }

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="existingNodes"></param>
        /// <param name="overrideNodes"></param>
        /// <returns></returns>
        static MiniYaml MergePartial(MiniYaml existingNodes,MiniYaml overrideNodes)
        {
            if (existingNodes == null)
                return overrideNodes;
            if (overrideNodes == null)
                return existingNodes;

            return new MiniYaml(overrideNodes.Value ?? existingNodes.Value, MergePartial(existingNodes.Nodes, overrideNodes.Nodes));
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="node"></param>
        /// <param name="tree"></param>
        /// <param name="inherited"></param>
        /// <returns></returns>
        static List<MiniYamlNode> ResolveInherits(string key,MiniYaml node,
            Dictionary<string,MiniYaml> tree,Dictionary<string,MiniYamlNode.SourceLocation> inherited)
        {
            var resolved = new List<MiniYamlNode>();

            inherited = new Dictionary<string, MiniYamlNode.SourceLocation>(inherited);

            foreach(var n in node.Nodes)
            {
                if(n.Key == "Inherits" || n.Key.StartsWith("Inherits@"))
                {
                    MiniYaml parent;
                    if(!tree.TryGetValue(n.Value.Value,out parent))
                    {
                        throw new YamlException("{0}:Parent type '{1}' not found".F(n.Location, n.Value.Value));
                    }

                    if (inherited.ContainsKey(n.Value.Value))
                        throw new YamlException("{0}:Parent type '{1}' was already inherited by this yaml tree at {2} (note:may be from a derived tree)".F(n.Location,n.Value.Value,inherited[n.Value.Value]));

                    inherited.Add(n.Value.Value, n.Location);
                    foreach (var r in ResolveInherits(n.Key, parent, tree, inherited))
                        MergeIntoResolved(r, resolved, tree, inherited);
                    
                }
                else if (n.Key.StartsWith("-"))
                {
                    var removed = n.Key.Substring(1);
                    if(resolved.RemoveAll(r=>r.Key == removed) == 0)
                    {
                        throw new YamlException("{0}: There are no elements with key '{1}' to remove".F(n.Location, removed));
                    }

                }
                else
                {
                    MergeIntoResolved(n, resolved, tree, inherited);
                }
            }

            return resolved;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="overrideNode"></param>
        /// <param name="existingNodes"></param>
        /// <param name="tree"></param>
        /// <param name="inherited"></param>
        static void MergeIntoResolved(MiniYamlNode overrideNode,List<MiniYamlNode> existingNodes,Dictionary<string,MiniYaml> tree,Dictionary<string,MiniYamlNode.SourceLocation> inherited)
        {
            var existingNode = existingNodes.FirstOrDefault(n => n.Key == overrideNode.Key);
            if (existingNode != null)
            {
                existingNode.Value = MergePartial(existingNode.Value, overrideNode.Value);
            }
            else
            {
                existingNodes.Add(overrideNode.Clone());
            }
        }

    }

    [Serializable]
    public class YamlException : Exception
    {
        public YamlException(string s) : base(s) { }
    }
}