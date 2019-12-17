using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    abstract class Block
    {
        protected readonly AABB[] emptyAABB = new AABB[0];
        public bool isTickable { get; protected set; }

        public abstract BlockState GetNewDefaultState();

        public virtual bool OnInteract(BlockState blockstate, World world)
        {
            return false;
        }

        public virtual bool CanAddBlockAt(World world, Vector3 intPosition)
        {
            return true;
        }

        public virtual void OnTick(BlockState blockState, World world, float deltaTime) { }

        public virtual void OnAdded(BlockState blockstate, World world) { }

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
        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDirt();
        }
    }

    class BlockStone : Block
    {
        public override BlockState GetNewDefaultState()
        {
            return new BlockStateStone();
        }

        public override bool OnInteract(BlockState blockstate, World world)
        {
            BlockStateStone state = (BlockStateStone)blockstate;
            System.Console.WriteLine("Interacted with stone at " + state.position);
            return true;
        }
    }

    class BlockFlower : Block
    {
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
        public BlockTNT()
        {
            isTickable = true;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateTNT();
        }

        public override void OnTick(BlockState blockstate, World world, float deltaTime)
        {
            BlockStateTNT blockTnt = (BlockStateTNT)blockstate;
            if (!blockTnt.triggered)
            {
                return;
            }

            blockTnt.elapsedSecondsSinceTrigger += deltaTime;
            if ((blockTnt.elapsedSecondsSinceTrigger > 2 && !blockTnt.triggeredByTnt)
                || (blockTnt.elapsedSecondsSinceTrigger > 0.2F && blockTnt.triggeredByTnt))
            {
                Explode(blockTnt, world);
            }
        }

        public override bool OnInteract(BlockState blockstate, World world)
        {
            ((BlockStateTNT)blockstate).triggered = true;
            return true;
        }

        private void Explode(BlockState blockstate, World world)
        {
            List<BlockState> explosives = new List<BlockState>();

            for (int x = -4; x <= 4; x++)
            {
                for (int y = -4; y <= 4; y++)
                {
                    for (int z = -4; z <= 4; z++)
                    {
                        if (x * x + y * y + z * z > 14)
                        {
                            continue;
                        }

                        Vector3 target = blockstate.position + new Vector3(x, y, z);
                        BlockState state = world.GetBlockAt(target);
                        if (state is BlockStateTNT && blockstate.position != target)
                        {
                            explosives.Add(state);
                        } else
                        {
                            world.DeleteBlockAt(target);
                        }
                    }
                }
            }

            foreach (BlockState state in explosives)
            {
                ((BlockStateTNT)state).triggeredByTnt = true;
                state.block.OnInteract(state, world);
            }
        }
    }
}
