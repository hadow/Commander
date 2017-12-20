using System;
using EW.Scripting;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Activities;
using EW.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptPropertyGroup("Movement")]
    public class MobileProperties:ScriptActorProperties,Requires<MobileInfo>
    {
        readonly Mobile mobile;

        public MobileProperties(ScriptContext context,Actor self) : base(context, self)
        {
            mobile = self.Trait<Mobile>();
        }

        /// <summary>
        /// Moves within the cell grid,ignoring lane biases.
        /// </summary>
        /// <param name="cell"></param>
        [ScriptActorPropertyActivity]
        public void ScriptedMove(CPos cell)
        {
            //Console.WriteLine("Scripted Move");
            Self.QueueActivity(new Move(Self, cell));
        }

    }
}