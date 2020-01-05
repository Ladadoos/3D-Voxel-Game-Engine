using System;
using System.IO;

namespace Minecraft
{
    class NetBufferedStream
    {
        private BufferedStream bufferedStream;
        private DataConverter converter;

        public NetBufferedStream(BufferedStream bufferedStream)
        {
            this.bufferedStream = bufferedStream;
            converter = new DataConverter();
        }

        public bool FlushToSocket()
        {
            try
            {
                bufferedStream.Flush();
                return true;
            }catch(Exception e)
            {
                Logger.Error("Flushing failed: " + e.Message);
                return false;
            }
        }

        public void WriteInt32(int value)
        {
            byte[] bytes = converter.Int32ToBytes(value);
            bufferedStream.Write(bytes, 0, bytes.Length);
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = converter.FloatToBytes(value);
            bufferedStream.Write(bytes, 0, bytes.Length);
        }

        public void WriteBool(bool value)
        {
            byte[] boolByte = new byte[] { value ? (byte)1 : (byte)0 };
            bufferedStream.Write(boolByte, 0, 1);
        }

        public void WriteUtf8String(string value)
        {
            byte[] messageBytes = converter.StringUtf8ToBytes(value);
            byte[] byteCount = converter.Int32ToBytes(messageBytes.Length);
            bufferedStream.Write(byteCount, 0, byteCount.Length);
            bufferedStream.Write(messageBytes, 0, messageBytes.Length);
        }
    }
}
