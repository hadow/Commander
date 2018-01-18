using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using EW.Framework;
namespace EW.NetWork
{
    public static class OrderIO
    {

        /// <summary>
        /// Serializes the sync.
        /// </summary>
        /// <returns>The sync.</returns>
        /// <param name="sync">Sync.</param>
        public static byte[] SerializeSync(int sync){

            var ms = new MemoryStream(1 + 4);
            using (var wr = new BinaryWriter(ms))
            {
                wr.Write((byte)0x65);
                wr.Write(sync);
            }
            return ms.GetBuffer();
        }


        public static void Write(this BinaryWriter w,CPos cell ){
            w.Write(cell.X);
            w.Write(cell.Y);
        }

        public static Int2 ReadInt2(this BinaryReader r){
            var x = r.ReadInt32();
            var y = r.ReadInt32();
            return new Int2(x, y);
        }


        public static List<Order> ToOrderList(this byte[] bytes,World world){
            var ms = new MemoryStream(bytes, 4, bytes.Length - 4);
            var reader = new BinaryReader(ms);
            var ret = new List<Order>();

            while(ms.Position<ms.Length)
            {
                var o = Order.Deserialize(world, reader);
                if (o != null)
                    ret.Add(o);
                
            }
            return ret;
        }
    }
}
