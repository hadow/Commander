using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Limits the zone where buildings can be constructed to a radius around this actor 
    /// 限制建筑物可以建造的区域
    /// </summary>
    public class BaseProviderInfo : ITraitInfo
    {
        public readonly WDist Range = WDist.FromCells((10));


        public readonly int CoolDown = 0;

        public readonly int InitialDelay;


        public object Create(ActorInitializer init) { return new BaseProvider(init.Self,this); }

    }
    public class BaseProvider:ITick,INotifyCreated,IRenderAboveShroudWhenSelected
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


        public bool Ready(){
            if (building != null && building.Locked)
                return false;

            return progress == 0;
        }


        bool ValidRenderPlayer(){

            return self.Owner == self.World.RenderPlayer || (allyBuildEnabled && self.Owner.IsAlliedWith(self.World.RenderPlayer));
        }
        void INotifyCreated.Created(Actor self){

            building = self.TraitOrDefault<Building>();


        }


        public IEnumerable<IRenderable> RangeCircleRenderables(WorldRenderer wr)
        {


            if (!ValidRenderPlayer())
                yield break;
            yield return new RangeCircleRenderable(self.CenterPosition,
                                                   Info.Range,
                                                   0,
                                                   Color.FromArgb(128, Ready() ? Color.White : Color.Red),
                                                   Color.FromArgb(96, Color.Black));



        }

        IEnumerable<IRenderable> IRenderAboveShroudWhenSelected.RenderAboveShroud(Actor self,WorldRenderer wr){


            return RangeCircleRenderables(wr);

        }



        void ITick.Tick(Actor self){

            if (progress > 0)
                progress--;
        }


        bool IRenderAboveShroudWhenSelected.SpatiallyPartitionable { get { return false; } }
    }
}