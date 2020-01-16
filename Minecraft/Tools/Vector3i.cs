using OpenTK;

using ProtoBuf;

namespace Minecraft
{
    [ProtoContract]
    struct Vector3i
    {
        [ProtoMember(1)]
        public int X;
        [ProtoMember(2)]
        public int Y;
        [ProtoMember(3)]
        public int Z;

        public Vector3i(int X, int Y, int Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public Vector3i(Vector3 vector3f)
        {
            X = (int)vector3f.X;
            Y = (int)vector3f.Y;
            Z = (int)vector3f.Z;
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
    }
}
