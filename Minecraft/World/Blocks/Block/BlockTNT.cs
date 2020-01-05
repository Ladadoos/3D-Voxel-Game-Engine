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

        public override void OnInteract(BlockState blockstate, World world)
        {
            ((BlockStateTNT)blockstate).triggered = true;
        }

        private void Explode(BlockState blockstate, World world)
        {
            List<BlockState> explosives = new List<BlockState>();

            /*for (int x = -4; x <= 4; x++)
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
                            world.QueueToRemoveBlockAt(target);
                        }
                    }
                }
            }*/

            foreach (BlockState state in explosives)
            {
                ((BlockStateTNT)state).triggeredByTnt = true;
                state.GetBlock().OnInteract(state, world);
            }
        }
    }
}
