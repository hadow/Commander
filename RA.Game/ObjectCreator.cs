using System;
using System.Collections.Generic;
using System.Reflection;
using RA.Game.FileSystem;
using RA.Game.Primitives;
using System.Linq;
namespace RA.Game
{
    public class ObjectCreator
    {


        readonly Pair<Assembly, string>[] assemblies;

        public ObjectCreator(Assembly a)
        {

            assemblies = a.GetNamespaces().Select(ns => Pair.New(a, ns)).ToArray();
        }

        public ObjectCreator(Manifest manifest,FileSystem.FileSystem modeFiles)
        {



        }


        public IEnumerable<Type> GetTypesImplementing<T>()
        {
            var it = typeof(T);
            return GetTypes().Where(t => t != it && it.IsAssignableFrom(t));
        }


        public IEnumerable<Type> GetTypes()
        {
            return assemblies.Select(ma => ma.First).Distinct().SelectMany(ma => ma.GetTypes());
        }
    }
}