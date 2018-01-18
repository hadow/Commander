using System;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{


    public class ValidateOrderInfo : TraitInfo<ValidateOrder>{}

    public class ValidateOrder:IValidateOrder
    {

        public bool OrderValidation(OrderManager orderManager,World world,int clientId,Order order){

            if (order.Subject == null || order.Subject.Owner == null)
                return true;

            var subjectClientId = order.Subject.Owner.ClientIndex;
            var subjectClient = orderManager.LobbyInfo.ClientWithIndex(subjectClientId);

            if(subjectClient == null)
            {
                return false;
            }

            var isBotOrder = subjectClient.Bot != null && clientId == subjectClient.BotControllerClientIndex;

            if (subjectClientId != clientId && !isBotOrder)
                return false;

            return order.Subject.AcceptsOrder(order.OrderString);
        }

    }
}