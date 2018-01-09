using System;


namespace EW.Traits
{
    public class SelectableInfo : InteractableInfo
    {
        public readonly int Priority = 10;

        
        public readonly int[] Bounds = null;

        public readonly string Class = null;

        [VoiceReference]
        public readonly string Voice = "Select";

        public override  object Create(ActorInitializer init) { return new Selectable(init.Self, this); }

    }
    public class Selectable:Interactable
    {
        public readonly string Class = null;

        public SelectableInfo Info;

        public Selectable(Actor self,SelectableInfo info)
        {
            Class = string.IsNullOrEmpty(info.Class) ? self.Info.Name : info.Class;
            Info = info;
        }
    }
}