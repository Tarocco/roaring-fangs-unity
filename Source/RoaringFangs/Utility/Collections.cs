﻿/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;

namespace RoaringFangs.Utility
{
    public static class Collections
    {
        public struct EnumeratedInstance<T>
        {
            public long Index;
            public T Element;
        }

        public static IEnumerable<EnumeratedInstance<T>> Enumerate<T>(this IEnumerable<T> collection)
        {
            long index = 0;
            foreach (var item in collection)
            {
                yield return new EnumeratedInstance<T>
                {
                    Index = index,
                    Element = item
                };
                index++;
            }
        }

        public static long Count<T>(this IEnumerable<T> collection)
        {
            long counter = 0;
            IEnumerator<T> enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
                counter++;
            return counter;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            TValue value;
            if (self.TryGetValue(key, out value))
                return value;
            return default(TValue);
        }

        public static int AggregatedHashCode<T>(this ICollection<T> self)
        {
            var hashes = self
                .Where(s => s != null)
                .Select(s => s.GetHashCode());
            if (hashes.Any())
                return hashes.Aggregate((a, b) => a ^ b);
            return 0;
        }

        public static int AggregatedInstanceIDs<T>(this ICollection<T> self) where T : UnityEngine.Object
        {
            var ids = self
                .Where(s => s != null)
                .Select(s => s.GetInstanceID());
            if (ids.Any())
                return ids.Aggregate((a, b) => a ^ b);
            return 0;
        }
    }
}