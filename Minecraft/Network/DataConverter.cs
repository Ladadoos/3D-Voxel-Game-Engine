using System;
using System.Text;

namespace Minecraft
{
    class DataConverter
    {
        private Encoding utf8 = new UTF8Encoding();

        public byte[] StringUtf8ToBytes(string value)
        {
            return utf8.GetBytes(value);        
        }

        public string BytesToUtf8String(byte[] bytes)
        {
            return utf8.GetString(bytes);
        }

        public byte[] Int32ToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] FloatToBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }
    }
}
