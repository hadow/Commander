using System;


namespace EW.Traits
{
    class LintAttributes
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UpgradeUsedReferenceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class VoiceReferenceAttribute : Attribute { }
}