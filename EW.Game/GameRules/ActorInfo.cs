using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EW.Primitives;

namespace EW
{
    /// <summary>
    /// A unit/building insiide the game. Every rules starts with one and adds trait to it.
    /// </summary>
    public class ActorInfo
    {

        public readonly string Name;
        readonly TypeDictionary traits = new TypeDictionary();
        List<ITraitInfo> constructOrderCache = null;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITraitInfo> TraitsInConstructOrder()
        {
            if (constructOrderCache != null)
                return constructOrderCache;

            var source = traits.WithInterface<ITraitInfo>().Select(i => new
            {
                Trait = i,
                Type = i.GetType(),
                Dependencies = PrerequisitesOf(i).ToList(),
            }).ToList();

            //Dependencies.Any ->true:序列中不包含任何元素 
            var resolved = source.Where(s => !s.Dependencies.Any()).ToList();

            var unresolved = source.Except(resolved);

            var testResolve = new Func<Type, Type, bool>((a, b) => a == b || a.IsAssignableFrom(b));

            var more = unresolved.Where(u => u.Dependencies.All(d => resolved.Exists(r => testResolve(d, r.Type)) &&
                !unresolved.Any(u1 => testResolve(d, u1.Type))));

            while (more.Any())
                resolved.AddRange(more);

            constructOrderCache = resolved.Select(r => r.Trait).ToList();

            return constructOrderCache;
        }

        public static IEnumerable<Type> PrerequisitesOf(ITraitInfo info)
        {
            var interfaces = info.GetType().GetInterfaces();
            return interfaces
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Requires<>))
                .Select(t => t.GetGenericArguments()[0]);
        }


        public bool HasTraitInfo<T>() where T : ITraitInfoInterface
        {
            return traits.Contains<T>();
        }

        public T TraitInfo<T>() where T : ITraitInfoInterface
        {
            return traits.Get<T>();
        }

        public T TraitInfoOrDefault<T>() where T : ITraitInfoInterface
        {
            return traits.GetOrDefault<T>();
        }

        public IEnumerable<T> TraitInfos<T>() where T : ITraitInfoInterface
        {
            return traits.WithInterface<T>();
        }


    }
}