using OpenTK;

namespace Minecraft
{
    class Dummy : Entity
    {
        public Dummy(int id) : base(id, EntityType.Dummy)
        {
            position = new Vector3(15, 105, 15);
        }
    }
}
