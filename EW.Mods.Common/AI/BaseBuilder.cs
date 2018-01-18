using System;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.AI
{
    public class BaseBuilder
    {
        readonly string category;
        readonly HackyAI ai;
        readonly World world;
        readonly Player player;
        readonly PowerManager playerPower;
        readonly PlayerResources playerResources;

        int waitTicks;
        Actor[] playerBuildings;
        int failCount;
        int failRetryTicks;
        int checkForBaseTicks;
        int cachedBases;
        int cachedBuildings;


        public BaseBuilder(HackyAI ai,string category,Player p,PowerManager pm,PlayerResources pr)
        {
            this.ai = ai;
            this.category = category;
            this.player = p;
            this.playerPower = pm;
            this.playerResources = pr;
            this.category = category;
            failRetryTicks = ai.Info.StructureProductionResumeDelay;

        }

        public void Tick(){


        }
    }
}
