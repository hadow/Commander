using System;
using System.Collections.Generic;
using System.Linq;
using Eluant;
using Eluant.ObjectBinding;
using EW.Traits;
using EW.Scripting;
using EW.NetWork;
using EW.Primitives;
namespace EW
{
    public enum WinState { Undefined,Won,Lost}

    public enum PowerState { Normal,Low,Critical}


    public class Player:IScriptBindable,IScriptNotifyBind,ILuaTableBinding,ILuaEqualityBinding,ILuaToStringBinding
    {
        public WinState WinState = WinState.Undefined;
       

        public readonly Actor PlayerActor;

        public readonly string PlayerName;
        public readonly string InternalName;
        public readonly bool Playable = true;
        public readonly bool NonCombatant = false;
        public readonly int ClientIndex;
        public readonly PlayerReference PlayerReference;
        public readonly FactionInfo Faction;
        public readonly FactionInfo DisplayFaction;
        public Shroud Shroud;

        public World World { get; private set; }

        public bool IsBot;
        public bool Spectating; //旁观
        public bool HasObjectives = false;

        readonly IFogVisibilityModifier[] fogVisibilities; 
        public bool CanViewActor(Actor a)
        {
            return a.CanBeViewedByPlayer(this);
        }
        public Dictionary<Player, Stance> Stances = new Dictionary<Player, Stance>();


        public Player(World world,Session.Client client,PlayerReference pr)
        {
            string botT;

            World = world;
            InternalName = pr.Name;
            PlayerReference = pr;

            if(client != null)
            {

            }
            else
            {
                //Map player
                ClientIndex = 0;

                PlayerName = pr.Name;
                NonCombatant = pr.NonCombatant;
                Playable = pr.Playable;
                Spectating = pr.Spectating;
                botT = pr.Bot;
                Faction = ChooseFaction(world, pr.Faction, false);
                DisplayFaction = ChooseDisplayFaction(world, pr.Faction);
            }

            PlayerActor = world.CreateActor("Player", new TypeDictionary() { new OwnerInit(this)});
            Shroud = PlayerActor.Trait<Shroud>();
            fogVisibilities = PlayerActor.TraitsImplementing<IFogVisibilityModifier>().ToArray();
        }

        /// <summary>
        /// 选择派系
        /// </summary>
        /// <param name="world"></param>
        /// <param name="name"></param>
        /// <param name="requiresSelectable"></param>
        /// <returns></returns>
        static FactionInfo ChooseFaction(World world,string name,bool requiresSelectable = true)
        {
            var selectableFactions = world.Map.Rules.Actors["world"].TraitInfos<FactionInfo>().Where(f => !requiresSelectable || f.Selectable).ToList();

            var selected = selectableFactions.FirstOrDefault(f => f.InternalName == name) ?? selectableFactions.Random(world.SharedRandom);

            for(var i = 0; i <= 10 && selected.RandomFactionMembers.Any(); i++)
            {
                var faction = selected.RandomFactionMembers.Random(world.SharedRandom);

                selected = selectableFactions.FirstOrDefault(f => f.InternalName == faction);

                if (selected == null)
                    throw new YamlException("Unknown faction:{0}".F(faction));
            }

            return selected;

        }

        static FactionInfo ChooseDisplayFaction(World world,string factionName)
        {
            var factions = world.Map.Rules.Actors["world"].TraitInfos<FactionInfo>().ToArray();

            return factions.FirstOrDefault(f => f.InternalName == factionName) ?? factions.First();
        }



        #region Scripting interface

        Lazy<ScriptPlayerInterface> luaInterface;

        public void OnScriptBind(ScriptContext context)
        {
            if (luaInterface == null)
                luaInterface = Exts.Lazy(() => new ScriptPlayerInterface(context, this));
        }

        public LuaValue this[LuaRuntime runtime,LuaValue keyValue]
        {
            get { return luaInterface.Value[runtime, keyValue]; }
            set
            {
                luaInterface.Value[runtime, keyValue] = value;
            }
        }

        public LuaValue Equals(LuaRuntime runtime,LuaValue left,LuaValue right)
        {
            Player a, b;
            if (!left.TryGetClrValue(out a) || !right.TryGetClrValue(out b))
                return false;

            return a == b;
        }

        public LuaValue ToString(LuaRuntime runtime)
        {
            return "Player ({0})".F(PlayerName);
        }

        #endregion
    }
}