﻿using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using EW.Traits;
using EW.Primitives;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    public interface IObservesVariablesInfo : ITraitInfo
    {

    }

	public interface IRenderActorPreviewInfo : ITraitInfo
	{
        IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init);
	}



    public interface IConditionConsumerInfo:ITraitInfo{}

    public interface IDeathActorInitModifier
    {
        void ModifyDeathActorInit(Actor self, TypeDictionary inits);
    }

    public interface IActorPreviewInitModifier
    {
        void ModifyActorPreviewInit(Actor self, TypeDictionary inits);
    }

    public interface INotifyAttack { void Attacking(Actor self, Target target, Armament a, Barrel barrel); }


    public interface IAcceptResourcesInfo : ITraitInfo { }

    public interface IQuantizeBodyOrientationInfo : ITraitInfo
    {
        int QuantizedBodyFacings(ActorInfo ai, SequenceProvider sequenceProvider, string race);
    }


    public delegate void VariableObserverNotifier(Actor self, IReadOnlyDictionary<string, int> variables);

    public struct VariableObserver
    {
        public VariableObserverNotifier Notifier;

        public IEnumerable<string> Variables;

        public VariableObserver(VariableObserverNotifier notifier,IEnumerable<string> variables)
        {
            Notifier = notifier;
            Variables = variables;
        }
    }

}
