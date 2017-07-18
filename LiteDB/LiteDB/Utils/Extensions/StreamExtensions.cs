﻿using System;
using System.IO;

namespace LiteDB
{
    internal static class StreamExtensions
    {
        public static byte ReadByte(this Stream stream, long position)
        {
            var buffer = new byte[1];
            stream.Seek(position, SeekOrigin.Begin);
            stream.Read(buffer, 0, 1);
            return buffer[0];
        }

        public static void WriteByte(this Stream stream, long position, byte value)
        {
            stream.Seek(position, SeekOrigin.Begin);
            stream.Write(new byte[] { value }, 0, 1);
        }

        public static void CopyTo(this Stream input, Stream output)
        {
            var buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// Try unlock stream segment. Do nothing if was not possible (it's not locked)
        /// </summary>
        public static bool TryUnlock(this FileStream stream, long position, long length)
        {
            try
            {
#if NET35
                stream.Unlock(position, length);
#endif
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Try lock a stream segment during timeout.
        /// </summary>
        public static void TryLock(this FileStream stream, long position, long length, TimeSpan timeout)
        {
            FileHelper.TryExec(() =>
            {
#if NET35
                stream.Lock(position, length);
#endif
            }, timeout);
        }
    }
}