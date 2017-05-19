﻿using System;

namespace BingoUtils.Helpers
{
    public static class RandomExtensions
    {
        // by: Matt Howells / StackOverflow Community
        // http://stackoverflow.com/a/110570/5686352
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
