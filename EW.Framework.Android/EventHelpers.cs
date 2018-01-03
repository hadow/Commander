using System;

namespace EW.Framework
{
    internal static class EventHelpers
    {



        internal static void Raise<TEventArgs>(object sender,EventHandler<TEventArgs> handler,TEventArgs e) where TEventArgs : EventArgs
        {
            if (handler != null)
                handler(sender, e);
        }


    }
}