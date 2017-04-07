using System;
using System.Collections.Generic;
using EW.Primitives;
namespace EW
{

    public interface IActorInit { }

    public interface IActorInit<T> : IActorInit
    {
        T Value(World world);
    }

    public interface IActorInitializer
    {
        World World { get; }
        T Get<T>() where T : IActorInit;
        U Get<T, U>() where T : IActorInit<U>;
        bool Contains<T>() where T : IActorInit;
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActorInitializer:IActorInitializer
    {
        public readonly Actor Self;
        public World World { get { return Self.World; } }

        internal TypeDictionary Dict;
        public ActorInitializer(Actor actor,TypeDictionary dict)
        {
            Self = actor;
            Dict = dict;
        }

        public T Get<T>() where T : IActorInit
        {
            return Dict.Get<T>();
        }

        public U Get<T,U>() where T : IActorInit<U>
        {
            return Dict.Get<T>().Value(World);
        }

        public bool Contains<T>() where T : IActorInit
        {
            return Dict.Contains<T>();
        }


    }
}