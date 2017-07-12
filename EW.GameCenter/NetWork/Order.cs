using System;
using System.IO;
namespace EW
{
    public sealed class Order
    {
        public readonly string OrderString;
        public readonly Actor Subject;
        public readonly bool Queued;
        public Actor TargetActor;
        public CPos TargetLocation;
        public string TargetString;
        public CPos ExtraLocation;
        public uint ExtraData;
        public bool IsImmediate;
        public bool SuppressVisualFeedback;


        Order(string orderString,Actor subject,Actor targetActor,CPos targetLocation,string targetString,bool queued,CPos extraLocation,uint extraData)
        {

        }

        public Order(string orderString,Actor subject,bool queued) : this(orderString, subject, null, CPos.Zero, null, queued, CPos.Zero, 0) { }

        public static Order PauseGame(bool paused)
        {
            return new Order("PauseGame", null, false) { TargetString = paused ? "Pause" : "UnPause" };
        }
    }
}