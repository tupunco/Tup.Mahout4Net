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

using Tup.Hadoop.Mapred;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors
{
    /**
     * An output converter which converts the output patterns and collects them in a
     * {@link FrequentPatternMaxHeap}
     * 
     * @param <A>
     */
    public class TopKPatternsOutputConverter<A>
        : IOutputCollector<int, FrequentPatternMaxHeap>
         where A : IComparable<A>
    {
        private IOutputCollector<A, List<KeyValuePair<List<A>, long>>> collector;
        private IDictionary<int, A> reverseMapping;

        public TopKPatternsOutputConverter(IOutputCollector<A, List<KeyValuePair<List<A>, long>>> collector,
                                           IDictionary<int, A> reverseMapping)
        {
            this.collector = collector;
            this.reverseMapping = reverseMapping;
        }

        public void Collect(int key, FrequentPatternMaxHeap value)
        {
            var perAttributePatterns = new List<KeyValuePair<List<A>, long>>();
            var t = value.GetHeap();
            while (!t.IsEmpty)
            {
                var itemSet = t.Poll();
                var frequentPattern = new List<A>();
                for (int j = 0; j < itemSet.Length; j++)
                {
                    frequentPattern.Add(reverseMapping[itemSet.GetPattern()[j]]);
                }
                frequentPattern.Sort();
                //Collections.sort(frequentPattern);

                var returnItemSet = new KeyValuePair<List<A>, long>(frequentPattern, itemSet.Support);
                perAttributePatterns.Add(returnItemSet);
            }
            perAttributePatterns.Reverse();
            //Collections.reverse(perAttributePatterns);

            collector.Collect(reverseMapping[key], perAttributePatterns);
        }
    }
}