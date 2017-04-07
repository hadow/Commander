using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EW
{
    public static class FieldLoader
    {

        public class MissingFieldsException : Exception
        {
            public readonly string[] Missing;
            public readonly string Header;

            public override string Message
            {
                get
                {
                    return (string.IsNullOrEmpty(Header) ? "" : Header + ": ") + Missing[0] + string.Concat(Missing.Skip(1).Select(m => ", " + m));
                }
            }

            public MissingFieldsException(string[] missing,string header = null,string headerSingle = null) : base(null)
            {
                Header = missing.Length > 1 ? header : headerSingle ?? header;
                Missing = missing;
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
                info.AddValue("Missing", Missing);
                info.AddValue("Header", Header);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="my"></param>
        public static void Load(object self,MiniYaml my)
        {

        }
    }
}