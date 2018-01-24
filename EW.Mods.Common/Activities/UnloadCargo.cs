using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Primitives;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class UnloadCargo:Activity
    {

        readonly Actor self;
        readonly Cargo cargo;

        readonly INotifyUnload[] notifiers;
        readonly bool unloadAll;

        public UnloadCargo(Actor self,bool unloadAll)
        {
            this.self = self;
            cargo = self.Trait<Cargo>();
            notifiers = self.TraitsImplementing<INotifyUnload>().ToArray();
            this.unloadAll = unloadAll;
        }

        public Pair<CPos,SubCell>? ChooseExitSubCell(Actor passenger)
        {
            var pos = passenger.Trait<IPositionable>();

            return cargo.CurrentAdjacentCells.Shuffle(self.World.SharedRandom)
                .Select(c => Pair.New(c, pos.GetAvailableSubCell(c)))
                .Cast<Pair<CPos, SubCell>?>()
                .FirstOrDefault(s => s.Value.Second != SubCell.Invalid);
        }

        public override Activity Tick(Actor self)
        {

            cargo.Unloading = false;

            if (IsCanceled || cargo.IsEmpty(self))
                return NextActivity;

            foreach (var inu in notifiers)
                inu.Unloading(self);


            var actor = cargo.Peek(self);
            var spawn = self.CenterPosition;

            var exitSubCell = ChooseExitSubCell(actor);
            if(exitSubCell != null)
            {
                self.NotifyBlocker(BlockedExitCells(actor));
                return ActivityUtils.SequenceActivities(new Wait(10), this);
            }
            cargo.Unload(self);
            self.World.AddFrameEndTask(w =>
            {
                if (actor.Disposed)
                    return;

                var move = actor.Trait<IMove>();
                var pos = actor.Trait<IPositionable>();

                actor.CancelActivity();
                pos.SetVisualPosition(actor, spawn);
                actor.QueueActivity(move.MoveIntoWorld(actor, exitSubCell.Value.First, exitSubCell.Value.Second));
                actor.SetTargetLine(Target.FromCell(w, exitSubCell.Value.First, exitSubCell.Value.Second), Color.Green, false);
                w.Add(actor);

            });

            if (!unloadAll || cargo.IsEmpty(self))
                return NextActivity;

            cargo.Unloading = true;
            return this;

        }

        IEnumerable<CPos> BlockedExitCells(Actor passenger)
        {
            var pos = passenger.Trait<IPositionable>();

            //Find the cells that are blocked by transient actors.
            return cargo.CurrentAdjacentCells.Where(c => pos.CanEnterCell(c, null, true) != pos.CanEnterCell(c, null, false));
        }
    }
}