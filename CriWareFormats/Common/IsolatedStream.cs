using System;
using System.IO;

namespace CriWareFormats.Common
{
    public class IsolatedStream : Stream
    {
        private readonly Stream sourceStream;
        private readonly long realPosition;

        private long internalPosition;

        private readonly object positionLock = new();

        public IsolatedStream(Stream sourceStream, long offset, long length)
        {
            this.sourceStream = sourceStream;
            realPosition = offset;
            internalPosition = 0;

            Length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position
        {
            get
            {
                lock (positionLock)
                {
                    return internalPosition;
                }
            }
            set
            {
                lock (positionLock)
                {
                    long checkValue = value + realPosition;
                    if (value < 0 || value >= Length) throw new ArgumentOutOfRangeException(nameof(value));
                    internalPosition = value;
                    sourceStream.Position = checkValue;
                }
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (positionLock)
            {
                long restore = sourceStream.Position;
                sourceStream.Position = realPosition + internalPosition;
                int read = sourceStream.Read(buffer, offset, count);
                internalPosition += read;
                sourceStream.Position = restore;
                return read;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (positionLock)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        if (offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                        internalPosition = offset;
                        break;

                    case SeekOrigin.Current:
                        if (internalPosition + offset >= Length) throw new ArgumentOutOfRangeException(nameof(offset));
                        internalPosition += offset;
                        break;

                    case SeekOrigin.End:
                        if (internalPosition - offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
                        internalPosition = Length;
                        internalPosition -= offset;
                        break;

                    default:
                        break;
                }

                return internalPosition;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}