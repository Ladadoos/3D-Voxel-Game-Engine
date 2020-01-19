using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateGrass : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Grass;
        }
    }
}
