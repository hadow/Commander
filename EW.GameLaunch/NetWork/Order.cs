using System;
using System.IO;
using EW.Traits;
using EW.Framework;
namespace EW.NetWork
{

    enum OrderFields : byte
    {
        Target = 0x01,
        TargetString = 0x04,
        Queued = 0x08,
        ExtraLocation = 0x10,
        ExtraData = 0x20,
    }

    static class OrderFieldsExts{
        public static bool HasField(this OrderFields of,OrderFields f){
            return (of & f) != 0;
        }
    }
    public sealed class Order
    {
        public readonly string OrderString;
        public readonly Actor Subject;
        public readonly bool Queued;
        //public Actor TargetActor;
        public readonly Target Target;
        //public CPos TargetLocation;
        public string TargetString;
        public CPos ExtraLocation;
        public uint ExtraData;
        public bool IsImmediate;
        public bool SuppressVisualFeedback;
        public Actor VisualFeedbackTarget;

        public Player Player
        {
            get
            {
                return Subject != null ? Subject.Owner : null;
            }
        }

        public Actor TargetActor { get { return Target.SerializableActor; } }

        public CPos TargetLocation
        {
            get
            {
                return Target.SerializableCell.HasValue ? Target.SerializableCell.Value : CPos.Zero;
            }
        }

        Order(string orderString,Actor subject,Target target,string targetString,bool queued,CPos extraLocation,uint extraData)
        {
            OrderString = orderString;
            Subject = subject;
            Target = target;
            TargetString = targetString;
            Queued = queued;
            ExtraLocation = extraLocation;
            ExtraData = extraData;
        }

        public Order(string orderString,Actor subject,bool queued) : this(orderString, subject, Target.Invalid,null, queued, CPos.Zero, 0) { }


        public Order(string orderString,Actor subject,Target target,bool queued):this(orderString,subject,target,null,queued,CPos.Zero,0){}


        public byte[] Serialize(){
            var minLength = OrderString.Length + 1 + (IsImmediate ? 1 + TargetString.Length + 1 : 6);

            var ret = new MemoryStream(minLength);

            var w = new BinaryWriter(ret);

            if(IsImmediate){
                w.Write((byte)0xFE);
                w.Write(OrderString);
                w.Write(TargetString);
                return ret.ToArray();

            }
            w.Write((byte)0xFF);
            w.Write(OrderString);
            w.Write(UIntFromActor(Subject));

            OrderFields fields = 0;
            if (Target.SerializableType != TargetT.Invalid)
                fields |= OrderFields.Target;

            if (TargetString != null)
                fields |= OrderFields.TargetString;

            if (Queued)
                fields |= OrderFields.Queued;

            if (ExtraLocation != CPos.Zero)
                fields |= OrderFields.ExtraLocation;

            if (ExtraData != 0)
                fields |= OrderFields.ExtraData;

            w.Write((byte)fields);

            if(fields.HasField(OrderFields.Target))
            {
                w.Write((byte)Target.SerializableType);
                switch(Target.SerializableType){
                    case TargetT.Actor:
                        w.Write(UIntFromActor(Target.SerializableActor));
                        break;
                    case TargetT.FrozenActor:
                        w.Write(Target.FrozenActor.Viewer.PlayerActor.ActorID);
                        w.Write(Target.FrozenActor.ID);
                        break;
                    case TargetT.Terrain:
                        w.Write(Target.SerializableCell.Value);
                        break;
                }
            }

            if (fields.HasField(OrderFields.TargetString))
                w.Write(TargetString);

            if (fields.HasField(OrderFields.ExtraLocation))
                w.Write(ExtraLocation);

            if (fields.HasField(OrderFields.ExtraData))
                w.Write(ExtraData);

            return ret.ToArray();
        }


        static uint UIntFromActor(Actor a){
            if (a == null) return uint.MaxValue;
            return a.ActorID;
        }

        /// <summary>
        /// Deserialize the specified world and r.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="world">World.</param>
        /// <param name="r">The red component.</param>
        public static Order Deserialize(World world,BinaryReader r){

            var magic = r.ReadByte();

            switch(magic){
                case 0xFF:
                    {
                        var order = r.ReadString();
                        var subjectId = r.ReadUInt32();
                        var flags = (OrderFields)r.ReadByte();

                        Actor subject = null;
                        if (world != null)
                            TryGetActorFromUInt(world, subjectId, out subject);

                        var target = Target.Invalid; 
                        if(flags.HasField(OrderFields.Target))
                        {
                            switch((TargetT)r.ReadByte())
                            {
                                case TargetT.Actor:
                                    {
                                        Actor targetActor;
                                        if(world != null && TryGetActorFromUInt(world,r.ReadUInt32(),out targetActor))
                                        {
                                            target = Target.FromActor(targetActor);
                                        }
                                        break;
                                    }
                                case TargetT.FrozenActor:
                                    {
                                        var playerActorID = r.ReadUInt32();
                                        var frozenActorID = r.ReadUInt32();

                                        Actor playerActor;

                                        if (world == null || !TryGetActorFromUInt(world, playerActorID, out playerActor))
                                            break;

                                        var frozenLayer = playerActor.TraitOrDefault<FrozenActorLayer>();
                                        if (frozenLayer == null)
                                            break;

                                        var frozen = frozenLayer.FromID(frozenActorID);
                                        if (frozen != null)
                                            target = Target.FromFrozenActor(frozen);

                                        break;
                                    }
                                case TargetT.Terrain:
                                    {
                                        
                                        if (world != null)
                                            target = Target.FromCell(world, (CPos)r.ReadInt2());
                                        break;
                                    }

                            }
                        }

                        var targetString = flags.HasField(OrderFields.TargetString) ? r.ReadString() : null;
                        var queued = flags.HasField(OrderFields.Queued);
                        var extraLocation = (CPos)(flags.HasField(OrderFields.ExtraLocation) ? r.ReadInt2() : Int2.Zero);
                        var extraData = flags.HasField(OrderFields.ExtraData) ? r.ReadUInt32() : 0;

                        if (world != null)
                            return new Order(order, null, target, targetString, queued, extraLocation, extraData);

                        if (subject == null && subjectId != uint.MaxValue)
                            return null;

                        return new Order(order, subject, target, targetString, queued, extraLocation, extraData);
                            
                    }
                case 0xfe:
                    {
                        var name = r.ReadString();
                        var data = r.ReadString();
                        return new Order(name, null, false) { IsImmediate = true, TargetString = data };
                                    
                    }
                default:
                    return null;
            }
        }


        static bool  TryGetActorFromUInt(World world,uint aID,out Actor ret){

            if(aID == uint.MaxValue)
            {
                ret = null;
                return true;
            }
            ret = world.GetActorById(aID);
            return ret != null;
        }
        public static Order PauseGame(bool paused)
        {
            return new Order("PauseGame", null, false) { TargetString = paused ? "Pause" : "UnPause" };
        }


        public static Order Command(string text)
        {
            return new Order("Command", null, false) { IsImmediate = true, TargetString = text };
        }

        public static Order HandshakeResponse(string text)
        {
            return new Order("HandshakeResponse", null, false) { IsImmediate = true, TargetString = text };
        }

        public static Order Pong(string pingTime)
        {
            return new Order("Pong", null, false) { IsImmediate = true, TargetString = pingTime };
        }
    }
}