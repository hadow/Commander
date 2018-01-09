using System;
using System.Linq;
using EW.Graphics;
using EW.Traits;

namespace EW.Traits
{
    public class InteractableInfo:ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new Interactable(); }
    }

    public class Interactable
    {

    }
}