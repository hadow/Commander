using System;
using System.Collections;
using System.Collections.Generic;
using EW.Graphics;
using EW.Primitives;
namespace EW.Mods.Common.Graphics
{

    public interface IActorPreview{
        void Tick();
        IEnumerable<IRenderable> Render(WorldRenderer wr, WPos pos);
    }

    public class ActorPreviewInitializer:IActorInitializer{
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

        
    }

}
