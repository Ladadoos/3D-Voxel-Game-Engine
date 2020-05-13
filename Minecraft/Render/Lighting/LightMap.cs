namespace Minecraft
{
    class LightMap
    {
        //0000 0000 0000 0000 0000 0000 0000 0000
        //XXXX YYYY ---- ---- ---- ---- ---- ----
        //X = light caused by blocks, from 0 to 15
        //Y = light caused by the sun, from 0 to 15

        private uint[] map = new uint[16 * 16 * 256];

        public void Reset()
        {
            for(uint x = 0; x < 16; x++)
            {
                for(uint z = 0; z < 16; z++)
                {
                    for(uint y = 0; y < 256; y++)
                    {
                        SetBlockLightAt(x, y, z, 0);
                    }
                }
            }
        }

        public void SetBlockLightAt(uint localX, uint worldY, uint localZ, uint lightValue)
        {
            if(lightValue < 0 || lightValue > 15)
                throw new System.Exception("Light was " + lightValue);

            map[(worldY << 8) + (localX << 4) + localZ] = lightValue << 28; 
            uint light = GetBlockLightAt(localX, worldY, localZ);

            if(light != lightValue)
                throw new System.Exception("Light was " + light + " but should be " + lightValue);
        }

        public uint GetBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return (map[(worldY << 8) + (localX << 4) + localZ] & 0xF0000000) >> 28;
        }

        public void SetBlockLightAt(Vector3i localPos, uint lightValue)
        {
            SetBlockLightAt((uint)localPos.X, (uint)localPos.Y, (uint)localPos.Z, lightValue);
        }

        public uint GetBlockLightAt(Vector3i localPos)
        {
            return GetBlockLightAt((uint)localPos.X, (uint)localPos.Y, (uint)localPos.Z);
        }

        public void SetSunLightAt(int localX, int worldY, int localZ, uint lightValue)
        {
            map[(localX << 8) + (worldY << 4) + localZ] = lightValue << 24;
        }

        public uint GetSunLightAt(int localX, int worldY, int localZ)
        {
            return (map[(localX << 8) + (worldY << 4) + localZ] & 0x0F000000) >> 24;
        }
    }
}
