using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetStitch.Server
{
    public class NonDisposableStream : Stream
    {
        readonly Stream stream;

        public NonDisposableStream(Stream stream)
        {
            this.stream = stream;
        }

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => stream.CanWrite;

        public override long Length => stream.Length;

        public override long Position { get { return stream.Position; } set { stream.Position = value; } }

        public override void Flush() => stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

        public override void SetLength(long value) => stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing) { }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => stream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override Task FlushAsync(CancellationToken cancellationToken) => stream.FlushAsync(cancellationToken);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => stream.ReadAsync(buffer, offset, count);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => stream.WriteAsync(buffer, offset, count, cancellationToken);

        public override int ReadByte() => stream.ReadByte();

        public override void WriteByte(byte value) => stream.WriteByte(value);
    }
}
