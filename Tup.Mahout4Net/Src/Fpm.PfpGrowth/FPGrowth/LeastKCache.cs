/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

using Tup.Mahout4Net.Utils;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LeastKCache<K, V>
        where K : IComparable<K>
    {
        private readonly int capacity;
        private readonly Dictionary<K, V> cache;
        private readonly PriorityQueue<K> queue;

        public LeastKCache(int capacity)
        {
            this.capacity = capacity;
            cache = new Dictionary<K, V>(capacity);
            queue = new PriorityQueue<K>(capacity + 1, new ReverseComparator<K>());
        }

        public V this[K key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }

        public V Get(K key)
        {
            V v = default(V);
            cache.TryGetValue(key, out v);
            return v;
        }

        public void Set(K key, V value)
        {
            if (!Contains(key))
            {
                queue.Add(key);
            }

            cache.Add(key, value);
            while (queue.Count > capacity)
            {
                K k = queue.Poll();
                cache.Remove(k);
            }
        }

        public int Count
        {
            get
            {
                return cache.Count;
            }
        }

        public bool Contains(K key)
        {
            return cache.ContainsKey(key);
        }

        class ReverseComparator<T>
            : IComparer<T> where T : IComparable<T>
        {
            #region IComparer<T> ≥…‘±
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
            #endregion
        }
    }
}