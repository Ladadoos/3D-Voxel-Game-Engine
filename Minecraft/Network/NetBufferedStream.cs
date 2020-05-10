using OpenTK;
using System;
using System.IO;

namespace Minecraft
{
    class NetBufferedStream
    {
        private readonly BufferedStream bufferedStream;

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
            } catch(Exception e)
            {
                Logger.Error("Flushing failed: " + e.Message);
                return false;
            }
        }

        public unsafe void WriteInt32(int value)
        {
            byte* pValue = (byte*)&value;
            bufferedStream.WriteByte(pValue[0]);
            bufferedStream.WriteByte(pValue[1]);
            bufferedStream.WriteByte(pValue[2]);
            bufferedStream.WriteByte(pValue[3]);
        }

        public unsafe void WriteFloat(float value)
        {
            WriteInt32(*(int*)&value);
        }

        public unsafe void WriteBool(bool value)
        {
            bufferedStream.WriteByte(((byte*)&value)[0]);
        }

        public unsafe void WriteInt16(short value)
        {
            byte* pValue = (byte*)&value;
            bufferedStream.WriteByte(pValue[0]);
            bufferedStream.WriteByte(pValue[1]);
        }

        public unsafe void WriteUInt16(ushort value)
        {
            byte* pValue = (byte*)&value;
            bufferedStream.WriteByte(pValue[0]);
            bufferedStream.WriteByte(pValue[1]);
        }

        public void WriteUtf8String(string value)
        {
            byte[] messageBytes = DataConverter.StringUtf8ToBytes(value);
            WriteInt32(messageBytes.Length);
            bufferedStream.Write(messageBytes, 0, messageBytes.Length);
        }

        public void WriteVector3i(Vector3i value)
        {
            WriteInt32(value.X);
            WriteInt32(value.Y);
            WriteInt32(value.Z);
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.X);
            WriteFloat(value.Y);
            WriteFloat(value.Z);
        }

        public void WriteBytes(byte[] value)
        {
            bufferedStream.Write(value, 0, value.Length);
        }
    }
}
