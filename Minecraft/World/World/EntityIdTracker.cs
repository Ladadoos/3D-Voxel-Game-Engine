using System;
using System.Collections.Generic;

namespace Minecraft
{
    class IdTracker
    {
        private readonly HashSet<int> idsTaken = new HashSet<int>();
        private readonly Random random = new Random();

        public int GenerateId()
        {
            while (true)
            {
                int rand = 1 + random.Next(int.MaxValue - 1);
                if (!idsTaken.Contains(rand))
                {
                    idsTaken.Add(rand);
                    return rand;
                }
            }
        }

        public void ReleaseId(int entityId)
        {
            idsTaken.Remove(entityId);
        }
    }
}
