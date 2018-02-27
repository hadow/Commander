using System;
using System.IO;
namespace EW.Server
{
    class ServerOrder
    {


        public readonly string Name;
        public readonly string Data;

        public ServerOrder(string name,string data)
        {
            Name = name;
            Data = data;
        }


        public static ServerOrder Deserialize(BinaryReader r)
        {
            byte b;
            switch (b = r.ReadByte())
            {
                case 0xff:
                    //This isn't a server order
                    return null;
                case 0xfe:
                    {
                        var name = r.ReadString();
                        var data = r.ReadString();

                        return new ServerOrder(name, data);
                    }
                default:
                    throw new NotImplementedException(b.ToString("x2"));
            }
        }


        public byte[] Serialize()
        {
            var ms = new MemoryStream(1 + Name.Length + 1 + Data.Length + 1);
            var bw = new BinaryWriter(ms);

            bw.Write((byte)0xfe);
            bw.Write(Name);
            bw.Write(Data);
            return ms.ToArray();
        }

    }
}