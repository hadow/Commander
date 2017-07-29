using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AlwaysVisibleInfo : TraitInfo<AlwaysVisible>
    {

    }
    public class AlwaysVisible:IDefaultVisibility
    {
        public bool IsVisible(Actor self,Player byPlayer)
        {
            return true;
        }
    }
}