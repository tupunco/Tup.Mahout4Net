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

    /**  keeps top K Attributes in a TreeSet */
    public class FrequentPatternMaxHeap
    {
        private int count;
        private Pattern least;
        private int maxSize;
        private bool subPatternCheck;
        private Dictionary<long, ISet<Pattern>> patternIndex;
        private PriorityQueue<Pattern> queue;

        public FrequentPatternMaxHeap(int numResults, bool subPatternCheck)
        {
            maxSize = numResults;
            queue = new PriorityQueue<Pattern>(maxSize);
            this.subPatternCheck = subPatternCheck;
            patternIndex = new Dictionary<long, ISet<Pattern>>();
            foreach (Pattern p in queue)
            {
                long index = p.Support;
                ISet<Pattern> patternList;
                if (!patternIndex.ContainsKey(index))
                {
                    patternList = new HashSet<Pattern>();
                    patternIndex.Add(index, patternList);
                }
                patternList = patternIndex[index];
                patternList.Add(p);
            }
        }

        public bool Addable(long support)
        {
            return count < maxSize || least.Support <= support;
        }

        public PriorityQueue<Pattern> GetHeap()
        {
            if (subPatternCheck)
            {
                PriorityQueue<Pattern> ret = new PriorityQueue<Pattern>(maxSize);
                foreach (Pattern p in queue)
                {
                    if (patternIndex[p.Support].Contains(p))
                    {
                        ret.Add(p);
                    }
                }
                return ret;
            }
            return queue;
        }

        public void AddAll(FrequentPatternMaxHeap patterns,
                           int attribute,
                           long attributeSupport)
        {
            foreach (Pattern pattern in patterns.GetHeap())
            {
                long support = Math.Min(attributeSupport, pattern.Support);
                if (this.Addable(support))
                {
                    pattern.Add(attribute, support);
                    this.Insert(pattern);
                }
            }
        }

        public void Insert(Pattern frequentPattern)
        {
            if (frequentPattern.Length == 0)
            {
                return;
            }

            if (count == maxSize)
            {
                if (frequentPattern.CompareTo(least) > 0 && AddPattern(frequentPattern))
                {
                    Pattern evictedItem = queue.Poll();
                    least = queue.Peek();
                    if (subPatternCheck)
                    {
                        patternIndex[evictedItem.Support].Remove(evictedItem);
                    }
                }
            }
            else
            {
                if (AddPattern(frequentPattern))
                {
                    count++;
                    if (least == null)
                    {
                        least = frequentPattern;
                    }
                    else
                    {
                        if (least.CompareTo(frequentPattern) < 0)
                        {
                            least = frequentPattern;
                        }
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public bool IsFull
        {
            get
            {
                return count == maxSize;
            }
        }

        public long LeastSupport
        {
            get
            {
                if (least == null)
                    return 0;

                return least.Support;
            }
        }

        private bool AddPattern(Pattern frequentPattern)
        {
            if (subPatternCheck)
            {
                long index = frequentPattern.Support;
                if (patternIndex.ContainsKey(index))
                {
                    ISet<Pattern> indexSet = patternIndex[index];
                    bool replace = false;
                    Pattern replacablePattern = null;
                    foreach (Pattern p in indexSet)
                    {
                        if (frequentPattern.IsSubPatternOf(p))
                        {
                            return false;
                        }
                        else if (p.IsSubPatternOf(frequentPattern))
                        {
                            replace = true;
                            replacablePattern = p;
                            break;
                        }
                    }
                    if (replace)
                    {
                        indexSet.Remove(replacablePattern);
                        if (!indexSet.Contains(frequentPattern))
                        {
                            queue.Add(frequentPattern);
                            indexSet.Add(frequentPattern);
                        }
                        return false;
                    }
                    queue.Add(frequentPattern);
                    indexSet.Add(frequentPattern);
                }
                else
                {
                    queue.Add(frequentPattern);
                    ISet<Pattern> patternList;
                    if (!patternIndex.ContainsKey(index))
                    {
                        patternList = new HashSet<Pattern>();
                        patternIndex.Add(index, patternList);
                    }
                    patternList = patternIndex[index];
                    patternList.Add(frequentPattern);
                }
            }
            else
            {
                queue.Add(frequentPattern);
            }
            return true;
        }
    }
}