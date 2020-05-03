using OpenTK;

namespace Minecraft
{
    struct Vector3i
    {
        public readonly static Vector3i NorthBasis = new Vector3i(1, 0, 0);
        public readonly static Vector3i SouthBasis = new Vector3i(-1, 0, 0);
        public readonly static Vector3i EastBasis = new Vector3i(0, 0, 1);
        public readonly static Vector3i WestBasis = new Vector3i(0, 0, -1);

        public int X;
        public int Y;
        public int Z;

        public Vector3i(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public Vector3i(Vector3 vector3f, bool snapToGrid = true)
        {
            if(snapToGrid)
            {
                X = vector3f.X < 0 ? (int)(vector3f.X - 1) : (int)(vector3f.X);
                Y = vector3f.Y < 0 ? (int)(vector3f.Y - 1) : (int)(vector3f.Y);
                Z = vector3f.Z < 0 ? (int)(vector3f.Z - 1) : (int)(vector3f.Z);
            } else
            {
                X = (int)vector3f.X;
                Y = (int)vector3f.Y;
                Z = (int)vector3f.Z;
            }
        }

        public Vector3 ToFloat()
        {
            return new Vector3(X, Y, Z);
        }

        public static Vector3i operator+(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3i operator -(Vector3i a, Vector3i b)
        {
            return new Vector3i(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return "Vector3i(" + X + "," + Y + "," + Z + ")";
        }

        public Vector3i Up()
        {
            return new Vector3i(X, Y + 1, Z);
        }

        public Vector3i Down()
        {
            return new Vector3i(X, Y - 1, Z);
        }

        public Vector3i East()
        {
            return this + EastBasis;
        }

        public Vector3i West()
        {
            return this + WestBasis;
        }

        public Vector3i North()
        {
            return this + NorthBasis;
        }

        public Vector3i South()
        {
            return this + SouthBasis;
        }

        public Vector3i[] GetSurroundingPositions()
        {
            Vector3i[] surroundings = new Vector3i[6];
            surroundings[0] = North();
            surroundings[1] = South();
            surroundings[2] = East();
            surroundings[3] = West();
            surroundings[4] = Up();
            surroundings[5] = Down();
            return surroundings;
        } 
    }

    static class Vector3Extensions
    {
        public static Vector3 Plus(this Vector3 vector, Vector3i vectorI)
        {
            return new Vector3(vector.X + vectorI.X, vector.Y + vectorI.Y, vector.Z + vectorI.Z);
        }

        public static Vector3 Plus(this Vector3 vector, float value)
        {
            return new Vector3(vector.X + value, vector.Y + value, vector.Z + value);
        }
    }
}
