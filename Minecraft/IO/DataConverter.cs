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

        public static ushort BytesToUInt16(byte[] bytes, ref int head)
        {
            //Little endian
            ushort value = (ushort)(bytes[head] | (bytes[head + 1] << 8));
            head += 2;
            return value;
        }

        public static int BytesToInt32(byte[] bytes, ref int head)
        {
            //Little endian
            int value = bytes[head] | (bytes[head + 1] << 8) | (bytes[head + 1] << 16) | (bytes[head + 1] << 24);
            head += 4;
            return value;
        }

        public static unsafe float BytesToFloat(byte[] bytes, ref int head)
        {
            //Little endian
            int value = BytesToInt32(bytes, ref head);
            return *(float*)&value;
        }

        public static bool BytesToBool(byte[] bytes, ref int head)
        {
            bool value = bytes[head] == 0 ? false : true;
            head += 1;
            return value;
        }

        public static Vector3i BytesToVector3i(byte[] bytes, ref int head)
        {
            int x = BytesToInt32(bytes, ref head);
            int y = BytesToInt32(bytes, ref head);
            int z = BytesToInt32(bytes, ref head);
            return new Vector3i(x, y, z);
        }

        public static Chunk BytesToChunk(byte[] bytes, ref int head)
        {
            int gridX = BytesToInt32(bytes, ref head);
            int gridZ = BytesToInt32(bytes, ref head);

            Chunk chunk = new Chunk(gridX, gridZ);

            for(int i = 0; i < Constants.NUM_SECTIONS_IN_CHUNKS; i++)
            {
                bool doesSectionHaveBlocks = BytesToBool(bytes, ref head);
                if(doesSectionHaveBlocks)
                {
                    for(int x = 0; x < 16; x++)
                    {
                        for(int y = 0; y < 16; y++)
                        {
                            for(int z = 0; z < 16; z++)
                            {
                                ushort blockId = BytesToUInt16(bytes, ref head);
                                if(blockId != 0)
                                {
                                    BlockState blockState = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();
                                    blockState.ExtractFromByteStream(bytes, ref head);
                                    chunk.AddBlockAt(x, i * 16 + y, z, blockState);
                                }
                            }
                        }
                    }
                }
            }
            return chunk;
        }
    }
}
