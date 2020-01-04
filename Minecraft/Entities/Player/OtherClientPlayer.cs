using OpenTK;

namespace Minecraft
{
    class OtherClientPlayer : Player
    {
        public Vector3 serverPosition;
        private float positionLerpSmoothFactor = 20;

        public OtherClientPlayer(int id) : base(id, new Vector3(10, 100, 10))
        {

        }

        public override void Update(float deltaTime, World world)
        {
            position = Maths.Lerp(position, serverPosition, deltaTime * positionLerpSmoothFactor);
            CalculatePlayerAABB();
        }
    }
}
