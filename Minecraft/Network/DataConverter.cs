using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Minecraft
{
    static class DataConverter
    {
        private static Encoding utf8 = new UTF8Encoding();

        public static byte[] StringUtf8ToBytes(string value)
        {
            return utf8.GetBytes(value);        
        }

        public static string BytesToUtf8String(byte[] bytes)
        {
            return utf8.GetString(bytes);
        }

        public static ushort BytesToUInt16(byte[] bytes, int source)
        {
            //Little endian
            return (ushort)(bytes[source] | (bytes[source + 1] << 8));
        }

        public static int BytesToInt32(byte[] bytes, int source)
        {
            //Little endian
            return (ushort)(bytes[source] | (bytes[source + 1] << 8) | (bytes[source + 1] << 16) | (bytes[source + 1] << 24));
        }

        public static unsafe float BytesToFloat(byte[] bytes, int source)
        {
            //Little endian
            int value = BytesToInt32(bytes, source);
            return *(float*)&value;
        }

        public static bool BytesToBool(byte[] bytes, int source)
        {
            return bytes[source] == 0 ? false : true;
        }

        public static Vector3i BytesToVector3i(byte[] bytes, int source)
        {
            int x = BytesToInt32(bytes, source);
            int y = BytesToInt32(bytes, source + 4);
            int z = BytesToInt32(bytes, source + 8);
            return new Vector3i(x, y, z);
        }
    }
}
