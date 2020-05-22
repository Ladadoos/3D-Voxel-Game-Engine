using OpenTK;
using System;

namespace Minecraft
{
    class Environment
    {
        /// <summary>
        /// The current time in seconds.
        /// </summary>
        public float CurrentTime { get; set; }

        /// <summary>
        /// The total amount of time in an ingame day in seconds.
        /// </summary>
        public int TimeInDay { get; private set; }

        /// <summary>
        /// The current normalized position of the sun on the skydome.
        /// </summary>
        public Vector3 SunPosition { get; set; }

        /// <summary>
        /// The ambient color that is applied to all fragments in the scene.
        /// </summary>
        public Vector3 AmbientColor { get; set; }
    
        /// <summary>
        /// The position of the sun in terms of rotation around the center of the world/sky dome.
        /// </summary>
        private double sunRotationRads = 0;

        private Sky sky;

        public Environment(int timeInDaySeconds)
        {
            if(timeInDaySeconds <= 0)
                throw new ArgumentOutOfRangeException(timeInDaySeconds + " is not valid day length");

            TimeInDay = timeInDaySeconds;
            sky = new Sky();
        }

        public Vector3 GetCurrentTopSkyColor()    => sky.GetTopSkyColor(GetCurrentEarthTime());
        public Vector3 GetCurrentBottomSkyColor() => sky.GetBottomSkyColor(GetCurrentEarthTime());
        public Vector3 GetCurrentHorizonColor()   => sky.GetHorizonColor(GetCurrentEarthTime());
        public Vector3 GetCurrentSunColor()       => sky.GetSunColor(GetCurrentEarthTime());
        public Vector3 GetCurrentSunGlowColor()   => sky.GetSunGlowColor(GetCurrentEarthTime());
        public Vector3 GetCurrentMoonColor()      => sky.GetMoonColor(GetCurrentEarthTime());
        public Vector3 GetCurrentMoonGlowColor()  => sky.GetMoonGlowColor(GetCurrentEarthTime());

        /// <summary>
        /// Returns the current time scaled to earth time, so 24 hour long days
        /// </summary>
        private float GetCurrentEarthTime() => CurrentTime * 24.0F / TimeInDay;

        public void Update(float deltaTimeSeconds)
        {
            CurrentTime += deltaTimeSeconds;
            if(CurrentTime >= TimeInDay)
            {
                CurrentTime = 0;
            }

            //Determine the current position on the unit sphere, add an offset to make angle
            //to make the sun rise at 6AM and set at start setting at 6PM
            double sunRiseAllignmentOffset = ((TimeInDay / 24.0F * 6.0F) / TimeInDay) * Math.PI * 2;
            sunRotationRads = (CurrentTime / TimeInDay) * Math.PI * 2 - sunRiseAllignmentOffset;

            Vector3 newSunPosition = new Vector3((float)Math.Cos(sunRotationRads), (float)Math.Sin(sunRotationRads), 0).Normalized();
            SunPosition = newSunPosition;
        }
    }
}
