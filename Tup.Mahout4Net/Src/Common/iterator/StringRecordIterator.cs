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
using System.Text.RegularExpressions;

using Tup.Mahout4Net.Utils;

namespace Tup.Mahout4Net.Common.Iterator
{
    /// <summary>
    /// 
    /// </summary>
    public class StringRecordIterator
        : IEnumerable<KeyValuePair<List<string>, long>>
    {
        private static readonly long ONE = 1L;
        private Regex m_splitter;
        private IEnumerable<KeyValuePair<List<string>, long>> m_delegate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringIterator"></param>
        /// <param name="pattern"></param>
        public StringRecordIterator(IEnumerable<string> stringIterator, string pattern)
        {
            this.m_splitter = new Regex(pattern, RegexOptions.IgnoreCase);
            m_delegate = Iterators.Transform(
                stringIterator,
                from =>
                {
                    var items = m_splitter.Split(from.Trim());
                    return new KeyValuePair<List<string>, long>(Arrays.asList(items), ONE);
                });
        }

        #region IEnumerable<KeyValuePair<int[],long>> 成员
        public IEnumerator<KeyValuePair<List<string>, long>> GetEnumerator()
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