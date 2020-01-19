using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateAir : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Air;
        }
    }
}
