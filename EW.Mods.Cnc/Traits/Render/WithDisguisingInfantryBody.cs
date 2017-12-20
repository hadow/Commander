using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{
    public class WithDisguisingInfantryBodyInfo:WithInfantryBodyInfo,Requires<DisguiseInfo>
    {


    }


    class WithDisguisingInfantryBody : WithInfantryBody
    {
        public WithDisguisingInfantryBody(ActorInitializer init,WithDisguisingInfantryBodyInfo info):base(init,info)
        {

        }
    }
}