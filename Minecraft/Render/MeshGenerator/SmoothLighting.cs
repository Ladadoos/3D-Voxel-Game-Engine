using System;

namespace Minecraft
{
    class SmoothLighting
    {
        private static Corner[] cornerTop = new Corner[] { Corner.BottomLeft, Corner.BottomRight, Corner.TopRight, Corner.TopLeft };
        private static Corner[] cornerBottom = new Corner[] { Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft };
        private static Corner[] cornerRight = new Corner[] { Corner.BottomRight, Corner.BottomLeft, Corner.TopLeft, Corner.TopRight };

        private Corner[] GetCornerOrder(Direction dir)
        {
            switch(dir)
            {
                case Direction.Top: return cornerTop;
                case Direction.Bottom: return cornerBottom;
                case Direction.Left: return cornerTop;
                case Direction.Right: return cornerRight;
                case Direction.Front: return cornerTop;
                case Direction.Back: return cornerRight;
                default: throw new NotImplementedException();
            }
        }

        private (Chunk, Vector3i)[] blockBuffer = new (Chunk, Vector3i)[4];
        private Light[] lighBuffer = new Light[4];
        public Light[] GetLightsAt(World world, Chunk chunk, int localX, int worldY, int localZ, Direction dir)
        {
            Vector3i anchor = new Vector3i(localX, worldY, localZ) + DirectionUtil.ToUnit(dir);

            Block blockSource = null;
            bool sideSource = true;
            var (pPos, pChunk) = BlockPropagation.FixReference(world, anchor, chunk, out bool pWasFixed);
            if(!pWasFixed)
            {
                blockBuffer[0] = (null, pPos);
                sideSource = false;
            } else
            {
                blockBuffer[0] = (pChunk, pPos);
                blockSource = pChunk.GetBlockAt(pPos).GetBlock();
            }

            Light d = new Light(0, 0, 0, 0, 0);
            if(sideSource && blockSource.IsOpaque)
            {
                for(int i = 0; i < 4; i++)
                    lighBuffer[i] = d;
                return lighBuffer;
            }

            int j = 0;
            foreach(Corner corner in GetCornerOrder(dir))
            {
                int i = 1;
                foreach(Vector3i target in GetTargets(world, chunk, anchor, dir, corner))
                {
                    var (fPos, fChunk) = BlockPropagation.FixReference(world, target, chunk, out bool wasFixed);
                    if(!wasFixed)
                        blockBuffer[i] = (null, fPos);
                    else
                        blockBuffer[i] = (fChunk, fPos);
                    i++;
                }

                bool sideOne = blockBuffer[1].Item1 != null;
                bool sideTwo = blockBuffer[2].Item1 != null;
                bool sideCorner = blockBuffer[3].Item1 != null;

                Block blockOne = null;
                Block blockTwo = null;
                Block blockCorner = null;

                if(sideOne)
                    blockOne = blockBuffer[1].Item1.GetBlockAt(blockBuffer[1].Item2).GetBlock();
                if(sideTwo)
                    blockTwo = blockBuffer[2].Item1.GetBlockAt(blockBuffer[2].Item2).GetBlock();

                if(sideOne && sideTwo && blockOne.IsOpaque && blockTwo.IsOpaque)
                {
                    if(!sideSource)
                        lighBuffer[j] = new Light(0, 10, 0, 0, 1);
                    else
                    {
                        Chunk currentChunk = blockBuffer[0].Item1;
                        uint x = (uint)blockBuffer[0].Item2.X;
                        uint y = (uint)blockBuffer[0].Item2.Y;
                        uint z = (uint)blockBuffer[0].Item2.Z;
                        uint r = currentChunk.LightMap.GetRedBlockLightAt(x, y, z);
                        uint g = currentChunk.LightMap.GetGreenBlockLightAt(x, y, z);
                        uint b = currentChunk.LightMap.GetBlueBlockLightAt(x, y, z);
                        uint s = currentChunk.LightMap.GetSunLightIntensityAt(x, y, z);
                        lighBuffer[j] = new Light(r, g, b, s, 63);
                    }
                    j++;
                    continue;
                }

                if(sideCorner)
                    blockCorner = blockBuffer[3].Item1.GetBlockAt(blockBuffer[3].Item2).GetBlock();

                Light l1 = sideSource ? (blockSource.IsOpaque ? d : blockBuffer[0].Item1.LightMap.GetLightColorAt(blockBuffer[0].Item2)) : d;
                Light l2 = sideOne    ? (blockOne.IsOpaque    ? d : blockBuffer[1].Item1.LightMap.GetLightColorAt(blockBuffer[1].Item2)) : d;
                Light l3 = sideTwo    ? (blockTwo.IsOpaque    ? d : blockBuffer[2].Item1.LightMap.GetLightColorAt(blockBuffer[2].Item2)) : d;
                Light l4 = sideCorner ? (blockCorner.IsOpaque ? d : blockBuffer[3].Item1.LightMap.GetLightColorAt(blockBuffer[3].Item2)) : d;
                lighBuffer[j] = 
                    new Light(l1.GetRedChannel()   + l2.GetRedChannel()   + l3.GetRedChannel()   + l4.GetRedChannel(),
                              l1.GetGreenChannel() + l2.GetGreenChannel() + l3.GetGreenChannel() + l4.GetGreenChannel(),
                              l1.GetBlueChannel()  + l2.GetBlueChannel()  + l3.GetBlueChannel()  + l4.GetBlueChannel(),
                              l1.GetSunlight()     + l2.GetSunlight()     + l3.GetSunlight()     + l4.GetSunlight(),
                              63);
                j++;
            }

            return lighBuffer;
        }

