using System;


namespace EW.Traits
{

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UpgradeUsedReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class VoiceReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class WeaponReferenceAttribute : Attribute { }

    public sealed class PaletteDefinitionAttribute : Attribute
    {
        public readonly bool IsPlayerPalette;

        public PaletteDefinitionAttribute(bool isPlayerPalette = false)
        {
            IsPlayerPalette = isPlayerPalette;
        }
    }
}