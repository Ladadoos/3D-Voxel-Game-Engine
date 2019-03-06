using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    struct ProjectionMatrixInfo
    {
        public float distanceNearPlane;
        public float distanceFarPlane;
        public float fieldOfView;
        public int windowWidth;
        public int windowHeight;

        public ProjectionMatrixInfo(float distanceNearPlane, float distanceFarPlane, float fieldOfView, int windowWidth, int windowHeight)
        {
            this.distanceNearPlane = distanceNearPlane;
            this.distanceFarPlane = distanceFarPlane;
            this.fieldOfView = fieldOfView;
            this.windowWidth = windowWidth;
            this.windowHeight = windowHeight;
        }
    }
}
