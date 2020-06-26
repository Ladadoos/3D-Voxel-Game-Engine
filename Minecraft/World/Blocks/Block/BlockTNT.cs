using System;
using System.Collections.Generic;

namespace Minecraft
{
    class BlockTNT : Block
    {
        public BlockTNT(ushort id) : base(id)
        {
            IsTickable = true;
            IsInteractable = true;
            HasCustomState = true;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateTNT();
        }

        public override void OnTick(BlockState blockstate, World world, Vector3i blockPos, float deltaTime)
        {
            if(!(world is WorldServer))
                return;

            BlockStateTNT blockTnt = (BlockStateTNT)blockstate;
            if (blockTnt.Trigger == ExplosionTrigger.None)
                return;

            blockTnt.ElapsedSecondsSinceTrigger += deltaTime;
            if ((blockTnt.ElapsedSecondsSinceTrigger > 2 && blockTnt.Trigger == ExplosionTrigger.PlayerInteraction)
                || (blockTnt.ElapsedSecondsSinceTrigger > 0.2F && blockTnt.Trigger == ExplosionTrigger.Explosive))
            {
                Explode(blockTnt, world);
            }
        }

        public override void OnInteract(BlockState blockstate, Vector3i blockPos, World world)
        {
            BlockStateTNT blockTnt = (BlockStateTNT)blockstate;
            blockTnt.Trigger = ExplosionTrigger.PlayerInteraction;
            blockTnt.BlockPosition = blockPos;
        }

        private void Explode(BlockStateTNT blockstate, World world)
        {
            List<BlockStateTNT> explosives = new List<BlockStateTNT>();
            List<Vector3i> targets = new List<Vector3i>();

            const int explosionSize = 10;
            for (int x = -explosionSize; x <= explosionSize; x++)
            {
                for (int y = -explosionSize; y <= explosionSize; y++)
                {
                    for (int z = -explosionSize; z <= explosionSize; z++)
                    {
                        if (x * x + y * y + z * z > explosionSize * explosionSize)
                        {
                            continue;
                        }

                        Vector3i target = blockstate.BlockPosition + new Vector3i(x, y, z);
                        BlockState state = world.GetBlockAt(target);
                        if(state.GetBlock() == Blocks.Air)
                        {
                            continue;
                        }

                        if (state is BlockStateTNT && blockstate.BlockPosition != target)
                        {
                            BlockStateTNT tntBlock = (BlockStateTNT)state;
                            tntBlock.BlockPosition = target;
                            explosives.Add(tntBlock);
                        } else
                        {
                            targets.Add(target);
                        }
                    }
                }
            }

            world.QueueToRemoveBlocksAt(targets);

            foreach (BlockStateTNT state in explosives)
            {
                state.GetBlock().OnInteract(state, state.BlockPosition, world);
                state.Trigger = ExplosionTrigger.Explosive;
            }
        }
    }

    enum ExplosionTrigger : byte
    {
        None,
        PlayerInteraction,
        Explosive
    }
}
