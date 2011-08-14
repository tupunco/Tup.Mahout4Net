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
    /// Caches large FPTree {@link Object} for each level of the recursive
    /// {@link FPGrowth} algorithm to reduce allocation overhead.
    /// </summary>
    public class FPTreeDepthCache
    {
        private readonly LeastKCache<int, FPTree> firstLevelCache = new LeastKCache<int, FPTree>(5);
        private int hits;
        private int misses;
        private readonly List<FPTree> treeCache = new List<FPTree>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public FPTree GetFirstLevelTree(int attr)
        {
            FPTree tree = firstLevelCache.Get(attr);
            if (tree != null)
            {
                hits++;
                return tree;
            }
            else
            {
                misses++;
                FPTree conditionalTree = new FPTree();
                firstLevelCache.Set(attr, conditionalTree);
                return conditionalTree;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Hits
        {
            get
            {
                return hits;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Misses
        {
            get
            {
                return misses;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public FPTree GetTree(int level)
        {
            while (treeCache.Count < level + 1)
            {
                FPTree cTree = new FPTree();
                treeCache.Add(cTree);
            }
            FPTree conditionalTree = treeCache[level];
            conditionalTree.Clear();
            return conditionalTree;
        }
    }
}