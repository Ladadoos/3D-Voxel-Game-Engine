using OpenTK;

namespace Minecraft
{
    class ServerPlayer : Player
    {
        public ServerPlayer(int id, string playerName, World world, Vector3 position) 
            : base(id, playerName, world, position)
        {          
        }
    }
}
