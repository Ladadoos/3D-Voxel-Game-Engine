using OpenTK;

namespace Minecraft
{
    enum Direction : byte
    {
        Back = 0, //Side facing negative Z
        Right = 1, //Side facing negative X
        Front = 2, //Side facing positive Z
        Left = 3, //Side facing positive X
        Top = 4, //Side facing positive Y
        Bottom = 5 //Side facing negative Y
    };

    static class DirectionUtil
    {
        public static bool IsEncodedInByte(byte flag, Direction direction)
        {
            return (flag & (1 << (byte)direction)) > 0;
        }

        public static void EncodeInByte(ref byte flag, Direction direction)
        {
            flag |= (byte)(1 << (byte)direction);
        }

        private static Vector3 Back = new Vector3(0, 0, -1);
        private static Vector3 Right = new Vector3(-1, 0, 0);
        private static Vector3 Front = new Vector3(0, 0, 1);
        private static Vector3 Left = new Vector3(1, 0, 0);
        private static Vector3 Top = new Vector3(0, 1, 0);
        private static Vector3 Bottom = new Vector3(0, -1, 0);

        public static Direction Invert(Direction direction)
        {
            switch (direction)
            {
                case Direction.Back: return Direction.Front;
                case Direction.Front: return Direction.Back;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                case Direction.Top: return Direction.Bottom;
                case Direction.Bottom: return Direction.Top;
                default: throw new System.NotImplementedException();
            }
        }

        public static Vector3 ToUnit(Direction direction)
        {
            switch (direction)
            {
                case Direction.Back: return Back;
                case Direction.Front: return Front;
                case Direction.Left: return Left;
                case Direction.Right: return Right;
                case Direction.Top: return Top;
                case Direction.Bottom: return Bottom;
                default: throw new System.NotImplementedException();
            }
        }
    }

    struct Vector3Int
    {
        public int X, Y, Z;

        public Vector3Int(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public static Vector3Int operator +(Vector3Int a, Vector3Int b)
        {
            return new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }
}
