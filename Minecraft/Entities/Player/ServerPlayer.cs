using OpenTK;

namespace Minecraft
{
    class ServerPlayer : Player
    {
        public ServerPlayer(int id, Vector3 position) : base(id, position)
        {
            
        }

        public override void Update(float deltaTime, World world)
        {
            CalculatePlayerAABB();
        }
    }
}
