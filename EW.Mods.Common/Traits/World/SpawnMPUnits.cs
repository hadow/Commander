using System;
using System.Linq;
using EW.Traits;
using EW.Graphics;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{

    public class SpawnMPUnitsInfo : ITraitInfo,Requires<MPStartLocationsInfo>,Requires<MPStartUnitsInfo>
    {


        public readonly string StartingUnitsClass = "none";

        public bool Locked = false;
        public object Create(ActorInitializer init) { return new SpawnMPUnits(this); }
    }

    public class SpawnMPUnits:IWorldLoaded
    {
        readonly SpawnMPUnitsInfo info;

        public SpawnMPUnits(SpawnMPUnitsInfo info){

            this.info = info;
        }



        void IWorldLoaded.WorldLoaded(World world,WorldRenderer wr)
        {
            foreach(var s in world.WorldActor.Trait<MPStartLocations>().Start)
            {
                SpawnUnitsForPlayer(world,s.Key,s.Value);
            }
        }


        void SpawnUnitsForPlayer(World w,Player p,CPos sp){

            var spawnClass = p.PlayerReference.StartingUnitsClass ?? w.LobbyInfo.GlobalSettings.OptionOrDefault("startingunits", info.StartingUnitsClass);

            var unitGroup = w.Map.Rules.Actors["world"].TraitInfos<MPStartUnitsInfo>()
                             .Where(g => g.Class == spawnClass && g.Factions != null && g.Factions.Contains(p.Faction.InternalName)).RandomOrDefault(w.SharedRandom);

            if (unitGroup == null)
                throw new InvalidOperationException("No starting units defined for faction {0} with class {1}".F(p.Faction.InternalName, spawnClass));

            if(unitGroup.BaseActor != null){

                w.CreateActor(unitGroup.BaseActor.ToLowerInvariant(),new TypeDictionary(){

                    new LocationInit(sp),
                    new OwnerInit(p),
                    new SkipMakeAnimsInit(),
                    new FacingInit(unitGroup.BaseActorFacing <0 ? w.SharedRandom.Next(256):unitGroup.BaseActorFacing),
                });
            }

            if (!unitGroup.SupportActors.Any())
                return;


            var supportSpawnCells = w.Map.FindTilesInAnnulus(sp, unitGroup.InnerSupportRadius + 1, unitGroup.OuterSupportRadius);

            foreach(var s in unitGroup.SupportActors){

                var actorRules = w.Map.Rules.Actors[s.ToLowerInvariant()];
                var ip = actorRules.TraitInfo<IPositionableInfo>();

                var validCells = supportSpawnCells.Where(c => ip.CanEnterCell(w, null, c));

                if (!validCells.Any())
                    continue;

                var cell = validCells.Random(w.SharedRandom);
                var subCell = ip.SharesCell ? w.ActorMap.FreeSubCell(cell) : 0;

                w.CreateActor(s.ToLowerInvariant(), new TypeDictionary(){
                    new OwnerInit(p),
                    new LocationInit(cell),
                    new SubCellInit(subCell),
                    new FacingInit(unitGroup.SupportActorsFacing < 0 ? w.SharedRandom.Next(256):unitGroup.SupportActorsFacing)
                });
            }

        }
    }
}