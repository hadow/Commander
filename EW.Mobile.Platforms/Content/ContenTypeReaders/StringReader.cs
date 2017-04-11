using System;


namespace EW.Mobile.Platforms.Content
{
    internal class StringReader:ContentTypeReader<String>
    {

        internal StringReader()
        {

        }

        protected internal override string Read(ContentReader input, string existingInstance)
        {
            return input.ReadString();
        }


    }
}