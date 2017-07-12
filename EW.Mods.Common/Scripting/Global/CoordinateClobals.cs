using System;
using EW.Scripting;
namespace EW.Mods.Common.Scripting
{
    
    [ScriptGlobal("CPos")]
    public class CPosGlobal : ScriptGlobal
    {
        public CPosGlobal(ScriptContext context) : base(context) { }

        public CPos New(int x,int y) { return new CPos(x, y); }

        public CPos Zero { get { return CPos.Zero; } }

    }

    [ScriptGlobal("CVec")]
    public class CVecGlobal : ScriptGlobal
    {
        public CVecGlobal(ScriptContext context) : base(context) { }

        public CVec New(int x,int y) { return new CVec(x, y); }

        public CVec Zero { get { return CVec.Zero; } }
    }

    [ScriptGlobal("WPos")]
    public class WPosGlobal : ScriptGlobal
    {
        public WPosGlobal(ScriptContext context) : base(context) { }

        public WPos New(int x,int y,int z) { return new WPos(x, y, z); }

        public WPos Zero { get { return WPos.Zero; } }
    }

    [ScriptGlobal("WVec")]
    public class WVecGlobal : ScriptGlobal
    {
        public WVecGlobal(ScriptContext context) : base(context) { }

        public WVec New(int x,int y,int z) { return new WVec(x, y, z); }

        public WVec Zero { get { return WVec.Zero; } }
    }

}