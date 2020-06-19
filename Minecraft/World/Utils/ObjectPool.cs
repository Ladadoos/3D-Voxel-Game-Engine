using System;
using System.Collections.Generic;

namespace Minecraft
{
    class ObjectPool<T> where T : class, new()
    {
        private Stack<T> pool;
        private HashSet<T> lended;
        private object lck = new object();

        public ObjectPool(int numOfObjects)
        {
            pool = new Stack<T>(numOfObjects);
            lended = new HashSet<T>(numOfObjects);

            for(int i = 0; i < numOfObjects; i++)
                pool.Push(new T());
        }

        public T GetObject()
        {
            lock(lck)
            {
                if(pool.Count <= 0)
                    pool.Push(new T());
                T item = pool.Pop();
                lended.Add(item);
                return item;
            }
        }

        public void ReturnObject(T item)
        {
            lock(lck)
            {
                if(!lended.Remove(item))
                    throw new Exception("Trying to return item that was not lended!");
                pool.Push(item);
            }
        }
    }
}