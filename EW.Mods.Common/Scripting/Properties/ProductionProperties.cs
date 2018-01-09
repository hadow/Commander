using EW.Scripting;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Activities;
using Eluant;
using EW.Primitives;
namespace EW.Mods.Common.Scripting
{
    [ScriptPropertyGroup("Production")]
    public class ProductionProperties:ScriptActorProperties,Requires<ProductionInfo>
    {
        readonly Production p;
        public ProductionProperties(ScriptContext context,Actor self) : base(context, self)
        {
            p = self.Trait<Production>();
        }


        /// <summary>
        /// Build a unit,ignoring the production queue.The activity will wait if the exit is blocked.
        /// </summary>
        /// <param name="actorType"></param>
        /// <param name="factionVariant"></param>
        [ScriptActorPropertyActivity]
        public void Produce(string actorType,string factionVariant = null,string productionType = null)
        {
            ActorInfo actorInfo;

            if (!Self.World.Map.Rules.Actors.TryGetValue(actorType, out actorInfo))
                throw new LuaException("Unknown actor type '{0}'".F(actorType));


            var faction = factionVariant ?? BuildableInfo.GetInitialFaction(actorInfo, p.Faction);

            var inits = new TypeDictionary
            {
                new OwnerInit(Self.Owner),
                new FactionInit(faction),
            };
            Self.QueueActivity(new WaitFor(() => p.Produce(Self, actorInfo, productionType,inits)));

        }



    }
}