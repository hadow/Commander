using System;
using EW.Scripting;

namespace EW.Common.Scripting.Global
{
    [ScriptGlobal("Camera")]
    public class CameraGlobal:ScriptGlobal
    {

        public CameraGlobal(ScriptContext context) : base(context) { }

        /// <summary>
        /// The center of the visible viewport.
        /// </summary>
        public WPos Position
        {
            get
            {
                Console.WriteLine("get Position:" + Context.WorldRenderer.ViewPort.CenterPosition.ToString());
                return Context.WorldRenderer.ViewPort.CenterPosition;
            }
            set
            {
                Context.WorldRenderer.ViewPort.Center(value);
                Console.Write("set Position:" + value.ToString());
            }
        }
    }
}