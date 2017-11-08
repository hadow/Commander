using System;
using System.Collections.Generic;


namespace EW
{
    public class Selection
    {
        /// <summary>
        /// Tracking Selection Change
        /// </summary>
        public int Hash { get; private set; }

        readonly HashSet<Actor> actors = new HashSet<Actor>();
        public IEnumerable<Actor> Actors { get { return actors; } }
    }
}