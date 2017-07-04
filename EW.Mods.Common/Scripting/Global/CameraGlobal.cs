using System;
using EW.Scripting;

namespace EW.Common.Scripting.Global
{
    [ScriptGlobal("Camera")]
    public class CameraGlobal:ScriptGlobal
    {

        public CameraGlobal(ScriptContext context) : base(context) { }

        public WPos Position
        {
            get { return Context.WorldRenderer.ViewPort.CenterPosition; }
            set
            {
                Context.WorldRenderer.ViewPort.Center(value);
            }
        }
    }
}