using System;
using EW.Activities;
using EW.Mods.Cnc.Traits.Render;
using EW.Mods.Common.Activities;
namespace EW.Mods.Cnc.Activities
{
    public class VoxelHarvesterDockSequence:HarvesterDockSequence
    {

        readonly WithVoxelUnloadBody body;
        readonly WithDockingOverlay spriteOverlay;


        public VoxelHarvesterDockSequence(Actor self,Actor refinery,int dockAngle,bool isDragRequired,WVec dragOffset,int dragLength):
            base(self, refinery, dockAngle, isDragRequired, dragOffset, dragLength)
        {

            body = self.Trait<WithVoxelUnloadBody>();
            spriteOverlay = refinery.TraitOrDefault<WithDockingOverlay>();
        }

        public override Activity OnStateDock(Actor self)
        {
            body.Docked = true;

            if (spriteOverlay != null && !spriteOverlay.Visible)
            {
                spriteOverlay.Visible = true;
                spriteOverlay.WithOffset.Animation.PlayThen(spriteOverlay.Info.Sequence, () =>
                {
                    dockingState = DockingState.Loop;
                    spriteOverlay.Visible = false;
                });
            }
            else
                dockingState = DockingState.Loop;

            return this;

        }

        public override Activity OnStateUndock(Actor self)
        {
            dockingState = DockingState.Wait;

            if(spriteOverlay != null && !spriteOverlay.Visible)
            {
                spriteOverlay.Visible = true;
                spriteOverlay.WithOffset.Animation.PlayBackwardsThen(spriteOverlay.Info.Sequence, () =>
                {
                    dockingState = DockingState.Complete;
                    body.Docked = false;
                    spriteOverlay.Visible = false;
                });
            }
            else
            {
                dockingState = DockingState.Complete;
                body.Docked = false;
            }
            return this;
        }

    }
}