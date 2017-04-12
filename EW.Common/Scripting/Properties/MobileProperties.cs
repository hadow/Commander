using System;
using EW.Scripting;
using EW.Mods.Common.Traits;
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



    }
}