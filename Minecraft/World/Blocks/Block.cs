using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    abstract class Block
    {
        public BlockMaterial material { get; protected set; }
        protected readonly AABB[] emptyAABB = new AABB[0];

        public Block(BlockMaterial material)
        {
            this.material = material;
        }

        public abstract BlockState GetNewDefaultState();

        public virtual bool OnInteract(BlockState blockstate, Game game)
        {
            return false;
        }

        public virtual bool CanAddBlockAt(World world, Vector3 intPosition)
        {
            return true;
        }

        public virtual void OnAdded(BlockState blockstate, Game game) { }

        public virtual AABB[] GetCollisionBox(BlockState state)
        {
            return new AABB[] { GetFullBlockCollision(state) };
        }

        public static AABB GetFullBlockCollision(BlockState state)
        {
            return new AABB(new Vector3(state.position.X, state.position.Y, state.position.Z),
                new Vector3(state.position.X + Constants.CUBE_SIZE, state.position.Y + Constants.CUBE_SIZE, state.position.Z + Constants.CUBE_SIZE));
        }
    }

    class BlockAir : Block
    {
        public BlockAir() : base(BlockMaterial.Air) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateAir();
        }

        public override AABB[] GetCollisionBox(BlockState state)
        {
            return emptyAABB;
        }
    }

    class BlockDirt : Block
    {
        public BlockDirt() : base(BlockMaterial.Opaque) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDirt();
        }
    }

    class BlockStone : Block
    {
        public BlockStone() : base(BlockMaterial.Opaque) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateStone();
        }

        public override bool OnInteract(BlockState blockstate, Game game)
        {
            BlockStateStone state = (BlockStateStone)blockstate;
            System.Console.WriteLine("Interacted with stone at " + state.position);
            return true;
        }
    }

    class BlockFlower : Block
    {
        public BlockFlower() : base(BlockMaterial.Fauna) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateFlower();
        }

        public override AABB[] GetCollisionBox(BlockState state)
        {
            return emptyAABB;
        }

        public override bool CanAddBlockAt(World world, Vector3 intPosition)
        {
            return world.GetBlockAt(intPosition + new Vector3(0, -1, 0)).block == Blocks.Dirt;
        }
    }

    class BlockTNT : Block
    {
        public BlockTNT() : base(BlockMaterial.Opaque) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateTNT();
        }

        public override bool OnInteract(BlockState blockstate, Game game)
        {
            List<BlockState> explosives = new List<BlockState>();

            for(int x = -4; x <= 4; x++)
            {
                for (int y = -4; y <= 4; y++)
                {
                    for (int z = -4; z <= 4; z++)
                    {
                        if(x * x + y * y + z * z > 14)
                        {
                            continue;
                        }

                        Vector3 target = blockstate.position + new Vector3(x, y, z);
                        BlockState state = game.world.GetBlockAt(target);
                        if(state is BlockStateTNT && blockstate.position != target)
                        {
                            explosives.Add(state);
                        } else
                        {
                            game.world.AddBlockToWorld(blockstate.position + new Vector3(x, y, z), Blocks.Air.GetNewDefaultState());
                        }
                    }
                }
            }

            foreach(BlockState state in explosives)
            {
                state.block.OnInteract(state, game);
            }

            return true;
        }
    }
}
