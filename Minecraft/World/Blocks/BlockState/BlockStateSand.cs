using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateSand : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Sand;
        }
    }
}
