using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace IPFilter.Cli
{
    public static class StreamExtensions
    {
        /// <summary>
        /// The default copy buffer size, used in the .NET source, is reputedly :
        /// "... the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
        /// The CopyTo/CopyToAsync buffer is short-lived and is likely to be collected at Gen0, and it offers a significant
        /// improvement in Copy performance".
        /// </summary>
        public const int DefaultCopyBufferSize = 81920;

        public static async Task<int> ReadAndHashAsync(this Stream source, byte[] buffer, int offset, int count, HashAlgorithm algorithm, CancellationToken cancellationToken)
        {
            var bytes = await source.ReadAsync(buffer, offset, count, cancellationToken);

            if (bytes == 0)
            {
                algorithm.TransformFinalBlock(buffer, 0, 0);
            }
            else
            {
                algorithm.TransformBlock(buffer, 0, bytes, null, 0);
            }

            return bytes;
        }

        public static async Task CopyToAsync(this Stream source, Stream destination, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            
            Debug.Assert(source.CanRead);
            Debug.Assert(destination.CanWrite);

            byte[] buffer = new byte[DefaultCopyBufferSize];
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
        }

        public static Task<byte[]> CopyToAndHashAsync(this Stream source, Stream destination, HashAlgorithm hasher, CancellationToken cancellationToken)
        {
            return CopyToAndHashAsync(source, destination, DefaultCopyBufferSize, hasher, cancellationToken);
        }

        public static async Task<byte[]> CopyToAndHashAsync(this Stream source, Stream destination, int bufferSize, HashAlgorithm hasher, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (hasher == null) throw new ArgumentNullException(nameof(hasher));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            Debug.Assert(source.CanRead);
            Debug.Assert(destination.CanWrite);

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = await source.ReadAndHashAsync(buffer, 0, buffer.Length, hasher, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            }
            
            return hasher.Hash;
        }
    }
}