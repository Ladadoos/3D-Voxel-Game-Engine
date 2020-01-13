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

        public static byte[] Int32ToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] FloatToBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static object ByteArrayToObject(byte[] byteArray)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(byteArray, 0, byteArray.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
