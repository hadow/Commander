using System;


namespace EW.Traits
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UpgradeGrantedReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UpgradeUsedReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class VoiceReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class WeaponReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PaletteDefinitionAttribute : Attribute
    {
        public readonly bool IsPlayerPalette;

        public PaletteDefinitionAttribute(bool isPlayerPalette = false)
        {
            IsPlayerPalette = isPlayerPalette;
        }
    }


    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SequenceReferenceAttribute : Attribute
    {
        public readonly string ImageReference;
        public readonly bool Prefix;

        public SequenceReferenceAttribute(string imageReference = null,bool prefix = false)
        {
            ImageReference = imageReference;
            Prefix = prefix;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PaletteReferenceAttribute : Attribute
    {
        public readonly bool IsPlayerPalette;

        public PaletteReferenceAttribute(bool isPlayerPalette = false)
        {
            IsPlayerPalette = isPlayerPalette;
        }

        public readonly string PlayerPaletteReferenceSwitch;

        public PaletteReferenceAttribute(string playerPaletteReferenceSwitch)
        {
            PlayerPaletteReferenceSwitch = playerPaletteReferenceSwitch;
        }
    }
}