        private static Vector3i[] targetBuffer = new Vector3i[3];
        private Vector3i[] GetTargets(World world, Chunk chunk, Vector3i anchor, Direction originalDir, Corner corner)
        {
            switch(originalDir)
            {
                case Direction.Bottom:
                case Direction.Top:
                    {
                        switch(corner)
                        {
                            case Corner.BottomLeft:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(0, 0, -1);
                                    targetBuffer[1] = anchor + new Vector3i(-1, 0, 0);
                                    targetBuffer[2] = anchor + new Vector3i(-1, 0, -1);
                                    break;
                                }
                            case Corner.BottomRight:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(-1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, 0, 1);
                                    targetBuffer[2] = anchor + new Vector3i(-1, 0, 1);
                                    break;
                                }
                            case Corner.TopRight:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, 0, 1);
                                    targetBuffer[2] = anchor + new Vector3i(1, 0, 1);
                                    break;
                                }
                            case Corner.TopLeft:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, 0, -1);
                                    targetBuffer[2] = anchor + new Vector3i(1, 0, -1);
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case Direction.Left:
                case Direction.Right:
                    {
                        switch(corner)
                        {
                            case Corner.BottomLeft:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(0, 0, -1);
                                    targetBuffer[1] = anchor + new Vector3i(0, -1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(0, -1, -1);
                                    break;
                                }
                            case Corner.BottomRight:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(0, 0, 1);
                                    targetBuffer[1] = anchor + new Vector3i(0, -1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(0, -1, 1);
                                    break;
                                }
                            case Corner.TopRight:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(0, 0, 1);
                                    targetBuffer[1] = anchor + new Vector3i(0, 1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(0, 1, 1);
                                    break;
                                }
                            case Corner.TopLeft:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(0, 0, -1);
                                    targetBuffer[1] = anchor + new Vector3i(0, 1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(0, 1, -1);
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }
                case Direction.Front:
                case Direction.Back:
                    {
                        switch(corner)
                        {
                            case Corner.BottomLeft:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(-1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, -1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(-1, -1, 0);
                                    break;
                                }
                            case Corner.BottomRight:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, -1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(1, -1, 0);
                                    break;
                                }
                            case Corner.TopRight:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, 1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(1, 1, 0);
                                    break;
                                }
                            case Corner.TopLeft:
                                {
                                    targetBuffer[0] = anchor + new Vector3i(-1, 0, 0);
                                    targetBuffer[1] = anchor + new Vector3i(0, 1, 0);
                                    targetBuffer[2] = anchor + new Vector3i(-1, 1, 0);
                                    break;
                                }
                            default: throw new NotImplementedException();
                        }
                        break;
                    }

                default: throw new NotImplementedException();
            }
            return targetBuffer;
        }

    }
}
