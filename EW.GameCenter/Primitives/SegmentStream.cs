using System;
using System.IO;
using System.ComponentModel;
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
                return BaseStream.Position - BaseOffset;
            }

            set
            {
                BaseStream.Position = BaseOffset + value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return BaseStream.Seek(BaseOffset + offset, SeekOrigin.Begin) - offset;
                case SeekOrigin.Current:
                    return BaseStream.Seek(offset, SeekOrigin.Current) - offset;
                case SeekOrigin.End:
                    return BaseStream.Seek(offset, SeekOrigin.End) - offset;
                default:
                    throw new InvalidEnumArgumentException("origin", (int)origin, typeof(SeekOrigin));
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanTimeout
        {
            get
            {
                return BaseStream.CanTimeout;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                return BaseStream.ReadTimeout;
            }

            set
            {
                BaseStream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return BaseStream.WriteTimeout;
            }

            set
            {
                BaseStream.WriteTimeout = value;
            }
        }

        protected override void Dispose(bool disposing)
        {

            if (disposing)
                BaseStream.Dispose();
            base.Dispose(disposing);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="overallBaseStream"></param>
        /// <returns></returns>
        public static long GetOverallNestedOffset(Stream stream,out Stream overallBaseStream)
        {
            var offset = 0L;
            overallBaseStream = stream;
            var segmentStream = stream as SegmentStream;
            if(segmentStream != null)
            {
                offset += segmentStream.BaseOffset + GetOverallNestedOffset(segmentStream.BaseStream, out overallBaseStream);
            }
            return offset;
        }
    }
}