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

        /// <summary>
		/// Creates a new <see cref="Stream"/> that wraps a subset of the source stream without taking ownership of it,
		/// allowing it to be reused by the caller. The <see cref="Stream"/> is independent of the source stream and
		/// won't affect its position.
		/// </summary>
		/// <param name="stream">The source stream, of which only a segment should be exposed. Ownership is retained by
		/// the caller.</param>
		/// <param name="offset">The offset at which the segment starts.</param>
		/// <param name="count">The length of the segment.</param>
		public static Stream CreateWithoutOwningStream(Stream stream, long offset, int count)
        {
            Stream parentStream;
            var nestedOffset = offset + GetOverallNestedOffset(stream, out parentStream);

            // Special case FileStream - instead of creating an in-memory copy,
            // just reference the portion of the on-disk file that we need to save memory.
            // We use GetType instead of 'is' here since we can't handle any derived classes of FileStream.
            if (parentStream.GetType() == typeof(FileStream))
            {
                var path = ((FileStream)parentStream).Name;
                return new SegmentStream(File.OpenRead(path), nestedOffset, count);
            }

            // For all other streams, create a copy in memory.
            // This uses more memory but is the only way in general to ensure the returned streams won't clash.
            stream.Seek(offset, SeekOrigin.Begin);
            return new MemoryStream(stream.ReadBytes(count));
        }
    }
}