using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WithTurretedSpriteBodyInfo : WithSpriteBodyInfo, Requires<TurretedInfo>, Requires<BodyOrientationInfo>
    {

    }
    class WithTurretedSpriteBody
    {
    }
}