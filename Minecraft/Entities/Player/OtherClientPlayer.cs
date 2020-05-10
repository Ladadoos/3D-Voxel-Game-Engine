using OpenTK;

namespace Minecraft
{
    class OtherClientPlayer : Player
    {
        public Vector3 ServerPosition { get; set; }
        private const float positionLerpSmoothFactor = 20;

        public OtherClientPlayer(int id, string playerName) : base(id, playerName, new Vector3(10, 100, 10))
        {
        }

        public override void Update(float deltaTime, World world)
        {
            Position = Maths.Lerp(Position, ServerPosition, deltaTime * positionLerpSmoothFactor);
            base.Update(deltaTime, world);
        }
    }
}
