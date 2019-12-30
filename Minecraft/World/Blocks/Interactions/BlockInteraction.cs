using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    abstract class BlockInteraction
    {
        private InteractionType type;

        public BlockInteraction(InteractionType type)
        {
            this.type = type;
        }

        public abstract void Interact(BlockState state);
    }

    class EmptyInteraction : BlockInteraction
    {
        public EmptyInteraction() : base (InteractionType.None) { }

        public override void Interact(BlockState state) { }
    }

    /*class TntInteraction : BlockInteraction
    {
        public TntInteraction() : base(InteractionType.Tnt) { }

        public override void 
    }*/

    enum InteractionType : byte
    {
        None,
        Tnt
    }
}
