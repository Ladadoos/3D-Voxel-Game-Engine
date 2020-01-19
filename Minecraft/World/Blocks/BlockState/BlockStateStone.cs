using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateStone : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Stone;
        }
    }
}
