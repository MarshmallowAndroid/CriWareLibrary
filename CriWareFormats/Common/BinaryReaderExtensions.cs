using System;
using System.IO;

namespace CriWareFormats.Common
{
    public static class BinaryReaderExtensions
    {
        public static short ReadInt16BE(this BinaryReader binaryReader)
        {
            ushort le = binaryReader.ReadUInt16();
            return (short)Endian.Reverse(le);
        }

        public static ushort ReadUInt16BE(this BinaryReader binaryReader)
        {
            ushort le = binaryReader.ReadUInt16();
            return Endian.Reverse(le);
        }

        public static int ReadInt32BE(this BinaryReader binaryReader)
        {
            uint le = binaryReader.ReadUInt32();
            return (int)Endian.Reverse(le);
        }

        public static uint ReadUInt32BE(this BinaryReader binaryReader)
        {
            uint le = binaryReader.ReadUInt32();
            return Endian.Reverse(le);
        }

        public static long ReadInt64BE(this BinaryReader binaryReader)
        {
            ulong le = binaryReader.ReadUInt64();
            return (long)Endian.Reverse(le);
        }

        public static ulong ReadUInt64BE(this BinaryReader binaryReader)
        {
            ulong le = binaryReader.ReadUInt64();
            return Endian.Reverse(le);
        }

        public static float ReadSingleBE(this BinaryReader binaryReader)
        {
            float le = binaryReader.ReadSingle();
            byte[] floatBytes = BitConverter.GetBytes(le);
            byte[] reversed = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                reversed[i] = floatBytes[3 - i];
            }
            return BitConverter.ToSingle(reversed, 0);
        }
    }
}