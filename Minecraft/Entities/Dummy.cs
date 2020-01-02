using OpenTK;

namespace Minecraft
{
    class Dummy : Entity
    {
        public Dummy(int id) : base(id, new Vector3(15, 105, 15), EntityType.Dummy)
        {
            
        }
    }
}
