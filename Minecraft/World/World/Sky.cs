using OpenTK;

namespace Minecraft
{
    /// <summary>
    /// Responsible for storing and keeping track of the colors the different parts 
    /// of the sky should currently have.
    /// </summary>
    class Sky
    {
        /*
         * The colors of the different parts of the sky, the sun and moon and other elements can be changed 
         * by altering the color for the given hour. The sky will interpolate between the previous color
         * and the next color linearly for smooth transitions between the different colors.
         */

        private Vector3[] topSkyColors = new Vector3[24];
        private Vector3[] bottomSkyColors = new Vector3[24];
        private Vector3[] horizonColors = new Vector3[24];
        private Vector3[] sunColors = new Vector3[24];
        private Vector3[] sunGlowColors = new Vector3[24];
        private Vector3[] moonColors = new Vector3[24];
        private Vector3[] moonGlowColors = new Vector3[24];

        private readonly Vector3 invalidColor = new Vector3(-1, -1, -1);

        public Sky()
        {
            for(int i = 0; i < 24; i++)
            {
                topSkyColors[i] = invalidColor;
                bottomSkyColors[i] = invalidColor;
                horizonColors[i] = invalidColor;
                sunColors[i] = invalidColor;
                sunGlowColors[i] = invalidColor;
                moonColors[i] = invalidColor;
                moonGlowColors[i] = invalidColor;
            }

            topSkyColors[4] = new Vector3(0.024F, 0.059F, 0.133F);
            topSkyColors[6] = new Vector3(0.176F, 0.424F, 0.655F);
            topSkyColors[8] = new Vector3(0.04F, 0.509F, 0.875F);
            topSkyColors[16] = new Vector3(0.04F, 0.509F, 0.875F);
            topSkyColors[18] = new Vector3(0.176F, 0.424F, 0.655F);
            topSkyColors[20] = new Vector3(0.024F, 0.059F, 0.133F);

            bottomSkyColors[4] = new Vector3(0.014F, 0.029F, 0.103F);
            bottomSkyColors[6] = new Vector3(0.517F, 0.686F, 0.949F);
            bottomSkyColors[8] = new Vector3(0.565F, 0.855F, 0.969F);
            bottomSkyColors[16] = new Vector3(0.565F, 0.855F, 0.969F);
            bottomSkyColors[18] = new Vector3(0.517F, 0.686F, 0.949F);
            bottomSkyColors[20] = new Vector3(0.014F, 0.029F, 0.103F);

            horizonColors[4] = new Vector3(0.034F, 0.079F, 0.163F);
            horizonColors[6] = new Vector3(0.696F, 0.349F, 0.231F);
            horizonColors[8] = new Vector3(0.578F, 0.886F, 1.0F);
            horizonColors[16] = new Vector3(0.578F, 0.886F, 1.0F);
            horizonColors[18] = new Vector3(0.696F, 0.349F, 0.231F);
            horizonColors[20] = new Vector3(0.034F, 0.079F, 0.163F);

            sunColors[4] = new Vector3(0.034F, 0.079F, 0.163F);
            sunColors[6] = new Vector3(0.996F, 0.349F, 0.231F);
            sunColors[8] = new Vector3(1.0F, 1.0F, 0.8F);
            sunColors[16] = new Vector3(1.0F, 1.0F, 0.8F);
            sunColors[18] = new Vector3(0.996F, 0.349F, 0.231F);
            sunColors[20] = new Vector3(0.034F, 0.079F, 0.163F);

            sunGlowColors[4] = new Vector3(0.030F, 0.069F, 0.133F);
            sunGlowColors[6] = new Vector3(0.896F, 0.309F, 0.201F);
            sunGlowColors[8] = new Vector3(0.85F, 0.85F, 0.7F);
            sunGlowColors[16] = new Vector3(0.85F, 0.85F, 0.7F);
            sunGlowColors[18] = new Vector3(0.896F, 0.309F, 0.201F);
            sunGlowColors[20] = new Vector3(0.030F, 0.069F, 0.133F);

            moonColors[4] = new Vector3(1.0F, 1.0F, 1.0F);
            moonColors[6] = new Vector3(0.85F, 0.85F, 0.7F);
            moonColors[8] = new Vector3(1.0F, 1.0F, 1.0F);
            moonColors[16] = new Vector3(1.0F, 1.0F, 1.0F);
            moonColors[18] = new Vector3(0.85F, 0.85F, 0.7F);
            moonColors[20] = new Vector3(1.0F, 1.0F, 1.0F);

            moonGlowColors[4] = new Vector3(0.224F, 0.259F, 0.233F);
            moonGlowColors[6] = new Vector3(0.85F, 0.85F, 0.7F);
            moonGlowColors[8] = new Vector3(1.0F, 1.0F, 1.0F);
            moonGlowColors[16] = new Vector3(1.0F, 1.0F, 1.0F);
            moonGlowColors[18] = new Vector3(0.85F, 0.85F, 0.7F);
            moonGlowColors[20] = new Vector3(0.224F, 0.259F, 0.233F);
        }

