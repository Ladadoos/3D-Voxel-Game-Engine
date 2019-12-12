using OpenTK;
using System;

namespace Minecraft
{
    abstract class BlockState
    {
        public Block block { get; protected set; }

        //Position is automatically set by the world once the block is placed.
        public Vector3 position;

        public BlockState()
        {

        }

        public BlockState Clone()
        {
            return (BlockState)MemberwiseClone();
        }
    }

    class BlockStateDirt : BlockState
    {
        public BlockStateDirt() : base()
        {
            block = Block.Dirt;
        }
    }

    class BlockStateAir : BlockState
    {
        public BlockStateAir() : base()
        {
            block = Block.Air;
        }
    }
}
