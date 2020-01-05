using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class WorldChunkStorage
    {
        public Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();
    }
}
