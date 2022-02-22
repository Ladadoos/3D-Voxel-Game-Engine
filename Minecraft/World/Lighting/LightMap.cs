namespace Minecraft
{
    class LightMap
    {
        //0000 0000 0000 0000
        //SSSS BBBB GGGG RRRR
        //R = red color channel, 0 to 15
        //G = green color channel, 0 to 15
        //B = blue color channel, 0 to 15
        //S = sunlight, 0 to 15

        private ushort[] map = new ushort[16 * 16 * Constants.MAX_BUILD_HEIGHT];

        public void ClearSunlightMap()
        {
            for(uint x = 0; x < 16; x++)
                for(uint z = 0; z < 16; z++)
                    for(uint y = 0; y < 256; y++)
                    {
                        SetRedBlockLightAt(x, y, z, 0);
                        SetGreenBlockLightAt(x, y, z, 0);
                        SetBlueBlockLightAt(x, y, z, 0);
                        SetSunLightIntensityAt(x, y, z, 0);
                    }
        }

        /*
         * Red channel
         */
        public void SetRedBlockLightAt(uint localX, uint worldY, uint localZ, uint lightValue)
        {
            map[(worldY << 8) + (localX << 4) + localZ] = (ushort)((map[(worldY << 8) + (localX << 4) + localZ] & 0xFFF0) | (ushort)lightValue);
        }

        public uint GetRedBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return (uint)(map[(worldY << 8) + (localX << 4) + localZ] & 0xF);
        }

        public void SetRedBlockLightAt(Vector3i chunkLocalPos, uint lightValue)
        {
            SetRedBlockLightAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z, lightValue);
        }

        public uint GetRedBlockLightAt(Vector3i chunkLocalPos)
        {
            return GetRedBlockLightAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z);
        }

        /*
         * Green channel
         */
        public void SetGreenBlockLightAt(uint localX, uint worldY, uint localZ, uint lightValue)
        {
            map[(worldY << 8) + (localX << 4) + localZ] = (ushort)((map[(worldY << 8) + (localX << 4) + localZ] & 0xFF0F) | (ushort)(lightValue << 4));
        }

        public uint GetGreenBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return (uint)(map[(worldY << 8) + (localX << 4) + localZ] >> 4) & 0xF;
        }

        public void SetGreenBlockLightAt(Vector3i chunkLocalPos, uint lightValue)
        {
            SetGreenBlockLightAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z, lightValue);
        }

        public uint GetGreenBlockLightAt(Vector3i chunkLocalPos)
        {
            return GetGreenBlockLightAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z);
        }

        /*
         * Blue channel
         */
        public void SetBlueBlockLightAt(uint localX, uint worldY, uint localZ, uint lightValue)
        {
            map[(worldY << 8) + (localX << 4) + localZ] = (ushort)((map[(worldY << 8) + (localX << 4) + localZ] & 0xF0FF) | (ushort)(lightValue << 8));
        }

        public uint GetBlueBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return (uint)(map[(worldY << 8) + (localX << 4) + localZ] >> 8) & 0xF;
        }

        public void SetBlueBlockLightAt(Vector3i chunkLocalPos, uint lightValue)
        {
            SetBlueBlockLightAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z, lightValue);
        }

        public uint GetBlueBlockLightAt(Vector3i chunkLocalPos)
        {
            return GetBlueBlockLightAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z);
        }

        /*
         * Sun light intensity
         */
        public void SetSunLightIntensityAt(uint localX, uint worldY, uint localZ, uint lightValue)
        {
            map[(worldY << 8) + (localX << 4) + localZ] = (ushort)((map[(worldY << 8) + (localX << 4) + localZ] & 0x0FFF) | (ushort)(lightValue << 12));
        }

        public uint GetSunLightIntensityAt(uint localX, uint worldY, uint localZ)
        {
            return (uint)(map[(worldY << 8) + (localX << 4) + localZ] >> 12) & 0xF;
        }

        public void SetSunLightIntensityAt(Vector3i chunkLocalPos, uint lightValue)
        {
            SetSunLightIntensityAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z, lightValue);
        }

        public uint GetSunLightIntensityAt(Vector3i chunkLocalPos)
        {
            return GetSunLightIntensityAt((uint)chunkLocalPos.X, (uint)chunkLocalPos.Y, (uint)chunkLocalPos.Z);
        }

        /*
         * General
         */

        /// <summary>
        /// Returns the red, green and blue channel at the given coordinates and sets them in the returned light.
        /// </summary>
        public Light GetLightColorAt(uint localX, uint worldY, uint localZ, uint mult = 1)
        {
            Light light = new Light();
            light.SetRedChannel(GetRedBlockLightAt(localX, worldY, localZ) * mult);
            light.SetGreenChannel(GetGreenBlockLightAt(localX, worldY, localZ) * mult);
            light.SetBlueChannel(GetBlueBlockLightAt(localX, worldY, localZ) * mult);
            light.SetSunlight(GetSunLightIntensityAt(localX, worldY, localZ) * mult);
            return light;
        }

        public Light GetLightColorAt(Vector3i position, uint mult = 1)
        {
            return GetLightColorAt((uint)position.X, (uint)position.Y, (uint)position.Z, mult);
        }
    }
}