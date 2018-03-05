using System;
using System.Linq;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    public class DeveloperModeInfo : ITraitInfo
    {

        public readonly bool CheckboxEnabled = false;

        public readonly bool DisableShroud;


        public readonly bool UnlimitedPower;

        public readonly int Cash = 20000;


        public object Create(ActorInitializer init) { return new DeveloperMode(this); }
    }
    public class DeveloperMode:ISync,INotifyCreated,IResolveOrder
    {

        readonly DeveloperModeInfo info;

        public bool Enabled { get; private set; }

        [Sync]bool pathDebug;

        [Sync] bool disableShroud;

        [Sync] bool unlimitedPower;

        [Sync] bool allTech;

        [Sync] bool fastBuild;

        bool enableAll;

        public bool PathDebug { get { return Enabled && pathDebug; } }

        public bool DisableShroud { get { return Enabled && disableShroud; } }

        public bool UnlimitedPower { get { return Enabled && unlimitedPower; } }


        public bool AllTech { get { return Enabled && allTech; } }
        public bool FastBuild { get { return Enabled && fastBuild; } }

        public DeveloperMode(DeveloperModeInfo info){

            this.info = info;
            disableShroud = info.DisableShroud;
            unlimitedPower = info.UnlimitedPower;


        }
        void INotifyCreated.Created(Actor self)
        {
            Enabled = self.World.LobbyInfo.NonBotPlayers.Count() == 1 
                          || self.World.LobbyInfo.GlobalSettings.OptionOrDefault("cheats", info.CheckboxEnabled);
        }

        void IResolveOrder.ResolveOrder(Actor self,Order order){

            if (!Enabled)
                return;

            switch(order.OrderString){

                case "DevAll":
                    {
                        enableAll ^= true;
                        allTech = disableShroud = unlimitedPower = enableAll;


                        if(enableAll){
                            self.Owner.Shroud.ExploreAll();

                            var amount = order.ExtraData != 0 ? (int)order.ExtraData : info.Cash;
                            self.Trait<PlayerResources>().GiveCash(amount);
                        }
                        else
                        {
                            self.Owner.Shroud.ResetExploration();
                        }

                        self.Owner.Shroud.Disabled = DisableShroud;

                        if(self.World.LocalPlayer  == self.Owner){

                            self.World.RenderPlayer = DisableShroud ? null : self.Owner;
                        }

                        break;
                    }
            }
        }

    }
}