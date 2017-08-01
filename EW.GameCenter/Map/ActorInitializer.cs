using System;
using System.Linq;
using System.Collections.Generic;
using EW.Primitives;
namespace EW
{

    public interface IActorInit { }

    public interface IActorInit<T> : IActorInit
    {
        T Value(World world);
    }

    /// <summary>
    /// Actor initializer.
    /// </summary>
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
    public class ActorInitializer : IActorInitializer
    {
        public readonly Actor Self;
        public World World { get { return Self.World; } }

        internal TypeDictionary Dict;
        public ActorInitializer(Actor actor, TypeDictionary dict)
        {
            Self = actor;
            Dict = dict;
        }

        public T Get<T>() where T : IActorInit
        {
            return Dict.Get<T>();
        }

        public U Get<T, U>() where T : IActorInit<U>
        {
            return Dict.Get<T>().Value(World);
        }

        public bool Contains<T>() where T : IActorInit
        {
            return Dict.Contains<T>();
        }



    }



    /// <summary>
    /// 
    /// </summary>
    public class LocationInit : IActorInit<CPos>
    {
        [FieldFromYamlKey]
        readonly CPos value = CPos.Zero;
        public LocationInit() { }

        public LocationInit(CPos init) { value = init; }

        public CPos Value(World world) { return value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OwnerInit : IActorInit<Player>
    {

        [FieldFromYamlKey]
        public readonly string PlayerName = "Neutral";
        Player player;

        public OwnerInit() { }

        public OwnerInit(string playerName)
        {
            PlayerName = playerName;
        }

        public OwnerInit(Player player)
        {
            this.player = player;
            PlayerName = player.InternalName;
        }

        public Player Value(World world)
        {
            if (player != null)
                return player;

            return world.Players.First(x => x.InternalName == PlayerName);
        }
             
    }
}