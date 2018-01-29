using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using EW.Framework.Touch;
using EW.Widgets;
using EW.Graphics;
using EW.Framework;
using EW.Traits;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Widgets
{
    public class WorldInteractionControllerWidget:Widget
    {
        protected readonly World World;
        readonly WorldRenderer worldRenderer;

        Int2 dragStart, mousePos;

        bool isDragging = false;

        bool IsValidDragBox{
            get{
                return isDragging && (dragStart - mousePos).Length > WarGame.Settings.Game.SelectionDeadzone;

            }
        }

        [ObjectCreator.UseCtor]
        public WorldInteractionControllerWidget(World world,WorldRenderer worldRenderer)
        {
            this.worldRenderer = worldRenderer;
            this.World = world;

        }


        public override bool HandleInput(GestureSample gs)
        {

            mousePos = worldRenderer.ViewPort.ViewToWorldPx(gs.Position.ToInt2());
            if(!isDragging && gs.GestureType == GestureType.FreeDrag){

                if (!TakeFocus(gs))
                    return false;

                dragStart = mousePos;
                isDragging = true;
            }

            if(gs.GestureType == GestureType.DragComplete){

                isDragging = false;
                YieldFocus(gs);
            }

            return true;

        }


        public override void Draw()
        {

            if(IsValidDragBox){

                var a = new Vector3(dragStart.X, dragStart.Y, dragStart.Y);
                var b = new Vector3(mousePos.X, mousePos.Y, mousePos.Y);

                WarGame.Renderer.WorldRgbaColorRenderer.DrawRect(a,b,1/worldRenderer.ViewPort.Zoom,Color.White);

                foreach (var u in SelectActorsInBoxWithDeadzone(World, dragStart, mousePos))
                    DrawRollover(u);
            }
            else{
                
            }
        }

        void DrawRollover(Actor unit){

            if(unit.Info.HasTraitInfo<SelectableInfo>()){

                var bounds = unit.TraitsImplementing<IDecorationBounds>()
                    .Select(b => b.DecorationBounds(unit, worldRenderer))
                    .FirstOrDefault(b => !b.IsEmpty);

                new SelectionBarsRenderable(unit, bounds, true, true).Render(worldRenderer);
            }


        }

        static IEnumerable<Actor> SelectHighestPriorityActorAtPoint(World world,Int2 a){

            var selected = world.ScreenMap.ActorsAtMouse(a)
                                .Where(x => x.Actor.Info.HasTraitInfo<SelectableInfo>() && (x.Actor.Owner.IsAlliedWith(world.RenderPlayer) || !world.FogObscures(x.Actor)))
                                .WithHighestSelectionPriority(a);
            if (selected != null)
                yield return selected;
            
        }


        static IEnumerable<Actor> SelectActorsInBoxWithDeadzone(World world,Int2 a,Int2 b){


            //For dragboxes that are too small,shrink the dragbox to a single point

            if ((a - b).Length <= WarGame.Settings.Game.SelectionDeadzone)
                a = b;

            if (a == b)
                return SelectHighestPriorityActorAtPoint(world, a);

            return world.ScreenMap.ActorsInMouseBox(a, b)
                        .Select(x => x.Actor)
                        .Where(x => x.Info.HasTraitInfo<SelectableInfo>() && (x.Owner.IsAlliedWith(world.RenderPlayer) || !world.FogObscures(x)))
                        .SubsetWithHighestSelectionPriority();
        }
    }
}
