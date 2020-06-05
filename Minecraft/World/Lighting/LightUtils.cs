using System;

namespace Minecraft
{
    static class LightUtils
    {
        /// <summary>
        /// The three color channels (R, G, B) in which block light propagates.
        /// </summary>
        public static LightChannel[] BlockVisibileColorChannels = new LightChannel[3]
            { LightChannel.Red, LightChannel.Green, LightChannel.Blue };

        /// <summary>
        /// Returns the color of the given channel from the given light source
        /// </summary>
        public static uint GetChannelColor(ILightSource source, LightChannel channel)
        {
            switch(channel)
            {
                case LightChannel.Red:   return (uint)source.LightColor.X;
                case LightChannel.Green: return (uint)source.LightColor.Y;
                case LightChannel.Blue:  return (uint)source.LightColor.Z;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns the color of the given channel at the given local position in the chunk's lightmap.
        /// </summary>
        public static uint GetLightOfChannel(Chunk chunk, Vector3i chunkLocalPos, LightChannel channel)
        {
            switch(channel)
            {
                case LightChannel.Red:   return chunk.LightMap.GetRedBlockLightAt(chunkLocalPos);
                case LightChannel.Green: return chunk.LightMap.GetGreenBlockLightAt(chunkLocalPos);
                case LightChannel.Blue:  return chunk.LightMap.GetBlueBlockLightAt(chunkLocalPos);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets the color of the given channel at the given location in the chunk's lightmap.
        /// </summary>
        public static void SetLightOfChannel(Chunk chunk, Vector3i chunkLocalPos, LightChannel channel, uint value)
        {
            switch(channel)
            {
                case LightChannel.Red:
                    chunk.LightMap.SetRedBlockLightAt(chunkLocalPos, value);
                    break;
                case LightChannel.Green:
                    chunk.LightMap.SetGreenBlockLightAt(chunkLocalPos, value);
                    break;
                case LightChannel.Blue:
                    chunk.LightMap.SetBlueBlockLightAt(chunkLocalPos, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
