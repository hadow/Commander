using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EW.Primitives;

namespace EW.NetWork
{
    /// <summary>
    /// ЦёБо
    /// </summary>
    public sealed class OrderManager:IDisposable
    {
        public readonly string Host;
        public readonly int Port;
        public readonly string Password = "";

        readonly SyncReport syncReport;

        readonly FrameData frameData = new FrameData();

        public World World;

        public IConnection Connection { get; private set; }
        public long LastTickTime;

        public int NetFrameNumber { get; private set; }
        public int LocalFrameNumber;
        public int FramesAhead = 0;

        List<Order> localOrders = new List<Order>();

        public Session LobbyInfo = new Session();

        Dictionary<int, byte[]> syncForFrame = new Dictionary<int, byte[]>();

        public bool GameStarted { get { return NetFrameNumber != 0; } }

        bool disposed;

        public OrderManager(string host,int port,string password,IConnection conn){

            Host = host;
            Port = port;
            Password = password;
            Connection = conn;
            syncReport = new SyncReport(this);

        }


        /// <summary>
        /// Checks the sync.
        /// </summary>
        /// <param name="packet">Packet.</param>
        void CheckSync(byte[] packet){

            var frame = BitConverter.ToInt32(packet, 0);
            byte[] existingSync;

            if(syncForFrame.TryGetValue(frame,out existingSync)){

                if (packet.Length != existingSync.Length)
                    OutOfSync(frame);
                else
                    for (var i = 0; i < packet.Length; i++)
                        if (packet[i] != existingSync[i])
                            OutOfSync(frame);
            }
            else{
                syncForFrame.Add(frame,packet);
            }
        }

        /// <summary>
        /// Outs the of sync.
        /// </summary>
        /// <param name="frame">Frame.</param>
        void OutOfSync(int frame){

            syncReport.DumpSyncReport(frame, frameData.OrdersForFrame(World, frame));

            throw new InvalidOperationException("Out of sync in frame {0}.\n Compare syncreport.log with other players".F(frame));
        }

        public void StartGame()
        {
            if (GameStarted)
                return;
            NetFrameNumber = 1;

        }


        public bool IsReadyForNextFrame
        {
            get { return NetFrameNumber >= 1 && frameData.IsReadyForFrame(NetFrameNumber); }
        }

        public void IssueOrder(Order order)
        {
            localOrders.Add(order);
        }

        public void IssueOrders(Order[] orders){
            foreach (var order in orders)
                IssueOrder(order);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Tick()
        {

            if (!IsReadyForNextFrame)
                throw new InvalidOperationException();

            Connection.Send(NetFrameNumber + FramesAhead,localOrders.Select(o=>o.Serialize()).ToList());
            localOrders.Clear();


            foreach (var order in frameData.OrdersForFrame(World, NetFrameNumber))
                UnitOrders.ProcessOrder(this, World, order.Client, order.Order);

            Connection.SendSync(NetFrameNumber,OrderIO.SerializeSync(World.SyncHash()));
            NetFrameNumber++;

        }


        public void TickImmediate(){

            var immediateOrders = localOrders.Where(o => o.IsImmediate).ToList();
            if (immediateOrders.Count != 0)
                Connection.SendImmediate(immediateOrders.Select(o => o.Serialize()).ToList());
            localOrders.RemoveAll(o=>o.IsImmediate);

            var immediatePackets = new List<Pair<int, byte[]>>();

            Connection.Receive((clientId,packet)=>{

                var frame = BitConverter.ToInt32(packet, 0);

                if(packet.Length == 5 && packet[4] == 0xBF ){
                    frameData.ClientQuit(clientId, frame);
                }
                else if(packet.Length>=5 && packet[4] == 0x65){
                    CheckSync(packet);
                }
                else if(frame == 0){
                    immediatePackets.Add(Pair.New(clientId, packet));
                }
                else{
                    frameData.AddFrameOrders(clientId,frame,packet);
                }
            });

            foreach(var p in immediatePackets){
                foreach(var o in p.Second.ToOrderList(World)){

                    UnitOrders.ProcessOrder(this,World,p.First,o);

                    //
                    if (disposed)
                        return;
                }
            }


        }
        public void Dispose()
        {
            disposed = true;
            if (Connection != null)
                Connection.Dispose();   
        }
    }
}