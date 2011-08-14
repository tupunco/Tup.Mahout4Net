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
using System.IO;

using Tup.Hadoop.Mapred;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors
{

    /**
     * Collects the {@link Writable} key and {@link Writable} value, and writes them into a {@link SequenceFile}
     * 
     * @param <K>
     * @param <V>
     */
    public class SequenceFileOutputCollector<K, V> :
        IOutputCollector<K, V>
    {
        private TextWriter m_writer;
        private string m_format = "{0}\t{1}\n";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="format"></param>
        public SequenceFileOutputCollector(TextWriter writer, string format)
        {
            this.m_writer = writer;

            if (!string.IsNullOrEmpty(format))
                this.m_format = format;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public SequenceFileOutputCollector(TextWriter writer)
            : this(writer, null)
        {
        }

        #region IOutputCollector<K,V> ≥…‘±
        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramK"></param>
        /// <param name="paramV"></param>
        public void Collect(K paramK, V paramV)
        {
            m_writer.Write(m_format, paramK, paramV);
        }
        #endregion
    }
}