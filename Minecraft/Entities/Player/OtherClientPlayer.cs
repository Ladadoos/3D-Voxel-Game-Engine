using OpenTK;

namespace Minecraft
{
    class OtherClientPlayer : Player
    {
        public Vector3 ServerPosition { get; set; }
        private const float positionLerpSmoothFactor = 20;

        public OtherClientPlayer(int id, string playerName, World world) : base(id, playerName, world, Vector3.Zero)
        {
        }

        public override void Update(float deltaTime, World world)
        {
            Position = Maths.Lerp(Position, ServerPosition, deltaTime * positionLerpSmoothFactor);
            base.Update(deltaTime, world);
        }
    }
}
