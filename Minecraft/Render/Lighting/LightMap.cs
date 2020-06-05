using OpenTK;

namespace Minecraft
{
    class LightMap
    {
        //TODO Consider switching over to ushort to save on 2 bytes per entry.

        //0000 0000 0000 0000 0000 0000 0000 0000
        //                    SSSS BBBB GGGG RRRR
        //R = red color channel, 0 to 15
        //G = green color channel, 0 to 15
        //B = blue color channel, 0 to 15
        //S = sunlight, 0 to 15

        private uint[] map = new uint[16 * 16 * 256];

        public void ClearSunlightMap()
        {
            for(uint x = 0; x < 16; x++)
                for(uint z = 0; z < 16; z++)
                    for(uint y = 0; y < 256; y++)
                        SetSunLightIntensityAt(x, y, z, 0);
        }

        /*
         * Red channel
         */
        public void SetRedBlockLightAt(uint localX, uint worldY, uint localZ, uint lightValue)
        {
            map[(worldY << 8) + (localX << 4) + localZ] = (map[(worldY << 8) + (localX << 4) + localZ] & 0xFFFFFFF0) | lightValue;
        }

        public uint GetRedBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return map[(worldY << 8) + (localX << 4) + localZ] & 0xF;
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
            map[(worldY << 8) + (localX << 4) + localZ] = (map[(worldY << 8) + (localX << 4) + localZ] & 0xFFFFFF0F) | (lightValue << 4);
        }

        public uint GetGreenBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return (map[(worldY << 8) + (localX << 4) + localZ] >> 4) & 0xF;
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
            map[(worldY << 8) + (localX << 4) + localZ] = (map[(worldY << 8) + (localX << 4) + localZ] & 0xFFFFF0FF) | (lightValue << 8);
        }

        public uint GetBlueBlockLightAt(uint localX, uint worldY, uint localZ)
        {
            return (map[(worldY << 8) + (localX << 4) + localZ] >> 8) & 0xF;
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
            map[(worldY << 8) + (localX << 4) + localZ] = (map[(worldY << 8) + (localX << 4) + localZ] & 0xFFFF0FFF) | (lightValue << 12);
        }

        public uint GetSunLightIntensityAt(uint localX, uint worldY, uint localZ)
        {
            return (map[(worldY << 8) + (localX << 4) + localZ] >> 12) & 0xF;
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
        public Light GetLightColorAt(uint localX, uint worldY, uint localZ)
        {
            Light light = new Light();
            light.SetRedChannel(GetRedBlockLightAt(localX, worldY, localZ));
            light.SetGreenChannel(GetGreenBlockLightAt(localX, worldY, localZ));
            light.SetBlueChannel(GetBlueBlockLightAt(localX, worldY, localZ));
            return light;
        }
    }
}