using System;

namespace Minecraft
{
    struct Light
    {
        /* 0000 0000 0000 0000 0000 0000 0000 0000
        *    II IIII SSSS SSBB BBBB GGGG GGRR RRRR
        *  I = illumation, S = sunlight, RGB = red, green, blue block channel
        */
        private uint storage;

        public uint GetStorage() => storage;

        public Light(uint r, uint g, uint b, uint s, uint br) : this()
        {
            SetRedChannel(r);
            SetGreenChannel(g);
            SetBlueChannel(b);
            SetSunlight(s);
            SetBrightness(br);
        }

        /*
         * Brightness
         */
        public void SetBrightness(uint brightness)
        {
            if(brightness < 0 || brightness > 127)
                throw new Exception("Invalid brightness of " + brightness);
            storage = (storage & 0xC0FFFFFF) | (brightness << 24);
        }
        public uint GetBrightness() => (storage >> 24) & 0x3F;

        /*
         * Red channel
         */
        public void SetRedChannel(uint redChannel)
        {
            if(redChannel < 0 || redChannel > 127)
                throw new Exception("Invalid red channel of " + redChannel);
            storage = (storage & 0xFFFFFFC0) | redChannel;
        }
        public uint GetRedChannel() => storage & 0x3F;

        /*
         * Green channel
         */
        public void SetGreenChannel(uint greenChannel)
        {
            if(greenChannel < 0 || greenChannel > 127)
                throw new Exception("Invalid green channel of " + greenChannel);
            storage = (storage & 0xFFFFF03F) | (greenChannel << 6);
        }
        public uint GetGreenChannel() => (storage >> 6) & 0x3F;

        /*
         * Blue channel
         */
        public void SetBlueChannel(uint blueChannel)
        {
            if(blueChannel < 0 || blueChannel > 127)
                throw new Exception("Invalid blue channel of " + blueChannel);
            storage = (storage & 0xFFFC0FFF) | (blueChannel << 12);
        }
        public uint GetBlueChannel() => (storage >> 12) & 0x3F;

        /*
        * Sun light
        */
        public void SetSunlight(uint brightness)
        {
            if(brightness < 0 || brightness > 127)
                throw new Exception("Invalid sunlight of " + brightness);
            storage = (storage & 0xFF03FFFF) | (brightness << 18);
        }
        public uint GetSunlight() => (storage >> 18) & 0x3F;

        public static (uint r, uint g, uint b, uint s, uint br) Add(Light l1, Light l2)
        {
            return (l1.GetRedChannel() + l2.GetRedChannel(), l1.GetGreenChannel() + l2.GetGreenChannel(),
                    l1.GetBlueChannel() + l2.GetBlueChannel(), l1.GetSunlight() + l2.GetSunlight(),
                    l1.GetBrightness() + l2.GetBrightness());
        }
    }
}
