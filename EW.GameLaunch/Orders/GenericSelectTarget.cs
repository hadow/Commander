using System;
using System.Collections.Generic;
using EW.NetWork;
using EW.Framework.Touch;
using EW.Traits;
namespace EW.Orders
{
    public class GenericSelectTarget:UnitOrderGenerator
    {
        public readonly string OrderName;
        protected readonly string Cursor;

        protected readonly IEnumerable<Actor> Subjects;
        protected readonly GestureType ExpectedGestureType;


        public GenericSelectTarget(IEnumerable<Actor> subjects, string order, string cursor, GestureType type)
        {
            Subjects = subjects;
            OrderName = order;
            Cursor = cursor;
            ExpectedGestureType = type;
        }

        public GenericSelectTarget(IEnumerable<Actor> subjects, string order, string cursor)
            : this(subjects, order, cursor, GestureType.Tap) { }

        public GenericSelectTarget(Actor subject, string order, string cursor)
            : this(new Actor[] { subject }, order, cursor) { }

        public GenericSelectTarget(Actor subject, string order, string cursor, GestureType type)
            : this(new Actor[] { subject }, order, cursor, type) { }


        protected virtual IEnumerable<Order> OrderInner(World world, CPos cell, GestureSample gs)
        {
            if(gs.GestureType == ExpectedGestureType && world.Map.Contains(cell) ){

                world.CancelInputMode();

                var queued = false;
                foreach(var subject in Subjects){
                    yield return new Order(OrderName, subject, Target.FromCell(world, cell), queued);
                }
            }
        }


        public override IEnumerable<Order> Order(World world, CPos cell, Framework.Int2 worldPixel, GestureSample gs)
        {
            if(gs.GestureType != ExpectedGestureType){

                world.CancelInputMode();
            }

            return OrderInner(world, cell, gs);
        }

    }
}