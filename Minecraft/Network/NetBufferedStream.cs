using System;
using System.IO;

namespace Minecraft
{
    class NetBufferedStream
    {
        public BufferedStream bufferedStream;

        public NetBufferedStream(BufferedStream bufferedStream)
        {
            this.bufferedStream = bufferedStream;
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
            byte[] bytes = DataConverter.Int32ToBytes(value);
            bufferedStream.Write(bytes, 0, bytes.Length);
        }

        public void WriteFloat(float value)
        {
            byte[] bytes = DataConverter.FloatToBytes(value);
            bufferedStream.Write(bytes, 0, bytes.Length);
        }

        public void WriteBool(bool value)
        {
            byte[] boolByte = new byte[] { value ? (byte)1 : (byte)0 };
            bufferedStream.Write(boolByte, 0, 1);
        }

        public void WriteUtf8String(string value)
        {
            byte[] messageBytes = DataConverter.StringUtf8ToBytes(value);
            byte[] byteCount = DataConverter.Int32ToBytes(messageBytes.Length);
            bufferedStream.Write(byteCount, 0, byteCount.Length);
            bufferedStream.Write(messageBytes, 0, messageBytes.Length);
        }

        public void WriteVector3i(Vector3i value)
        {
            WriteInt32(value.X);
            WriteInt32(value.Y);
            WriteInt32(value.Z);
        }

        public void WriteBytes(byte[] value)
        {
            bufferedStream.Write(value, 0, value.Length);
        }
    }
}
