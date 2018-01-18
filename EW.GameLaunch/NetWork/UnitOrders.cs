using System;
using EW.Traits;
using System.Linq;
namespace EW.NetWork
{
    public static class UnitOrders
    {

        /// <summary>
        /// Processes the order.
        /// </summary>
        /// <param name="orderManager">Order manager.</param>
        /// <param name="world">World.</param>
        /// <param name="clientId">Client identifier.</param>
        /// <param name="order">Order.</param>
        internal static void ProcessOrder(OrderManager orderManager,World world,int clientId,Order order){

            if(world != null){

                if(!world.WorldActor.TraitsImplementing<IValidateOrder>().All(vo=>vo.OrderValidation(orderManager,world,clientId,order))){
                    return;
                }
            }

            switch(order.OrderString){
                case "Disconnected":
                    {
                        break;
                    }
                case "StartGame":
                    {
                        break;
                    }
                case "PauseGame":
                    {
                        break;
                    }
                case "HandsshakeRequest":
                    {
                        break;
                    }
                case "ServerError":
                    {
                        break;
                    }
                case "AuthenticationError":{
                        break;
                    }
                case "SyncInfo":
                    {
                        
                        break;
                    }
                case "Ping":
                    {
                        break;
                    }

                default:
                    {
                        if(!order.IsImmediate){
                            var self = order.Subject;
                            if(!self.IsDead)
                            {
                                foreach (var t in self.TraitsImplementing<IResolveOrder>())
                                    t.ResolveOrder(self, order);
                            }
                        }
                        break;
                    }
            }
        }
    }
}
