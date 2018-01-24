using System;
using System.Linq;
using System.Drawing;
using EW.Graphics;
using EW.Framework;

namespace EW.Traits
{
    public class InteractableInfo:ITraitInfo,IMouseBoundsInfo
    {

        public readonly int[] Bounds = null;

        public readonly int[] DecorationBounds = null;



        public virtual object Create(ActorInitializer init) { return new Interactable(this); }
    }

    public class Interactable:INotifyCreated,IMouseBounds
    {
        readonly InteractableInfo info;
        IAutoMouseBounds[] autoBounds;

        public Interactable(InteractableInfo info){
            this.info = info;
        }

        void INotifyCreated.Created(Actor self){

            autoBounds = self.TraitsImplementing<IAutoMouseBounds>().ToArray();
        }

        Rectangle AutoBounds(Actor self,WorldRenderer wr){

            return autoBounds.Select(s => s.AutoMouseoverBounds(self, wr)).FirstOrDefault(r => !r.IsEmpty);

        }

        Rectangle Bounds(Actor self,WorldRenderer wr,int[] bounds){

            if (bounds == null)
                return AutoBounds(self, wr);

            var size = new Int2(bounds[0], bounds[1]);

            var offset = -size / 2;

            if (bounds.Length > 2)
                offset += new Int2(bounds[2], bounds[3]);

            var xy = wr.ScreenPxPosition(self.CenterPosition) + offset;
            return new Rectangle(xy.X, xy.Y, size.X, size.Y);
        }

        Rectangle IMouseBounds.MouseoverBounds(Actor self,WorldRenderer wr){
            return Bounds(self, wr, info.Bounds);
        }

    }
}