using System;
using System.Collections;
using System.Collections.Generic;
using EW.Primitives;
namespace EW
{

    public interface ISuppressInitExport { }
    /// <summary>
    /// 
    /// </summary>
    public class ActorReference:IEnumerable
    {
        public string Type;


        /// <summary>
        /// Convert ActorReference field to Lazy Makes LoadMaps 40% faster
        /// </summary>
        Lazy<TypeDictionary> initDict;

        public TypeDictionary InitDict { get { return initDict.Value; } }


        public IEnumerator GetEnumerator() { return InitDict.GetEnumerator(); }


        public ActorReference(string type,Dictionary<string,MiniYaml> inits)
        {
            Type = type;
            initDict = Exts.Lazy(() =>
            {
                var dict = new TypeDictionary();
                foreach(var i in inits)
                {
                    dict.Add(LoadInit(i.Key, i.Value));
                }
                return dict;
            });
        }

        static IActorInit LoadInit(string traitName,MiniYaml my)
        {
            var info = WarGame.CreateObject<IActorInit>(traitName + "Init");
            FieldLoader.Load(info, my);
            return info;
        }
    }
}