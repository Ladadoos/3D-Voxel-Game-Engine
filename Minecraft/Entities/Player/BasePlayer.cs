using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    class BasePlayer
    {
        protected Vector3 position; //Bottom-left of player AABB
        protected Vector3 velocity;
        protected AABB hitbox;
    }
}
