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
    /// The Frequent Pattern Tree datastructure used for mining patterns using
    /// {@link FPGrowth} algorithm
    /// </summary>
    public class FPTree
    {
        public static readonly int ROOTNODEID = 0;
        private static readonly int DEFAULT_CHILDREN_INITIAL_SIZE = 2;
        private static readonly int DEFAULT_HEADER_TABLE_INITIAL_SIZE = 4;
        private static readonly int DEFAULT_INITIAL_SIZE = 8;
        private static readonly float GROWTH_RATE = 1.5f;
        private static readonly int HEADERTABLEBLOCKSIZE = 2;
        private static readonly int HT_LAST = 1;
        private static readonly int HT_NEXT = 0;

        private int[] attribute;
        private int[] childCount;
        private int[] conditional;
        private long[] headerTableAttributeCount;
        private int[] headerTableAttributes;
        private int headerTableCount;
        private int[] headerTableLookup;
        private int[][] headerTableProperties;
        private int[] next;
        private int[][] nodeChildren;
        private long[] nodeCount;
        private int nodes;
        private int[] parent;
        private bool singlePath;
        private ICollection<int> sortedSet = new SortedSet<int>();

        public FPTree()
            : this(DEFAULT_INITIAL_SIZE)
        {
        }

        public FPTree(int size)
        {
            if (size < DEFAULT_INITIAL_SIZE)
            {
                size = DEFAULT_INITIAL_SIZE;
            }

            parent = new int[size];
            next = new int[size];
            childCount = new int[size];
            attribute = new int[size];
            nodeCount = new long[size];

            nodeChildren = new int[size][];
            conditional = new int[size];

            headerTableAttributes = new int[DEFAULT_HEADER_TABLE_INITIAL_SIZE];
            headerTableAttributeCount = new long[DEFAULT_HEADER_TABLE_INITIAL_SIZE];
            headerTableLookup = new int[DEFAULT_HEADER_TABLE_INITIAL_SIZE];
            Arrays.fill(headerTableLookup, -1);
            headerTableProperties = new int[DEFAULT_HEADER_TABLE_INITIAL_SIZE][];

            singlePath = true;
            CreateRootNode();
        }

        public void AddChild(int parentNodeId, int childnodeId)
        {
            int length = childCount[parentNodeId];
            if (length >= nodeChildren[parentNodeId].Length)
            {
                ResizeChildren(parentNodeId);
            }
            nodeChildren[parentNodeId][length++] = childnodeId;
            childCount[parentNodeId] = length;

            if (length > 1 && singlePath)
            {
                singlePath = false;
            }
        }

        public bool AddCount(int nodeId, long count)
        {
            if (nodeId < nodes)
            {
                this.nodeCount[nodeId] += count;
                return true;
            }
            return false;
        }

        public void AddHeaderCount(int attributeValue, long count)
        {
            int index = GetHeaderIndex(attributeValue);
            headerTableAttributeCount[index] += count;
        }

        public void AddHeaderNext(int attributeValue, int nodeId)
        {
            int index = GetHeaderIndex(attributeValue);
            if (headerTableProperties[index][HT_NEXT] == -1)
            {
                headerTableProperties[index][HT_NEXT] = nodeId;
                headerTableProperties[index][HT_LAST] = nodeId;
            }
            else
            {
                SetNext(headerTableProperties[index][HT_LAST], nodeId);
                headerTableProperties[index][HT_LAST] = nodeId;
            }
        }

        public int Attribute(int nodeId)
        {
            return this.attribute[nodeId];
        }

        public int ChildAtIndex(int nodeId, int index)
        {
            if (childCount[nodeId] < index)
            {
                return -1;
            }
            return nodeChildren[nodeId][index];
        }

        public int ChildCount(int nodeId)
        {
            return childCount[nodeId];
        }

        public int ChildWithAttribute(int nodeId, int childAttribute)
        {
            int length = childCount[nodeId];
            for (int i = 0; i < length; i++)
            {
                if (attribute[nodeChildren[nodeId][i]] == childAttribute)
                {
                    return nodeChildren[nodeId][i];
                }
            }
            return -1;
        }

        public void Clear()
        {
            nodes = 0;
            headerTableCount = 0;
            singlePath = true;
            Arrays.fill(headerTableLookup, -1);
            sortedSet.Clear();
            CreateRootNode();
        }

        public void ClearConditional()
        {
            for (int i = nodes - 1; i >= 0; i--)
            {
                conditional[i] = 0;
            }
        }

        public int Conditional(int nodeId)
        {
            return this.conditional[nodeId];
        }

        public long Count(int nodeId)
        {
            return nodeCount[nodeId];
        }

        public int CreateConditionalNode(int attributeValue, long count)
        {
            if (nodes >= this.attribute.Length)
            {
                Resize();
            }
            childCount[nodes] = 0;
            next[nodes] = -1;
            parent[nodes] = -1;
            conditional[nodes] = 0;
            this.attribute[nodes] = attributeValue;
            nodeCount[nodes] = count;

            if (nodeChildren[nodes] == null)
            {
                nodeChildren[nodes] = new int[DEFAULT_CHILDREN_INITIAL_SIZE];
            }

            return nodes++;
        }

        public int CreateNode(int parentNodeId, int attributeValue, long count)
        {
            if (nodes >= this.attribute.Length)
            {
                Resize();
            }

            childCount[nodes] = 0;
            next[nodes] = -1;
            parent[nodes] = parentNodeId;
            this.attribute[nodes] = attributeValue;
            nodeCount[nodes] = count;

            conditional[nodes] = 0;
            if (nodeChildren[nodes] == null)
            {
                nodeChildren[nodes] = new int[DEFAULT_CHILDREN_INITIAL_SIZE];
            }

            int childNodeId = nodes++;
            AddChild(parentNodeId, childNodeId);
            AddHeaderNext(attributeValue, childNodeId);
            return childNodeId;
        }

        public int CreateRootNode()
        {
            childCount[nodes] = 0;
            next[nodes] = -1;
            parent[nodes] = 0;
            attribute[nodes] = -1;
            nodeCount[nodes] = 0;
            if (nodeChildren[nodes] == null)
            {
                nodeChildren[nodes] = new int[DEFAULT_CHILDREN_INITIAL_SIZE];
            }
            return nodes++;
        }

        public int GetAttributeAtIndex(int index)
        {
            return headerTableAttributes[index];
        }

        public int GetHeaderNext(int attributeValue)
        {
            int index = GetHeaderIndex(attributeValue);
            return headerTableProperties[index][HT_NEXT];
        }

        public long GetHeaderSupportCount(int attributeValue)
        {
            int index = GetHeaderIndex(attributeValue);
            return headerTableAttributeCount[index];
        }

        public int[] GetHeaderTableAttributes()
        {
            int[] attributes = new int[headerTableCount];
            Array.Copy(headerTableAttributes, 0, attributes, 0, headerTableCount);
            return attributes;
        }

        public int GetHeaderTableCount()
        {
            return headerTableCount;
        }

        public bool IsEmpty()
        {
            return nodes <= 1;
        }

        public int Next(int nodeId)
        {
            return next[nodeId];
        }

        public int Parent(int nodeId)
        {
            return parent[nodeId];
        }

        public void RemoveHeaderNext(int attributeValue)
        {
            int index = GetHeaderIndex(attributeValue);
            headerTableProperties[index][HT_NEXT] = -1;
        }

        public void ReorderHeaderTable()
        {
            // Arrays.sort(headerTableAttributes, 0, headerTableCount);
            int i = 0;
            foreach (int attr in sortedSet)
            {
                headerTableAttributes[i++] = attr;
            }
        }

        public void ReplaceChild(int parentNodeId, int replacableNode, int childnodeId)
        {
            int max = childCount[parentNodeId];
            for (int i = 0; i < max; i++)
            {
                if (nodeChildren[parentNodeId][i] == replacableNode)
                {
                    nodeChildren[parentNodeId][i] = childnodeId;
                    parent[childnodeId] = parentNodeId;
                }
            }
        }

        public bool SetConditional(int nodeId, int conditionalNode)
        {
            if (nodeId < nodes)
            {
                this.conditional[nodeId] = conditionalNode;
                return true;
            }
            return false;
        }

        public bool SetNext(int nodeId, int nextNode)
        {
            if (nodeId < nodes)
            {
                this.next[nodeId] = nextNode;
                return true;
            }
            return false;
        }

        public bool SetParent(int nodeId, int parentNode)
        {
            if (nodeId < nodes)
            {
                this.parent[nodeId] = parentNode;

                int length = childCount[parentNode];
                if (length >= nodeChildren[parentNode].Length)
                {
                    ResizeChildren(parentNode);
                }
                nodeChildren[parentNode][length++] = nodeId;
                childCount[parentNode] = length;
                return true;
            }
            return false;
        }
        public bool SinglePath
        {
            get { return singlePath; }
            set { singlePath = value; }
        }

        private int GetHeaderIndex(int attributeValue)
        {
            if (attributeValue >= headerTableLookup.Length)
            {
                ResizeHeaderLookup(attributeValue);
            }
            int index = headerTableLookup[attributeValue];
            if (index == -1)
            { // if attribute didnt exist;
                if (headerTableCount >= headerTableAttributes.Length)
                {
                    ResizeHeaderTable();
                }
                headerTableAttributes[headerTableCount] = attributeValue;
                if (headerTableProperties[headerTableCount] == null)
                {
                    headerTableProperties[headerTableCount] = new int[HEADERTABLEBLOCKSIZE];
                }
                headerTableAttributeCount[headerTableCount] = 0;
                headerTableProperties[headerTableCount][HT_NEXT] = -1;
                headerTableProperties[headerTableCount][HT_LAST] = -1;
                index = headerTableCount++;
                headerTableLookup[attributeValue] = index;
                sortedSet.Add(attributeValue);
            }
            return index;
        }

        private void Resize()
        {
            int size = (int)(GROWTH_RATE * nodes);
            if (size < DEFAULT_INITIAL_SIZE)
            {
                size = DEFAULT_INITIAL_SIZE;
            }

            int[] oldChildCount = childCount;
            int[] oldAttribute = attribute;
            long[] oldnodeCount = nodeCount;
            int[] oldParent = parent;
            int[] oldNext = next;
            int[][] oldNodeChildren = nodeChildren;
            int[] oldConditional = conditional;

            childCount = new int[size];
            attribute = new int[size];
            nodeCount = new long[size];
            parent = new int[size];
            next = new int[size];

            nodeChildren = new int[size][];
            conditional = new int[size];

            Array.Copy(oldChildCount, 0, this.childCount, 0, nodes);
            Array.Copy(oldAttribute, 0, this.attribute, 0, nodes);
            Array.Copy(oldnodeCount, 0, this.nodeCount, 0, nodes);
            Array.Copy(oldParent, 0, this.parent, 0, nodes);
            Array.Copy(oldNext, 0, this.next, 0, nodes);
            Array.Copy(oldNodeChildren, 0, this.nodeChildren, 0, nodes);
            Array.Copy(oldConditional, 0, this.conditional, 0, nodes);
        }

        private void ResizeChildren(int nodeId)
        {
            int length = childCount[nodeId];
            int size = (int)(GROWTH_RATE * length);
            if (size < DEFAULT_CHILDREN_INITIAL_SIZE)
            {
                size = DEFAULT_CHILDREN_INITIAL_SIZE;
            }
            int[] oldNodeChildren = nodeChildren[nodeId];
            nodeChildren[nodeId] = new int[size];
            Array.Copy(oldNodeChildren, 0, this.nodeChildren[nodeId], 0, length);
        }

        private void ResizeHeaderLookup(int attributeValue)
        {
            int size = (int)(attributeValue * GROWTH_RATE);
            int[] oldLookup = headerTableLookup;
            headerTableLookup = new int[size];
            Arrays.fill(headerTableLookup, oldLookup.Length, size, -1);
            Array.Copy(oldLookup, 0, this.headerTableLookup, 0, oldLookup.Length);
        }

        private void ResizeHeaderTable()
        {
            int size = (int)(GROWTH_RATE * headerTableCount);
            if (size < DEFAULT_HEADER_TABLE_INITIAL_SIZE)
            {
                size = DEFAULT_HEADER_TABLE_INITIAL_SIZE;
            }

            int[] oldAttributes = headerTableAttributes;
            long[] oldAttributeCount = headerTableAttributeCount;
            int[][] oldProperties = headerTableProperties;
            headerTableAttributes = new int[size];
            headerTableAttributeCount = new long[size];
            headerTableProperties = new int[size][];
            Array.Copy(oldAttributes, 0, this.headerTableAttributes, 0,
              headerTableCount);
            Array.Copy(oldAttributeCount, 0, this.headerTableAttributeCount, 0,
              headerTableCount);
            Array.Copy(oldProperties, 0, this.headerTableProperties, 0,
              headerTableCount);
        }
    }
}
