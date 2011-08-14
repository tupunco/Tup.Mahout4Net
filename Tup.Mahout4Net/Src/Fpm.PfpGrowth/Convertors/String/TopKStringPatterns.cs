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
using System.Text;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors.String
{

    /**
     * A class which collects Top K string patterns
     *
     */
    public sealed class TopKStringPatterns
        : IEnumerable<KeyValuePair<List<string>, long>>
    {
        private List<KeyValuePair<List<string>, long>> frequentPatterns;
        /// <summary>
        /// 
        /// </summary>
        public TopKStringPatterns()
        {
            frequentPatterns = new List<KeyValuePair<List<string>, long>>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="patterns"></param>
        public TopKStringPatterns(ICollection<KeyValuePair<List<string>, long>> patterns)
        {
            frequentPatterns = new List<KeyValuePair<List<string>, long>>();
            if (patterns != null)
                frequentPatterns.AddRange(patterns);
        }

        #region IEnumerable<KeyValuePair<List<string>,long>> 成员
        public IEnumerator<KeyValuePair<List<string>, long>> GetEnumerator()
        {
            return frequentPatterns.GetEnumerator();
        }
        #endregion

        #region IEnumerable 成员
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return frequentPatterns.GetEnumerator();
        }
        #endregion

        public List<KeyValuePair<List<string>, long>> Patterns
        {
            get
            {
                return frequentPatterns;
            }
        }

        //public TopKStringPatterns merge(TopKStringPatterns pattern, int heapSize)
        //{
        //    List<KeyValuePair<List<String>, long>> patterns = new ArrayList<KeyValuePair<List<String>, long>>();
        //    Iterator<KeyValuePair<List<String>, long>> myIterator = frequentPatterns.iterator();
        //    Iterator<KeyValuePair<List<String>, long>> otherIterator = pattern.iterator();
        //    KeyValuePair<List<String>, long> myItem = null;
        //    KeyValuePair<List<String>, long> otherItem = null;
        //    for (int i = 0; i < heapSize; i++)
        //    {
        //        if (myItem == null && myIterator.hasNext())
        //        {
        //            myItem = myIterator.next();
        //        }
        //        if (otherItem == null && otherIterator.hasNext())
        //        {
        //            otherItem = otherIterator.next();
        //        }
        //        if (myItem != null && otherItem != null)
        //        {
        //            int cmp = myItem.getSecond().compareTo(otherItem.getSecond());
        //            if (cmp == 0)
        //            {
        //                cmp = myItem.getFirst().size() - otherItem.getFirst().size();
        //                if (cmp == 0)
        //                {
        //                    for (int j = 0; j < myItem.getFirst().size(); j++)
        //                    {
        //                        cmp = myItem.getFirst().get(j).compareTo(
        //                          otherItem.getFirst().get(j));
        //                        if (cmp != 0)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            if (cmp <= 0)
        //            {
        //                patterns.add(otherItem);
        //                if (cmp == 0)
        //                {
        //                    myItem = null;
        //                }
        //                otherItem = null;
        //            }
        //            else if (cmp > 0)
        //            {
        //                patterns.add(myItem);
        //                myItem = null;
        //            }
        //        }
        //        else if (myItem != null)
        //        {
        //            patterns.add(myItem);
        //            myItem = null;
        //        }
        //        else if (otherItem != null)
        //        {
        //            patterns.add(otherItem);
        //            otherItem = null;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return new TopKStringPatterns(patterns);
        //}

        //@Override
        //public void readFields(DataInput in) throws IOException {
        //  frequentPatterns.clear();
        //  int length = in.readInt();
        //  for (int i = 0; i < length; i++) {
        //    List<String> items = new ArrayList<String>();
        //    int itemsetLength = in.readInt();
        //    long support = in.readlong();
        //    for (int j = 0; j < itemsetLength; j++) {
        //      items.add(in.readUTF());
        //    }
        //    frequentPatterns.add(new KeyValuePair<List<String>,long>(items, support));
        //  }
        //}

        //@Override
        //public void write(DataOutput out) throws IOException {
        //  out.writeInt(frequentPatterns.size());
        //  for (KeyValuePair<List<String>,long> pattern : frequentPatterns) {
        //    out.writeInt(pattern.getFirst().size());
        //    out.writelong(pattern.getSecond());
        //    for (String item : pattern.getFirst()) {
        //      out.writeUTF(item);
        //    }
        //  }
        //}
        public override string ToString()
        {
            var sb = new StringBuilder();
            var sep = ", ";
            foreach (var pattern in frequentPatterns)
            {
                if (sb.Length > 0)
                    sb.Append(sep);
                sb.Append(PairToString(pattern));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string PairToString(KeyValuePair<List<string>, long> item)
        {
            return string.Format("([{0}],{1})", string.Join(",", item.Key), item.Value);
        }
    }
}