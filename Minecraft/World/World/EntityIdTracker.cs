using System;
using System.Collections.Generic;

namespace Minecraft
{
    class EntityIdTracker
    {
        private HashSet<int> entityIdsTaken = new HashSet<int>();
        private Random random = new Random();

        public int GenerateId()
        {
            while (true)
            {
                int rand = random.Next(int.MaxValue);
                if (!entityIdsTaken.Contains(rand))
                {
                    entityIdsTaken.Add(rand);
                    return rand;
                }
            }
        }

        public void ReleaseId(int entityId)
        {
            entityIdsTaken.Remove(entityId);
        }
    }
}
