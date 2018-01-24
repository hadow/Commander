using System;
using System.Collections.Generic;
using System.Linq;
namespace EW.Widgets
{
    public static class ChromeMetrics
    {
        static Dictionary<string, string> data = new Dictionary<string, string>();

        public static void Initialize(ModData modData){

            data = new Dictionary<string, string>();

            var metrics = MiniYaml.Merge(modData.Manifest.ChromeMetrics.Select(y => MiniYaml.FromStream(modData.DefaultFileSystem.Open(y), y)));

            foreach(var m in metrics){
                foreach (var n in m.Value.Nodes)
                    data[n.Key] = n.Value.Value;
            }
        }


        public static T Get<T>(string key){

            return FieldLoader.GetValue<T>(key, data[key]);
        }

        public static bool TryGet<T>(string key,out T result){

            string s;
            if(!data.TryGetValue(key,out s)){
                result = default(T);
                return false;
            }

            result = FieldLoader.GetValue<T>(key, s);
            return true;
        }
    }
}
