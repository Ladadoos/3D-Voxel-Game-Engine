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

        public void FlushToSocket()
        {
            bufferedStream.Flush();
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

        public void WriteUtf8String(string value)
        {
            byte[] messageBytes = converter.StringUtf8ToBytes(value);
            byte[] byteCount = converter.Int32ToBytes(messageBytes.Length);
            bufferedStream.Write(byteCount, 0, byteCount.Length);
            bufferedStream.Write(messageBytes, 0, messageBytes.Length);
        }
    }
}
