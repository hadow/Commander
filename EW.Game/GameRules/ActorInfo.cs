using System;
using System.Collections;
using System.Collections.Generic;
using RA.Primitives;

namespace RA
{
    /// <summary>
    /// 
    /// </summary>
    public class ActorInfo
    {

        public readonly string Name;
        readonly TypeDictionary traits = new TypeDictionary();

        public ActorInfo(ObjectCreator creator,string name,MiniYaml node)
        {
            try
            {
                Name = name;

                var abstractActorT = name.StartsWith("^");
                foreach(var t in node.Nodes)
                {
                    try
                    {
                        traits.Add(LoadTraitInfo(creator, t.Key.Split('@')[0], t.Value));
                    }
                    catch(FieldLoader.MissingFieldsException e)
                    {
                        if (!abstractActorT)
                            throw new Exception(e.Message);
                    }
                }
            }
            catch(Exception e)
            {
                throw new Exception("Actor type {0}: {1}".F(name, e.Message));
            }
        }



        public ActorInfo(string name,params ITraitInfo[] traitInfos)
        {
            Name = name;
            foreach(var t in traitInfos)
            {
                traits.Add(t);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="traitName"></param>
        /// <param name="my"></param>
        /// <returns></returns>
        static ITraitInfo LoadTraitInfo(ObjectCreator creator,string traitName,MiniYaml my)
        {
            if (!string.IsNullOrEmpty(my.Value))
                throw new Exception("junk value '{0}' on trait node {1}".F(my.Value, traitName));

            var info = creator.CreateObject<ITraitInfo>(traitName + "Info");
            try
            {
                FieldLoader.Load(info, my);
            }
            catch(FieldLoader.MissingFieldsException e)
            {
                var header = "Trait name " + traitName + ": " + (e.Missing.Length > 1 ? "Required properties missing" : "Required property missing");
                throw new FieldLoader.MissingFieldsException(e.Missing, header);
            }
            return info;

        }


        public bool HasTraitInfo<T>() where T : ITraitInfoInterface
        {
            return traits.Contains<T>();
        }

        public T TraitInfo<T>() where T : ITraitInfoInterface
        {
            return traits.Get<T>();
        }

        public IEnumerable<T> TraitInfos<T>() where T : ITraitInfoInterface
        {
            return traits.WithInterface<T>();
        }


    }
}