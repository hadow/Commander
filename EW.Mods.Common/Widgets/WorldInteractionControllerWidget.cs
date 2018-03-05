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
using EW.Mods.Common.Effects;
using EW.Orders;

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


                //Place buildings,use support powers,and other non-unit things
                if(!(World.OrderGenerator is UnitOrderGenerator))
                {
                    ApplyOrders(World, gs);
                    isDragging = false;
                    YieldFocus(gs);
                    return true;
                }
            }

            if(gs.GestureType == GestureType.DragComplete){


                if(World.OrderGenerator is UnitOrderGenerator)
                {

                    if(isDragging && (!(World.OrderGenerator is GenericSelectTarget) || IsValidDragBox))
                    {
                        var newSelection = SelectActorsInBoxWithDeadzone(World, dragStart, mousePos);
                        World.Selection.Combine(World, newSelection, false, dragStart == mousePos);
                    }
                    
                    World.CancelInputMode();
                }
                isDragging = false;
                YieldFocus(gs);
            }

            if(gs.GestureType == GestureType.Tap)
            {
                if (!IsValidDragBox)
                {
                    ApplyOrders(World, gs);
                }
            }

            return true;

        }


        void ApplyOrders(World world,GestureSample gs)
        {
            if (world.OrderGenerator == null)
                return;

            var cell = worldRenderer.ViewPort.ViewToWorld(gs.Position.ToInt2());
            var worldPixel = worldRenderer.ViewPort.ViewToWorldPx(gs.Position.ToInt2());
            var orders = world.OrderGenerator.Order(world, cell, worldPixel, gs).ToArray();
            world.PlayVoiceForOrders(orders);

            var flashed = false;

            foreach(var order in orders)
            {
                var o = order;
                if (o == null)
                    continue;

                if(!flashed && !o.SuppressVisualFeedback)
                {
                    var visualTargetActor = o.VisualFeedbackTarget ?? o.TargetActor;
                    if(visualTargetActor != null)
                    {
                        world.AddFrameEndTask(w => w.Add(new FlashTarget(visualTargetActor)));
                        flashed = true;

                    }
                    else if(o.TargetLocation != CPos.Zero)
                    {
                        var pos = world.Map.CenterOfCell(cell);
                        world.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, world, "moveflsh", "idle", "moveflash", true, true)));
                        flashed = true;
                    }
                }

                world.IssueOrder(o);
            }
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
