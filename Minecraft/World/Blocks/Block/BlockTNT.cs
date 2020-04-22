using System.Collections.Generic;

namespace Minecraft
{
    class BlockTNT : Block
    {
        public BlockTNT(int id) : base(id)
        {
            isTickable = true;
            isInteractable = true;
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

        public override void OnInteract(BlockState blockstate, Vector3i blockPos, World world)
        {
            BlockStateTNT blockTnt = (BlockStateTNT)blockstate;
            blockTnt.triggered = true;
            blockTnt.blockPos = blockPos;
        }

        private void Explode(BlockStateTNT blockstate, World world)
        {
            List<BlockStateTNT> explosives = new List<BlockStateTNT>();

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

                        Vector3i target = blockstate.blockPos + new Vector3i(x, y, z);
                        BlockState state = world.GetBlockAt(target);
                        if(state.GetBlock() == Blocks.Air)
                        {
                            continue;
                        }

                        if (state is BlockStateTNT && blockstate.blockPos != target)
                        {
                            BlockStateTNT tntBlock = (BlockStateTNT)state;
                            tntBlock.blockPos = target;
                            explosives.Add(tntBlock);
                        } else
                        {
                            world.QueueToRemoveBlockAt(target);
                        }
                    }
                }
            }

            foreach (BlockStateTNT state in explosives)
            {
                state.triggeredByTnt = true;
                state.GetBlock().OnInteract(state, state.blockPos, world);
            }
        }
    }
}
