using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;

namespace EW
{
    public static class VoiceExts
    {

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



    }
}