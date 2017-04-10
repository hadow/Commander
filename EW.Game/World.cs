using System;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;
using EW.Primitives;
namespace EW
{
    public enum WorldT
    {
        Regular,
        Shellmap,
        Editor,
    }
    /// <summary>
    /// 虚拟战争世界
    /// </summary>
    public sealed class World:IDisposable
    {
        public readonly Actor WorldActor;
        public readonly Map Map;
        
        public readonly ActorMap ActorMap;

        internal readonly TraitDictionary TraitDict = new TraitDictionary();
        internal readonly OrderManager OrderManager;
        uint nextAID = 0;


        internal World(Map map,OrderManager orderManager,WorldT type)
        {

        }

        public Actor CreateActor(string name,TypeDictionary initDict)
        {

        }

        public Actor CreateActor(bool addToWorld,string name,TypeDictionary initDict)
        {
            var a = new Actor(this, name, initDict);
            foreach(var t in a.tra)
        }



        internal uint NextAID()
        {
            return nextAID++;
        }

        public void Dispose()
        {
        }
    }
}