﻿using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using EW.Traits;
using EW.Primitives;
using EW.Graphics;
using EW.Mods.Common.Traits.Render;
namespace EW.Mods.Common.Traits
{
    public interface ICrushable
    {
        bool CrushableBy(Actor self, Actor crusher, HashSet<string> crushClasses);
    }
    public interface IObservesVariables
    {
        IEnumerable<VariableObserver> GetVariableObservers();
    }
    public interface IObservesVariablesInfo : ITraitInfo
    {

    }

	public interface IRenderActorPreviewInfo : ITraitInfo
	{
        IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init);
	}

    public interface IRenderActorPreviewVoxelsInfo : ITraitInfo
    {
        IEnumerable<ModelAnimation> RenderPreviewVoxels(ActorPreviewInitializer init, RenderVoxelsInfo rv, string image, Func<WRot> orientation, int facings, PaletteReference p);
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

    /// <summary>
    /// 观察变量
    /// </summary>
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
