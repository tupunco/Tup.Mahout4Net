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

using Tup.Hadoop.Mapred;
using System.Collections.Generic;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors.String
{
    /**
     * Collects a string pattern in a MaxHeap and outputs the top K patterns
     * 
     */
    public sealed class StringOutputConverter
        : IOutputCollector<string, List<KeyValuePair<List<string>, long>>>
    {

        private IOutputCollector<string,TopKStringPatterns> m_collector;

        public StringOutputConverter(IOutputCollector<string, TopKStringPatterns> collector)
        {
            if (collector == null)
                throw new System.ArgumentNullException("collector");

            this.m_collector = collector;
        }

        #region IOutputCollector<string,List<KeyValuePair<List<string>,long>>> ≥…‘±
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramK"></param>
        /// <param name="paramV"></param>
        public void Collect(string paramK, List<KeyValuePair<List<string>, long>> paramV)
        {
            m_collector.Collect(paramK, new TopKStringPatterns(paramV));
        }
        #endregion
    }
}