using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateFlower : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Flower;
        }
    }
}
