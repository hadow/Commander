using System;
using System.Collections.Generic;
namespace EW
{
    public class Arguments
    {
        Dictionary<string, string> args = new Dictionary<string, string>();

        public static Arguments Empty { get { return new Arguments(); } }


        public Arguments(params string[] src)
        {

        }
        
        public bool Contains(string key) { return args.ContainsKey(key); }

        public string GetValue(string key,string defaultValue)
        {
            return Contains(key) ? args[key] : defaultValue;
        }

        public void ReplaceValue(string key,string value)
        {
            args[key] = value;
        }

    }
}