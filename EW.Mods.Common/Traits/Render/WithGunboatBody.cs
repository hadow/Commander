using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    class WithGunboatBodyInfo : WithSpriteBodyInfo, Requires<BodyOrientationInfo>, Requires<TurretedInfo>
    {

    }
    class WithGunboatBody
    {
    }
}