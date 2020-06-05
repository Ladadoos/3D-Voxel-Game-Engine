using System;

namespace Minecraft
{
    struct Light
    {
        /* 0000 0000 0000 0000 0000 0000 0000 0000
         * ---- ---- SSSS IIII IIII BBBB GGGG RRRR
         * S = sun light occlusion, 4 bits, values between 0 and 15
         * I = illumination (brightness), 1 byte, values between 0 and 255
         * R = red color channel, 4 bits, values between 0 and 15
         * G = green color channel, 4 bits, values between 0 and 15
         * B = blue color channel, 4 bits, values between 0 and 15
         */

        private uint storage;

        public uint GetStorage() => storage;

        /*
         * Brightness
         */
        public void SetBrightness(uint brightness)
        {
            if(brightness < 0 || brightness > 15)
                throw new Exception("Invalid brightness of " + brightness);

            storage = (storage & 0xFFF00FFF) | (brightness << 12);
        }

        public uint GetBrightness()
        {
            return (storage >> 12) & 0xFF;
        }

        public void ConvertAndSetBrightness(float brightness)
        {
            SetBrightness((uint)Maths.ConvertRange(0, 1, 0, 15, brightness));
        }

        /*
         * Red channel
         */
        public void SetRedChannel(uint redChannel)
        {
            if(redChannel < 0 || redChannel > 15)
                throw new Exception("Invalid red channel of " + redChannel);

            storage = (storage & 0xFFFFFFF0) | redChannel;
        }

        public uint GetRedChannel()
        {
            return storage & 0xF;
        }

        public void ConvertAndSetRedChannel(float redChannel)
        {
            SetRedChannel((uint)Maths.ConvertRange(0, 1, 0, 15, redChannel));
        }

        /*
         * Green channel
         */
        public void SetGreenChannel(uint greenChannel)
        {
            if(greenChannel < 0 || greenChannel > 15)
                throw new Exception("Invalid green channel of " + greenChannel);

            storage = (storage & 0xFFFFFF0F) | (greenChannel << 4);
        }

        public uint GetGreenChannel()
        {
            return (storage >> 4) & 0xF;
        }

        public void ConvertAndSetGreenChannel(float greenChannel)
        {
            SetGreenChannel((uint)Maths.ConvertRange(0, 1, 0, 15, greenChannel));
        }

        /*
         * Blue channel
         */
        public void SetBlueChannel(uint blueChannel)
        {
            if(blueChannel < 0 || blueChannel > 15)
                throw new Exception("Invalid blue channel of " + blueChannel);

            storage = (storage & 0xFFFFF0FF) | (blueChannel << 8);
        }

        public uint GetBlueChannel()
        {
            return (storage >> 8) & 0xF;
        }

        public void ConvertAndSetBlueChannel(float blueChannel)
        {
            SetBlueChannel((uint)Maths.ConvertRange(0, 1, 0, 15, blueChannel));
        }

        /*
        * Sun light
        */
        public void SetSunlight(uint brightness)
        {
            if(brightness < 0 || brightness > 15)
                throw new Exception("Invalid brightness of " + brightness);

            storage = (storage & 0xFF0FFFFF) | (brightness << 20);
        }

        public uint GetSunlight()
        {
            return (storage >> 20) & 0xF;
        }

        public void ConvertAndSetSunlight(float brightness)
        {
            SetSunlight((uint)Maths.ConvertRange(0, 1, 0, 15, brightness));
        }
    }
}
