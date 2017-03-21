using System;
using System.Collections;
using System.Collections.Generic;
namespace RA
{


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





    }


    /// <summary>
    /// 
    /// </summary>
    public class MiniYaml
    {

        public string Value;
        public List<MiniYamlNode> Nodes;


        public MiniYaml(string value):this(value,null) { }

        public MiniYaml(string value,List<MiniYamlNode> nodes)
        {
            Value = value;
            Nodes = nodes ?? new List<MiniYamlNode>();
        }
    }
}