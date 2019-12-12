using OpenTK;

namespace Minecraft
{
    class Cube
    {
        /* 
         * Back   --- Negative Z
         * Front  --- Positive Z
         * Top    --- Positive Y
         * Bottom --- Negative Y
         * Left   --- Negative X
         * Right  --- Positive X
         */

        public static float[] GetCubeBackVertices(float x, float y, float z)
        {
            return new float[] {
                 //Back
                 x + 1, y + 0, z + 0, // 0 - 1
                 x + 0, y + 0, z + 0, // 2 - 3
                 x + 0, y + 1, z + 0, // 4 - 5
                 x + 1, y + 1, z + 0, // 6 - 7
              };
        }

        public static float[] GetCubeRightVertices(float x, float y, float z)
        {
            return new float[] {
                 //Right
                 x + 1, y + 0, z + 1, // 8 - 9
                 x + 1, y + 0, z + 0, // 10 - 11
                 x + 1, y + 1, z + 0, // 12 - 13
                 x + 1, y + 1, z + 1, // 14 - 15
              };
        }

        public static float[] GetCubeFrontVertices(float x, float y, float z)
        {
            return new float[] {
                 //Front
                 x + 0, y + 0, z + 1, // 16 - 17
                 x + 1, y + 0, z + 1, // 18 - 19
                 x + 1, y + 1, z + 1, // 20 - 21
                 x + 0, y + 1, z + 1, // 22 - 23
              };
        }

        public static float[] GetCubeLeftVertices(float x, float y, float z)
        {
            return new float[] {
                 //Left
                 x + 0, y + 0, z + 0, // 24 - 25
                 x + 0, y + 0, z + 1, // 26 - 27
                 x + 0, y + 1, z + 1, // 28 - 29
                 x + 0, y + 1, z + 0, // 30 - 31
              };
        }

        public static float[] GetCubeTopVertices(float x, float y, float z)
        {
            return new float[] {
                 //Top
                 x + 0, y + 1, z + 1, // 32 - 33
                 x + 1, y + 1, z + 1, // 34 - 35
                 x + 1, y + 1, z + 0, // 36 - 37
                 x + 0, y + 1, z + 0, // 38 - 39
              };
        }

        public static float[] GetCubeBottomVertices(float x, float y, float z)
        {
            return new float[] {
                 //Bottom
                 x + 0, y + 0, z + 0, // 40 - 41
                 x + 1, y + 0, z + 0, // 42 - 43
                 x + 1, y + 0, z + 1, // 44 - 45
                 x + 0, y + 0, z + 1, // 46 - 47
              };
        }

        public static float[] GetCubeVerticesForSide(Direction side, float x, float y, float z)
        {
            if (side == Direction.Back)
            {
                return GetCubeBackVertices(x, y, z);
            }
            else if (side == Direction.Right)
            {
                return GetCubeRightVertices(x, y, z);
            }
            else if (side == Direction.Front)
            {
                return GetCubeFrontVertices(x, y, z);
            }
            else if (side == Direction.Left)
            {
                return GetCubeLeftVertices(x, y, z);
            }
            else if (side == Direction.Top)
            {
                return GetCubeTopVertices(x, y, z);
            }
            else if (side == Direction.Bottom)
            {
                return GetCubeBottomVertices(x, y, z);
            }
            return null;
        }

        public static int[] GetOriginIndices()
        {
            return new int[] {
                 0, 1, 2,
                 2, 3, 0,

                 4, 5, 6,
                 6, 7, 4,

                 8, 9, 10,
                 10, 11, 8,

                 12, 13, 14,
                 14, 15, 12,

                 16, 17, 18,
                 18, 19, 16,

                 20, 21, 22,
                 22, 23, 20
             };
        }
    }
}
