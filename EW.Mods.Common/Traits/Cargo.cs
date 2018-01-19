using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public interface INotifyPassengerEntered { void OnPassengerEntered(Actor self, Actor passenger); }

    public interface INotifyPassengerExited { void OnPassengerExited(Actor self, Actor passenger); }
    public class CargoInfo : ITraitInfo, Requires<IOccupySpaceInfo>
    {
        public readonly string[] LoadingUpgrades = { };

        public object Create(ActorInitializer init) { return new Cargo(init, this); }
    }
    public class Cargo
    {
        public readonly CargoInfo Info;
        readonly Stack<Actor> cargo = new Stack<Actor>();
        readonly HashSet<Actor> reserves = new HashSet<Actor>();
        readonly UpgradeManager upgradeManager;

        int totalWeight = 0;
        int reservedWeight = 0;

        CPos currentCell;
        bool initialized;
        public Cargo(ActorInitializer init,CargoInfo info)
        {
            Info = info;
        }

        public void Load(Actor self,Actor a)
        {
            cargo.Push(a);
            var w = GetWeight(a);
            totalWeight += w;
            if (reserves.Contains(a))
            {
                reservedWeight -= w;
                reserves.Remove(a);
                if (reserves.Count == 0)
                    foreach (var u in Info.LoadingUpgrades)
                        upgradeManager.RevokeUpgrade(self, u, this);
            }

            if (initialized)
                foreach (var npe in self.TraitsImplementing<INotifyPassengerEntered>())
                    npe.OnPassengerEntered(self, a);

            var p = a.Trait<Passenger>();
            p.Transport = self;
            foreach (var u in p.Info.GrantUpgrades)
                upgradeManager.GrantUpgrade(self, u, p);

            Console.WriteLine("Cargo Line:55");
        }

        public Actor Unload(Actor self)
        {
            var a = cargo.Pop();
            return a;
        }

        static int GetWeight(Actor a)
        {
            return a.Info.TraitInfo<PassengerInfo>().Weight;
        }
    }
}