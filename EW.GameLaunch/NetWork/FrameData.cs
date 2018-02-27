using System;
using System.Collections.Generic;
using System.Linq;
namespace EW.NetWork
{
    class FrameData
    {
        public struct ClientOrder{
            public int Client;
            public Order Order;

            public override string ToString()
            {
                return string.Format("[ClientOrder:{0} {1}]",Client,Order);
            }
        }

        readonly Dictionary<int, int> clientQuitTimes = new Dictionary<int, int>();

        readonly Dictionary<int, Dictionary<int, byte[]>> framePackets = new Dictionary<int, Dictionary<int, byte[]>>();

        /// <summary>
        /// Clientses the playing in frame.
        /// </summary>
        /// <returns>The playing in frame.</returns>
        /// <param name="frame">Frame.</param>

        public IEnumerable<int> ClientsPlayingInFrame(int frame){

            return clientQuitTimes.Where(x => frame <= x.Value).Select(x => x.Key).OrderBy(x => x);
        }

        public bool IsReadyForFrame(int frame){

            return !ClientsNotReadyForFrame(frame).Any();
        }

        public IEnumerable<int> ClientsNotReadyForFrame(int frame){

            var frameData = framePackets.GetOrAdd(frame);

            return ClientsPlayingInFrame(frame).Where(client => !frameData.ContainsKey(client));
        }


        /// <summary>
        /// Adds the frame orders.
        /// </summary>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="frame">Frame.</param>
        /// <param name="orders">Orders.</param>
        public void AddFrameOrders(int clientId,int frame,byte[] orders){

            var frameData = framePackets.GetOrAdd(frame);
            frameData.Add(clientId,orders);

        }

        public IEnumerable<ClientOrder> OrdersForFrame(World world,int frame)
        {

            var frameData = framePackets[frame];
            var clientData = ClientsPlayingInFrame(frame).ToDictionary(k=>k,v=>frameData[v]);

            return clientData.SelectMany(x => x.Value.ToOrderList(world).Select(o => new ClientOrder
            {
                Client = x.Key,
                Order = o,
            }));



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="lastClientFrame"></param>
        public void ClientQuit(int clientId,int lastClientFrame)
        {
            if (lastClientFrame == -1)
                lastClientFrame = framePackets.Where(x => x.Value.ContainsKey(clientId)).Select(x => x.Key).OrderBy(x => x).LastOrDefault();

            clientQuitTimes[clientId] = lastClientFrame;
        }


    }
}
