using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class BaseProviderInfo : ITraitInfo
    {
        public readonly WDist Range = WDist.FromCells((10));


        public readonly int CoolDown = 0;

        public readonly int InitialDelay;


        public object Create(ActorInitializer init) { return new BaseProvider(init.Self,this); }

    }
    public class BaseProvider:ITick,INotifyCreated
    {

        public readonly BaseProviderInfo Info;

        readonly Actor self;

        Building building;


        int total;

        int progress;

        bool allyBuildEnabled;

        public BaseProvider(Actor self, BaseProviderInfo info)
        {
            Info = info;
            this.self = self;

            progress = total = info.InitialDelay;

            allyBuildEnabled = self.World.WorldActor.Trait<MapBuildRadius>().AllyBuildRadiusEnabled;
        }

        void INotifyCreated.Created(Actor self){

            building = self.TraitOrDefault<Building>();


        }


        void ITick.Tick(Actor self){

            if (progress > 0)
                progress--;
        }
    }
}