using System;
using System.IO;

namespace EW.Primitives
{

    /// <summary>
    /// 
    /// </summary>
    public class SegmentStream:Stream
    {
        public readonly Stream BaseStream;
        public readonly long BaseOffset;
        public readonly long BaseCount;

        public SegmentStream(Stream stream,long offset,long count)
        {
            if (stream == null)
                throw new ArgumentException("stream");
            if (!stream.CanSeek)
                throw new ArgumentException("stream must be seekable.","stream");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset musb be non-negative");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count must be non-negative.");

            BaseStream = stream;
            BaseOffset = offset;
            BaseCount = count;

            stream.Seek(BaseOffset, SeekOrigin.Begin);
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanRead
        {
            get
            {
                return BaseStream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return BaseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return BaseCount;
            }
        }

        public override long Position
        {
            get
            {
            }

            set
            {

            }
        }
    }
}