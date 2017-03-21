using System;
using RA.Game.Primitives;

namespace RA.Game
{
    /// <summary>
    /// 
    /// </summary>
    public class ActorInfo
    {

        public readonly string name;
        readonly TypeDictionary traits = new TypeDictionary();


        public bool HasTraitInfo<T>() where T : ITraitInfoInterface
        {
            return traits.Contains<T>();
        }



    }
}