using System;
using System.Collections.Generic;

namespace Minecraft
{
    class Section
    {  
        public byte Height { get; private set; }
        public bool IsFullTransparent { get; private set; }
        public bool IsEmpty { get; private set; }
        public int GridX { get; private set; }
        public int GridZ { get; private set; }
        private int numberOfOpaqueBlocks;
        private int numberOfBlocks;

        private ushort[] blocks = new ushort[16 * 16 * 16];
        private Dictionary<int, BlockState> customStates = new Dictionary<int, BlockState>();

        public Section(int gridX, int gridZ, byte height)
        {
            GridX = gridX;
            GridZ = gridZ;
            Height = height;
            IsFullTransparent = true;
            IsEmpty = false;
        }

        public override string ToString()
        {
            return "Section[Height=" + Height + " FullTransparent=" + IsFullTransparent + " IsEmpty=" + IsEmpty +
                " NumOpaqueBlocks=" + numberOfOpaqueBlocks + " NumBlocks=" + numberOfBlocks + "]";
        }

        public void ResetAndAssign(int gridX, int gridZ)
        {
            GridX = gridX;
            GridZ = gridZ;

            if(customStates.Count != 0)
                throw new Exception();
        }

        public void AddBlockAt(int localX, int localY, int localZ, BlockState blockstate)
        {
            if(localX < 0 || localX > 15 || localY < 0 || localY > 15 || localZ < 0 || localZ > 15)
                throw new ArgumentOutOfRangeException(localX + "," + localY + "," + localZ);

            Block block = blockstate.GetBlock();
            int index = (localX << 8) + (localY << 4) + localZ;
            if(blocks[index] != 0)
                RemoveBlockAt(localX, localY, localZ);

            blocks[index] = block.ID;

            if(block.HasCustomState)
                customStates.Add(index, blockstate);

            if(block.IsOpaque)
                numberOfOpaqueBlocks++;
            IsFullTransparent = numberOfOpaqueBlocks == 0;

            numberOfBlocks++;
            IsEmpty = numberOfBlocks == 0;
        }

        public void RemoveBlockAt(int localX, int localY, int localZ)
        {
            if(localX < 0 || localX > 15 || localY < 0 || localY > 15 || localZ < 0 || localZ > 15)
                throw new ArgumentOutOfRangeException(localX + "," + localY + "," + localZ);

            int index = (localX << 8) + (localY << 4) + localZ;
            if(blocks[index] == 0)
                return;

            Block block = Blocks.GetBlockFromIdentifier(blocks[index]);
            if(block.HasCustomState)
                if(!customStates.Remove(index))
                    throw new Exception("Removing custom state block that was not in storage.");
            blocks[index] = 0;

            if(block.IsOpaque)
                numberOfOpaqueBlocks--;
            IsFullTransparent = numberOfOpaqueBlocks == 0;

            numberOfBlocks--;
            IsEmpty = numberOfBlocks == 0;
        }

        public BlockState GetBlockAt(int localX, int localY, int localZ)
        {
            int index = (localX << 8) + (localY << 4) + localZ;
            if(blocks[index] == 0)
                return null;

            Block block = Blocks.GetBlockFromIdentifier(blocks[index]);
            if(!block.HasCustomState)
                return Blocks.GetState(block);

            if(!customStates.TryGetValue(index, out BlockState state))
                throw new Exception("Custom state block was not in storage");

            return state;
        }
    }
}
