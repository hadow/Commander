using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class AttackWanderInfo : WandersInfo, Requires<AttackMoveInfo>
    {

    }
    class AttackWander:Wanders
    {

    }
}