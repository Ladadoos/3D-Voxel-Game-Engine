using OpenTK;

namespace Minecraft
{
    class LightDebugRenderer
    {
        private readonly Game game;
        private readonly WireframeRenderer wireframeRenderer;
        public int DesiredLightLevel { get; private set; } = 1;

        public LightDebugRenderer(Game game, WireframeRenderer wireframeRenderer)
        {
            this.game = game;
            this.wireframeRenderer = wireframeRenderer;
        }

        public void RenderLightArea()
        {
            if(Game.Input.OnKeyPress(OpenTK.Input.Key.Down))
            {
                if(DesiredLightLevel > 1)
                {
                    DesiredLightLevel--;
                }
            }
            if(Game.Input.OnKeyPress(OpenTK.Input.Key.Up))
            {
                if(DesiredLightLevel < 15)
                {
                    DesiredLightLevel++;
                }
            }

            Vector2 chunkPos = World.GetChunkPosition(game.ClientPlayer.Position.X, game.ClientPlayer.Position.Z);

            if(game.World.loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                for(uint x = 0; x < 16; x++)
                {
                    for(uint z = 0; z < 16; z++)
                    {
                        for(uint y = 0; y < 256; y++)
                        {
                            uint lightValue = chunk.LightMap.GetBlockLightAt(x, y, z);
                            if(lightValue == DesiredLightLevel)
                            {
                                Vector3 scaleVector = new Vector3(1, 1, 1);
                                Vector3 translation = new Vector3(x + chunk.GridX * 16, y, z + chunk.GridZ * 16);

                                float green = Maths.ConvertRange(0, 15, 0, 0.85F, lightValue) + 0.15F;
                                wireframeRenderer.RenderWireframeAt(3, translation, scaleVector, new Vector3(0, green, 0));
                            }
                        }
                    }
                }
            }
        }
    }
}
