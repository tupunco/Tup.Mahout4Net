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
    /**
     * A  in FPGrowth is a list of items (here int) and the
     * support(the number of times the pattern is seen in the dataset)
     * 
     */
    public class Pattern : IComparable<Pattern>
    {
        private static readonly int DEFAULT_INITIAL_SIZE = 2;
        private static readonly float GROWTH_RATE = 1.5f;
        private bool dirty = true;
        private int hashCode;
        private int length;
        private int[] pattern;
        private long support = long.MaxValue;
        private long[] supportValues;

        public Pattern()
            : this(DEFAULT_INITIAL_SIZE)
        {
        }

        private Pattern(int size)
        {
            if (size < DEFAULT_INITIAL_SIZE)
            {
                size = DEFAULT_INITIAL_SIZE;
            }
            this.pattern = new int[size];
            this.supportValues = new long[size];
            dirty = true;
        }

        public void Add(int id, long supportCount)
        {
            dirty = true;
            if (length >= pattern.Length)
            {
                Resize();
            }
            this.pattern[length] = id;
            this.supportValues[length++] = supportCount;
            this.support = supportCount > this.support ? this.support : supportCount;
        }

        public int[] GetPattern()
        {
            return this.pattern;
        }

        public object[] GetPatternWithSupport()
        {
            return new object[] { this.pattern, this.supportValues };
        }

        public bool IsSubPatternOf(Pattern frequentPattern)
        {
            int[] otherPattern = frequentPattern.GetPattern();
            int otherLength = frequentPattern.Length;
            if (this.Length > frequentPattern.Length)
            {
                return false;
            }
            int i = 0;
            int otherI = 0;
            while (i < length && otherI < otherLength)
            {
                if (otherPattern[otherI] == pattern[i])
                {
                    otherI++;
                    i++;
                }
                else if (otherPattern[otherI] < pattern[i])
                {
                    otherI++;
                }
                else
                {
                    return false;
                }
            }
            return otherI != otherLength || i == length;
        }

        public int Length
        {
            get
            {
                return this.length;
            }
        }

        public long Support
        {
            get
            {
                return this.support;
            }
        }

        private void Resize()
        {
            int size = (int)(GROWTH_RATE * length);
            if (size < DEFAULT_INITIAL_SIZE)
            {
                size = DEFAULT_INITIAL_SIZE;
            }
            int[] oldpattern = pattern;
            long[] oldSupport = supportValues;
            this.pattern = new int[size];
            this.supportValues = new long[size];
            Array.Copy(oldpattern, 0, this.pattern, 0, length);
            Array.Copy(oldSupport, 0, this.supportValues, 0, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (this.GetType() != obj.GetType())
            {
                return false;
            }
            Pattern other = (Pattern)obj;
            // expensive check done only if length and support matches    
            return length == other.length && support == other.support
                            && Arrays.equals(pattern, other.pattern);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (!dirty)
            {
                return hashCode;
            }
            int result = pattern.GetHashCode();
            result = 31 * result + support.GetHashCode();
            result = 31 * result + length;
            hashCode = result;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0}]-{1}", Arrays.Join(",", this.pattern, 0, length), support);
        }

        #region IComparable<Pattern> ≥…‘±
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cr2"></param>
        /// <returns></returns>
        public int CompareTo(Pattern cr2)
        {
            long support2 = cr2.Support;
            int length2 = cr2.Length;
            if (support == support2)
            {
                if (length < length2)
                    return -1;
                else if (length > length2)
                    return 1;
                else
                    return 0;
            }
            else
            {
                return support > support2 ? 1 : -1;
            }
        }
        #endregion
    }
}