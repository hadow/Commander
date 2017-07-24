using System;
using System.Collections.Generic;
using Eluant;
using Eluant.ObjectBinding;
using EW.Traits;
using EW.Scripting;
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
        public readonly int ClientIndex;
        public readonly PlayerReference PlayerReference;
        public Shroud Shroud;

        public World World { get; private set; }

        public bool IsBot;


        public bool CanViewActor(Actor a)
        {
            return a.CanBeViewedByPlayer(this);
        }
        public Dictionary<Player, Stance> Stances = new Dictionary<Player, Stance>();
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