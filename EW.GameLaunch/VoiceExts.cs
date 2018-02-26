using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.NetWork;
namespace EW
{
    public static class VoiceExts
    {
        public static void PlayVoice(this Actor self,string phrase)
        {
            if (phrase == null)
                return;

            foreach(var voiced in self.TraitsImplementing<IVoiced>())
            {
                if (string.IsNullOrEmpty(voiced.VoiceSet))
                    return;

                voiced.PlayVoice(self, phrase, self.Owner.Faction.InternalName);
            }
        }

        public static void PlayVoiceLocal(this Actor self,string phrase,float volume)
        {
            if (phrase == null)
                return;

            foreach(var voiced in self.TraitsImplementing<IVoiced>())
            {
                if (string.IsNullOrEmpty(voiced.VoiceSet))
                    return;

                voiced.PlayVoiceLocal(self, phrase, self.Owner.Faction.InternalName, volume);
            }
        }

        public static void PlayVoiceForOrders(this World w,Order[] orders)
        {

        }

        public static bool HasVoice(this Actor self,string voice)
        {
            return self.TraitsImplementing<IVoiced>().Any(x => x.HasVoice(self, voice));
        }



    }
}