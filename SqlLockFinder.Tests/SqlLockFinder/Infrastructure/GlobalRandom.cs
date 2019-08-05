using System;

namespace SqlLockFinder.Infrastructure
{
    public static class GlobalRandom
    {
        public static Random Instance { get; }

        static GlobalRandom()
        {
            Instance = new Random((int) DateTime.Now.Ticks);
        }
    }
}