        public void SetTopSkyColorTo(Vector3 color, int hour)    => topSkyColors[hour]    = color;
        public void SetBottomSkyColorTo(Vector3 color, int hour) => bottomSkyColors[hour] = color;
        public void SetHorizonColorTo(Vector3 color, int hour)   => horizonColors[hour]   = color;
        public void SetSunColorTo(Vector3 color, int hour)       => sunColors[hour]       = color;
        public void SetSunGlowColorTo(Vector3 color, int hour)   => sunGlowColors[hour]   = color;
        public void SetMoonColorTo(Vector3 color, int hour)      => moonColors[hour]      = color;
        public void SetMoonGlowColorTo(Vector3 color, int hour)  => moonGlowColors[hour]  = color;

        public Vector3 GetTopSkyColor(float hour)    => GetCurrentColorMix(topSkyColors, hour);
        public Vector3 GetBottomSkyColor(float hour) => GetCurrentColorMix(bottomSkyColors, hour);
        public Vector3 GetHorizonColor(float hour)   => GetCurrentColorMix(horizonColors, hour);
        public Vector3 GetSunColor(float hour)       => GetCurrentColorMix(sunColors, hour);
        public Vector3 GetSunGlowColor(float hour)   => GetCurrentColorMix(sunGlowColors, hour);
        public Vector3 GetMoonColor(float hour)      => GetCurrentColorMix(moonColors, hour);
        public Vector3 GetMoonGlowColor(float hour)  => GetCurrentColorMix(moonGlowColors, hour);

        private Vector3 GetCurrentColorMix(Vector3[] colors, float hour)
        {
            int prevColorIndex = FindPreviousColorIndex(colors, (int)hour);
            int nextColorIndex = FindNextColorIndex(colors, (int)hour);

            if(prevColorIndex == nextColorIndex)
                return colors[prevColorIndex];

            int hoursTillNextColor = nextColorIndex - prevColorIndex;
            if(hoursTillNextColor < 0)
                hoursTillNextColor += 24;

            int addition = 0;
            if(nextColorIndex < prevColorIndex && hour < nextColorIndex)
                addition = 24;

            float offset = (hour + addition - prevColorIndex) / hoursTillNextColor;
            return colors[prevColorIndex] * (1 - offset) + colors[nextColorIndex] * offset;
        }

        private int FindNextColorIndex(Vector3[] colors, int hour)
        {
            do
            {
                hour++;
                if(hour > 23)
                    hour = 0;
            } while(colors[hour] == invalidColor);
            return hour;
        }

        private int FindPreviousColorIndex(Vector3[] colors, int hour)
        {
            while(colors[hour] == invalidColor)
            {
                hour--;
                if(hour < 0)
                    hour = 23;
            }
            return hour;
        }
    }
}
