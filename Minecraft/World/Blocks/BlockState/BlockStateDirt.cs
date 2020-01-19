using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateDirt : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Dirt;
        }
    }
}
