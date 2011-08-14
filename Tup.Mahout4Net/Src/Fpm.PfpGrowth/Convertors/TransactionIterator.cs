using System;
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
using System.Collections.Generic;

using Tup.Mahout4Net.Utils;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors
{
    /**
     * Iterates over a Transaction and outputs the transaction integer id mapping and the support of the
     * transaction
     */
    public class TransactionIterator<T> : IEnumerable<KeyValuePair<int[], long>>
    {
        private int[] m_transactionBuffer;
        private IEnumerable<KeyValuePair<int[], long>> m_delegate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="attributeIdMapping"></param>
        public TransactionIterator(IEnumerable<KeyValuePair<List<T>, long>> transactions, 
                                    Dictionary<T, int> attributeIdMapping)
        {
            if (transactions == null)
                throw new System.ArgumentNullException("transactions");
            if (attributeIdMapping == null)
                throw new System.ArgumentNullException("attributeIdMapping");

            m_transactionBuffer = new int[attributeIdMapping.Count];
            m_delegate = Iterators.Transform(transactions,
                from =>
                {
                    int index = 0;
                    foreach (T attribute in from.Key)
                    {
                        if (attributeIdMapping.ContainsKey(attribute))
                        {
                            m_transactionBuffer[index++] = attributeIdMapping[attribute];
                        }
                    }
                    int[] transactionList = new int[index];
                    Array.Copy(m_transactionBuffer, 0, transactionList, 0, index);
                    return new KeyValuePair<int[], long>(transactionList, from.Value);
                });
        }

        #region IEnumerable<KeyValuePair<int[],long>> 成员
        public IEnumerator<KeyValuePair<int[], long>> GetEnumerator()
        {
            return m_delegate.GetEnumerator();
        }
        #endregion

        #region IEnumerable 成员
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_delegate.GetEnumerator();
        }
        #endregion
    }
}
