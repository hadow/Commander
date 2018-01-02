using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Warheads
{
    /// <summary>
    /// Leave smudge warhead.
    /// </summary>
    public class LeaveSmudgeWarhead:Warhead
    {


        public readonly int[] Size = { 0, 0 };

        public readonly HashSet<string> SmudgeType = new HashSet<string>();

        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers)
        {


        }
    }
}