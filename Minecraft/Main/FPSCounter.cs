
namespace Minecraft
{
    class FPSCounter
    {
        private long totalElapsedFrames;
        private double totalElapsedTimeInSeconds;

        public void IncrementFrameCounter()
        {
            totalElapsedFrames++;
        }

        public void AddElapsedTime(double seconds)
        {
            totalElapsedTimeInSeconds += seconds;
        }

        public int GetAverageFPS()
        {
            return (int)(totalElapsedFrames / totalElapsedTimeInSeconds);
        }
    }
}
