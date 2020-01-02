using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    class WorldChunkStorage
    {
        public Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();
    }
}
