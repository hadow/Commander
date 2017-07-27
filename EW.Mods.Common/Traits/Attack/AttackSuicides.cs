using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class AttackSuicidesInfo : ITraitInfo, Requires<IMoveInfo>
    {
        


    }

    class AttackSuicides:IIssueOrder,IResolveOrder,IOrderVoice
    {


    }
}