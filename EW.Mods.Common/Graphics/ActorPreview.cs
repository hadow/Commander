﻿using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Graphics;
using EW.Primitives;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Graphics
{

    public interface IActorPreview{
        void Tick();
        IEnumerable<IRenderable> Render(WorldRenderer wr, WPos pos);

        IEnumerable<Rectangle> ScreenBounds(WorldRenderer wr, WPos pos);
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActorPreviewInitializer:IActorInitializer
    {

        public readonly ActorInfo Actor;

        public readonly WorldRenderer WorldRenderer;
        public World World { get { return WorldRenderer.World; }}

        readonly TypeDictionary dict;
        public ActorPreviewInitializer(ActorInfo actor,WorldRenderer worldRenderer,TypeDictionary dict){

            this.Actor = actor;
            this.WorldRenderer = worldRenderer;
            this.dict = dict;
        }

        public T Get<T>() where T : IActorInit { return dict.Get<T>(); }

        public U Get<T, U>() where T : IActorInit<U> { return dict.Get<T>().Value(World); }

        public bool Contains<T>() where T : IActorInit { return dict.Contains<T> (); }

        public DamageState GetDamageState()
        {
            var health = dict.GetOrDefault<HealthInit>();
            if (health == null)
                return DamageState.Undamaged;

            var hf = health.Value(null);
            if (hf <= 0)
                return DamageState.Dead;
            if (hf < 0.25f)
                return DamageState.Critical;
            if (hf < 0.5f)
                return DamageState.Heavy;

            if (hf < 0.75f)
                return DamageState.Medium;

            if (hf < 1.0f)
                return DamageState.Light;
            return DamageState.Undamaged;
        }

        public Func<WRot> GetOrientation()
        {
            var facingInfo = Actor.TraitInfoOrDefault<IFacingInfo>();
            if (facingInfo == null)
                return () => WRot.Zero;

            // Dynamic facing takes priority
            var dynamicInit = dict.GetOrDefault<DynamicFacingInit>();
            if(dynamicInit != null)
            {
                var getFacing = dynamicInit.Value(null);
                return () => WRot.FromFacing(getFacing());
            }

            var facingInit = dict.GetOrDefault<FacingInit>();
            var facing = facingInit != null ? facingInit.Value(null) : facingInfo.GetInitialFacing();
            var orientation = WRot.FromFacing(facing);
            return () => orientation;
        }
    }

}
