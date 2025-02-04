﻿using System.Collections.Generic;
using UnityEngine;

namespace HollowTwitch.Extensions
{
    public static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value) 
            => (key, value) = (self.Key, self.Value);
    }
}
