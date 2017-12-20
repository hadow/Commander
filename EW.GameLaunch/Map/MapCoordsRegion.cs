using System;
using System.Collections;
using System.Collections.Generic;

namespace EW
{
    public struct MapCoordsRegion:IEnumerable<MPos>
    {

        readonly MPos topLeft;
        readonly MPos bottomRight;


        public MPos TopLeft { get { return topLeft; } }
        public MPos BottomRight { get { return bottomRight; } }

        public MapCoordsRegion(MPos mapTopLeft,MPos mapBottomRight)
        {
            topLeft = mapTopLeft;
            bottomRight = mapBottomRight;
        }

        public MapCoordsEnumerator GetEnumerator()
        {
            return new MapCoordsEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<MPos> IEnumerable<MPos>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct MapCoordsEnumerator : IEnumerator<MPos>
        {
            readonly MapCoordsRegion r;
            MPos current;

            public MapCoordsEnumerator(MapCoordsRegion region):this()
            {
                r = region;
                Reset();
            }

            public bool MoveNext()
            {
                var u = current.U + 1;
                var v = current.V;

                if (u > r.bottomRight.U)
                {
                    v += 1;
                    u = r.topLeft.U;

                    if (v > r.bottomRight.V)
                        return false;
                }

                current = new MPos(u, v);
                return true;
            }

            public void Reset()
            {
                current = new MPos(r.topLeft.U - 1, r.topLeft.V);
            }

            public MPos Current { get { return current; } }

            object IEnumerator.Current { get { return Current; } }

            public void Dispose() { }

        }

    }
}