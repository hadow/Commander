using System;
using System.Collections.Generic;
using EW.Scripting;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptGlobal("Lighting")]
    public class LightingGlobal:ScriptGlobal
    {
        readonly IEnumerable<FlashPaletteEffect> flashPaletteEffects;

        readonly GlobalLightingPaletteEffect lighting;
        public LightingGlobal(ScriptContext context) : base(context)
        {

        }


    }
}