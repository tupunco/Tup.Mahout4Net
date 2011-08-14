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
using Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors;

namespace Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth
{
    /// <summary>
    /// Implementation of PFGrowth Algorithm with FP-Bonsai pruning
    /// </summary>
    /// <typeparam name="A">object type used as the cell items in a transaction list</typeparam>
    public class FPGrowth<A>
        where A : IComparable<A>
    {
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(typeof(FPGrowth<A>));

        //public static List<KeyValuePair<string,TopKStringPatterns>> readFrequentPattern(Configuration conf, Path path) {
        //  List<KeyValuePair<string,TopKStringPatterns>> ret = new List<KeyValuePair<string,TopKStringPatterns>>();
        //  // key is feature value is count
        //  foreach (KeyValuePair<Writable,TopKStringPatterns> record
        //       in new SequenceFileIterable<Writable,TopKStringPatterns>(path, true, conf)) {
        //    ret.add(new Pair<String,TopKStringPatterns>(record.getFirst().toString(),
        //                                                new TopKStringPatterns(record.getSecond().getPatterns())));
        //  }
        //  return ret;
        //}

        /// <summary>
        /// Generate the Feature Frequency list from the given transaction whose
        /// frequency > minSupport
        /// </summary>
        /// <param name="transactions">Iterator over the transaction database</param>
        /// <param name="minSupport">minSupport of the feature to be included</param>
        /// <returns>the List of features and their associated frequency as a Pair</returns>
        public List<KeyValuePair<A, long>> GenerateFList(IEnumerable<KeyValuePair<List<A>, long>> transactions, int minSupport)
        {
            Dictionary<A, long> attributeSupport = new Dictionary<A, long>();
            foreach (var transaction in transactions)
            {
                foreach (A attribute in transaction.Key)
                {
                    if (attributeSupport.ContainsKey(attribute))
                        attributeSupport[attribute] += transaction.Value;
                    else
                        attributeSupport[attribute] = transaction.Value;
                }
            }

            List<KeyValuePair<A, long>> fList = new List<KeyValuePair<A, long>>();
            foreach (var e in attributeSupport)
            {
                long value = e.Value;
                if (value >= minSupport)
                {
                    fList.Add(new KeyValuePair<A, long>(e.Key, value));
                }
            }

            fList.Sort((x, y) =>
                        {
                            var ret = y.Value.CompareTo(x.Value);
                            if (ret != 0)
                                return ret;
                            return x.Key.CompareTo(y.Key);
                        });

            return fList;
        }

        /// <summary>
        /// Generate Top K Frequent Patterns for every feature in returnableFeatures
        /// given a stream of transactions and the minimum support
        /// </summary>
        /// <param name="transactionStream">Iterator of transaction</param>
        /// <param name="frequencyList">list of frequent features and their support value</param>
        /// <param name="minSupport">minimum support of the transactions</param>
        /// <param name="k">Number of top frequent patterns to keep</param>
        /// <param name="returnableFeatures">
        /// set of features for which the frequent patterns are mined. If the
        /// set is empty or null, then top K patterns for every frequent item (an item
        /// whose support> minSupport) is generated
        /// </param>
        /// <param name="output">
        /// The output collector to which the the generated patterns are
        /// written
        /// </param>
        /// <param name="updater"></param>
        public void GenerateTopKFrequentPatterns(IEnumerable<KeyValuePair<List<A>, long>> transactionStream,
                                                       ICollection<KeyValuePair<A, long>> frequencyList,
                                                       long minSupport,
                                                       int k,
                                                       ICollection<A> returnableFeatures,
                                                       IOutputCollector<A, List<KeyValuePair<List<A>, long>>> output,
                                                       IStatusUpdater updater)
        {
            Dictionary<int, A> reverseMapping = new Dictionary<int, A>();
            Dictionary<A, int> attributeIdMapping = new Dictionary<A, int>();

            int id = 0;
            foreach (var feature in frequencyList)
            {
                A attrib = feature.Key;
                long frequency = feature.Value;
                if (frequency >= minSupport)
                {
                    attributeIdMapping.Add(attrib, id);
                    reverseMapping.Add(id++, attrib);
                }
            }

            long[] attributeFrequency = new long[attributeIdMapping.Count];
            foreach (var feature in frequencyList)
            {
                A attrib = feature.Key;
                long frequency = feature.Value;
                if (frequency < minSupport)
                {
                    break;
                }
                attributeFrequency[attributeIdMapping[attrib]] = frequency;
            }

            log.InfoFormat("Number of unique items {0}", frequencyList.Count);

            ICollection<int> returnFeatures = new HashSet<int>();
            if (returnableFeatures != null && returnableFeatures.Count != 0)
            {
                foreach (A attrib in returnableFeatures)
                {
                    if (attributeIdMapping.ContainsKey(attrib))
                    {
                        returnFeatures.Add(attributeIdMapping[attrib]);
                        log.InfoFormat("Adding Pattern {0}=>{1}", attrib, attributeIdMapping[attrib]);
                    }
                }
            }
            else
            {
                for (int j = 0; j < attributeIdMapping.Count; j++)
                {
                    returnFeatures.Add(j);
                }
            }

            log.InfoFormat("Number of unique pruned items {0}", attributeIdMapping.Count);

            GenerateTopKFrequentPatterns(new TransactionIterator<A>(transactionStream,
                attributeIdMapping), attributeFrequency, minSupport, k, reverseMapping.Count,
                    returnFeatures, new TopKPatternsOutputConverter<A>(output,
                    reverseMapping), updater);
        }

        //TODO private Dictionary<int, FrequentPatternMaxHeap> 
        /// <summary>
        /// Top K FpGrowth Algorithm 
        /// </summary>
        /// <param name="tree">to be mined</param>
        /// <param name="minSupportValue">minimum support of the pattern to keep</param>
        /// <param name="k">Number of top frequent patterns to keep</param>
        /// <param name="requiredFeatures">Set of int id's of features to mine</param>
        /// <param name="outputCollector">the Collector class which converts the given frequent pattern in int to A</param>
        /// <param name="updater"></param>
        private void FpGrowth(FPTree tree,
                                                            long minSupportValue,
                                                            int k,
                                                            ICollection<int> requiredFeatures,
                                                            TopKPatternsOutputConverter<A> outputCollector,
                                                            IStatusUpdater updater)
        {
            //TODO Dictionary<int, FrequentPatternMaxHeap> patterns = new Dictionary<int, FrequentPatternMaxHeap>();
            FPTreeDepthCache treeCache = new FPTreeDepthCache();
            for (int i = tree.GetHeaderTableCount() - 1; i >= 0; i--)
            {
                int attribute = tree.GetAttributeAtIndex(i);
                if (requiredFeatures.Contains(attribute))
                {
                    log.InfoFormat("Mining FTree Tree for all patterns with {0}", attribute);
                    long minSupport = minSupportValue;
                    FrequentPatternMaxHeap frequentPatterns = Growth(tree, ref minSupport, k,
                                                                     treeCache, 0, attribute, updater);
                    //TODO patterns.Add(attribute, frequentPatterns);
                    outputCollector.Collect(attribute, frequentPatterns);
                    frequentPatterns = null;

                    minSupportValue = Math.Max(minSupportValue, minSupport / 2);

                    //TODO log.InfoFormat("Found {0} Patterns with Least Support {1}",
                    //                patterns[attribute].Count, patterns[attribute].LeastSupport);
                }
            }
            log.InfoFormat("Tree Cache: First Level: Cache hits={0} Cache Misses={1}",
                                    treeCache.Hits, treeCache.Misses);

            //TODO return patterns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="k"></param>
        /// <param name="minSupport"></param>
        /// <returns></returns>
        private static FrequentPatternMaxHeap GenerateSinglePathPatterns(FPTree tree,
                                                                         int k,
                                                                         long minSupport)
        {
            FrequentPatternMaxHeap frequentPatterns = new FrequentPatternMaxHeap(k, false);

            int tempNode = FPTree.ROOTNODEID;
            Pattern frequentItem = new Pattern();
            while (tree.ChildCount(tempNode) != 0)
            {
                if (tree.ChildCount(tempNode) > 1)
                {
                    log.InfoFormat("This should not happen {0} {1}", tree.ChildCount(tempNode),
                      tempNode);
                }
                tempNode = tree.ChildAtIndex(tempNode, 0);
                if (tree.Count(tempNode) >= minSupport)
                {
                    frequentItem.Add(tree.Attribute(tempNode), tree.Count(tempNode));
                }
            }
            if (frequentItem.Length > 0)
            {
                frequentPatterns.Insert(frequentItem);
            }

            return frequentPatterns;
        }

        /**
         * Internal TopKFrequentPattern Generation algorithm, which represents the A's
         * as ints and transforms features to use only ints
         *
         * @param transactions
         *          Transaction database Iterator
         * @param attributeFrequency
         *          array representing the Frequency of the corresponding attribute id
         * @param minSupport
         *          minimum support of the pattern to be mined
         * @param k
         *          Max value of the Size of the Max-Heap in which Patterns are held
         * @param featureSetSize
         *          number of features
         * @param returnFeatures
         *          the id's of the features for which Top K patterns have to be mined
         * @param topKPatternsOutputCollector
         *          the outputCollector which transforms the given Pattern in int
         *          format to the corresponding A Format
         * @return Top K frequent patterns for each attribute
         */
        //TODO private Dictionary<int, FrequentPatternMaxHeap> 
        private void GenerateTopKFrequentPatterns(
          IEnumerable<KeyValuePair<int[], long>> transactions,
          long[] attributeFrequency,
          long minSupport,
          int k,
          int featureSetSize,
          ICollection<int> returnFeatures, TopKPatternsOutputConverter<A> topKPatternsOutputCollector,
          IStatusUpdater updater)
        {
            FPTree tree = new FPTree(featureSetSize);
            for (int i = 0; i < featureSetSize; i++)
            {
                tree.AddHeaderCount(i, attributeFrequency[i]);
            }

            // Constructing initial FPTree from the list of transactions
            int nodecount = 0;
            // int attribcount = 0;
            int ni = 0;
            var t = transactions.GetEnumerator();
            while (t.MoveNext())
            {
                var transaction = t.Current;
                Array.Sort(transaction.Key);
                // attribcount += transaction.length;
                nodecount += TreeAddCount(tree, transaction.Key, transaction.Value, minSupport, attributeFrequency);
                ni++;
                if (ni % 10000 == 0)
                {
                    log.InfoFormat("FPTree Building: Read {0} Transactions", ni);
                }
            }

            log.InfoFormat("Number of Nodes in the FP Tree: {0}", nodecount);

            //TODO  return 
            FpGrowth(tree, minSupport, k, returnFeatures, topKPatternsOutputCollector, updater);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="minSupportMutable"></param>
        /// <param name="k"></param>
        /// <param name="treeCache"></param>
        /// <param name="level"></param>
        /// <param name="currentAttribute"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        private static FrequentPatternMaxHeap Growth(FPTree tree,
                                                     ref long minSupportMutable,
                                                     int k,
                                                     FPTreeDepthCache treeCache,
                                                     int level,
                                                     int currentAttribute,
                                                     IStatusUpdater updater)
        {

            FrequentPatternMaxHeap frequentPatterns = new FrequentPatternMaxHeap(k,
              true);

            int i = Array.BinarySearch(tree.GetHeaderTableAttributes(),
              currentAttribute);
            if (i < 0)
            {
                return frequentPatterns;
            }

            int headerTableCount = tree.GetHeaderTableCount();

            while (i < headerTableCount)
            {
                int attribute = tree.GetAttributeAtIndex(i);
                long count = tree.GetHeaderSupportCount(attribute);
                if (count < minSupportMutable)
                {
                    i++;
                    continue;
                }
                updater.Update("FPGrowth Algorithm for a given feature: " + attribute);
                FPTree conditionalTree = treeCache.GetFirstLevelTree(attribute);
                if (conditionalTree.IsEmpty())
                {
                    TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                      minSupportMutable, conditionalTree, tree);
                    // printTree(conditionalTree);

                }

                FrequentPatternMaxHeap returnedPatterns;
                if (attribute == currentAttribute)
                {

                    returnedPatterns = GrowthTopDown(conditionalTree, ref minSupportMutable, k,
                      treeCache, level + 1, true, currentAttribute, updater);

                    frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                      attribute, count, true);
                }
                else
                {
                    returnedPatterns = GrowthTopDown(conditionalTree, ref minSupportMutable, k,
                      treeCache, level + 1, false, currentAttribute, updater);
                    frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                      attribute, count, false);
                }
                if (frequentPatterns.IsFull && minSupportMutable < frequentPatterns.LeastSupport)
                {
                    minSupportMutable = frequentPatterns.LeastSupport;
                }
                i++;
            }

            return frequentPatterns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="minSupportMutable"></param>
        /// <param name="k"></param>
        /// <param name="treeCache"></param>
        /// <param name="level"></param>
        /// <param name="conditionalOfCurrentAttribute"></param>
        /// <param name="currentAttribute"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        private static FrequentPatternMaxHeap GrowthBottomUp(FPTree tree,
                                                             ref long minSupportMutable,
                                                             int k,
                                                             FPTreeDepthCache treeCache,
                                                             int level,
                                                             bool conditionalOfCurrentAttribute,
                                                             int currentAttribute,
                                                             IStatusUpdater updater)
        {

            FrequentPatternMaxHeap frequentPatterns = new FrequentPatternMaxHeap(k,
              false);

            if (!conditionalOfCurrentAttribute)
            {
                int index = Array.BinarySearch(tree.GetHeaderTableAttributes(),
                  currentAttribute);
                if (index < 0)
                {
                    return frequentPatterns;
                }
                else
                {
                    int attribute = tree.GetAttributeAtIndex(index);
                    long count = tree.GetHeaderSupportCount(attribute);
                    if (count < minSupportMutable)
                    {
                        return frequentPatterns;
                    }
                }
            }

            if (tree.SinglePath)
            {
                return GenerateSinglePathPatterns(tree, k, minSupportMutable);
            }

            updater.Update("Bottom Up FP Growth");
            for (int i = tree.GetHeaderTableCount() - 1; i >= 0; i--)
            {
                int attribute = tree.GetAttributeAtIndex(i);
                long count = tree.GetHeaderSupportCount(attribute);
                if (count < minSupportMutable)
                {
                    continue;
                }
                FPTree conditionalTree = treeCache.GetTree(level);

                FrequentPatternMaxHeap returnedPatterns;
                if (conditionalOfCurrentAttribute)
                {
                    TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                      minSupportMutable, conditionalTree, tree);
                    returnedPatterns = GrowthBottomUp(conditionalTree, ref minSupportMutable,
                      k, treeCache, level + 1, true, currentAttribute, updater);

                    frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                      attribute, count, true);
                }
                else
                {
                    if (attribute == currentAttribute)
                    {
                        TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                          minSupportMutable, conditionalTree, tree);
                        returnedPatterns = GrowthBottomUp(conditionalTree, ref minSupportMutable,
                          k, treeCache, level + 1, true, currentAttribute, updater);

                        frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                          attribute, count, true);
                    }
                    else if (attribute > currentAttribute)
                    {
                        TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                          minSupportMutable, conditionalTree, tree);
                        returnedPatterns = GrowthBottomUp(conditionalTree, ref minSupportMutable,
                          k, treeCache, level + 1, false, currentAttribute, updater);
                        frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                          attribute, count, false);
                    }
                }

                if (frequentPatterns.IsFull && minSupportMutable < frequentPatterns.LeastSupport)
                {
                    minSupportMutable = frequentPatterns.LeastSupport;
                }
            }

            return frequentPatterns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="minSupportMutable"></param>
        /// <param name="k"></param>
        /// <param name="treeCache"></param>
        /// <param name="level"></param>
        /// <param name="conditionalOfCurrentAttribute"></param>
        /// <param name="currentAttribute"></param>
        /// <param name="updater"></param>
        /// <returns></returns>
        private static FrequentPatternMaxHeap GrowthTopDown(FPTree tree,
                                                            ref long minSupportMutable,
                                                            int k,
                                                            FPTreeDepthCache treeCache,
                                                            int level,
                                                            bool conditionalOfCurrentAttribute,
                                                            int currentAttribute,
                                                            IStatusUpdater updater)
        {

            FrequentPatternMaxHeap frequentPatterns = new FrequentPatternMaxHeap(k,
              true);

            if (!conditionalOfCurrentAttribute)
            {
                int index = Array.BinarySearch(tree.GetHeaderTableAttributes(),
                  currentAttribute);
                if (index < 0)
                {
                    return frequentPatterns;
                }
                else
                {
                    int attribute = tree.GetAttributeAtIndex(index);
                    long count = tree.GetHeaderSupportCount(attribute);
                    if (count < minSupportMutable)
                    {
                        return frequentPatterns;
                    }
                }
            }

            if (tree.SinglePath)
            {
                return GenerateSinglePathPatterns(tree, k, minSupportMutable);
            }

            updater.Update("Top Down Growth:");

            for (int i = 0; i < tree.GetHeaderTableCount(); i++)
            {
                int attribute = tree.GetAttributeAtIndex(i);
                long count = tree.GetHeaderSupportCount(attribute);
                if (count < minSupportMutable)
                {
                    continue;
                }

                FPTree conditionalTree = treeCache.GetTree(level);

                FrequentPatternMaxHeap returnedPatterns;
                if (conditionalOfCurrentAttribute)
                {
                    TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                      minSupportMutable, conditionalTree, tree);

                    returnedPatterns = GrowthBottomUp(conditionalTree, ref minSupportMutable,
                      k, treeCache, level + 1, true, currentAttribute, updater);
                    frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                      attribute, count, true);

                }
                else
                {
                    if (attribute == currentAttribute)
                    {
                        TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                          minSupportMutable, conditionalTree, tree);
                        returnedPatterns = GrowthBottomUp(conditionalTree, ref minSupportMutable,
                          k, treeCache, level + 1, true, currentAttribute, updater);
                        frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                          attribute, count, true);

                    }
                    else if (attribute > currentAttribute)
                    {
                        TraverseAndBuildConditionalFPTreeData(tree.GetHeaderNext(attribute),
                          minSupportMutable, conditionalTree, tree);
                        returnedPatterns = GrowthBottomUp(conditionalTree, ref minSupportMutable,
                          k, treeCache, level + 1, false, currentAttribute, updater);
                        frequentPatterns = MergeHeap(frequentPatterns, returnedPatterns,
                          attribute, count, false);

                    }
                }
                if (frequentPatterns.IsFull && minSupportMutable < frequentPatterns.LeastSupport)
                {
                    minSupportMutable = frequentPatterns.LeastSupport;
                }
            }

            return frequentPatterns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frequentPatterns"></param>
        /// <param name="returnedPatterns"></param>
        /// <param name="attribute"></param>
        /// <param name="count"></param>
        /// <param name="addAttribute"></param>
        /// <returns></returns>
        private static FrequentPatternMaxHeap MergeHeap(FrequentPatternMaxHeap frequentPatterns,
                                                        FrequentPatternMaxHeap returnedPatterns,
                                                        int attribute,
                                                        long count,
                                                        bool addAttribute)
        {
            frequentPatterns.AddAll(returnedPatterns, attribute, count);
            if (frequentPatterns.Addable(count) && addAttribute)
            {
                Pattern p = new Pattern();
                p.Add(attribute, count);
                frequentPatterns.Insert(p);
            }

            return frequentPatterns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstConditionalNode"></param>
        /// <param name="minSupport"></param>
        /// <param name="conditionalTree"></param>
        /// <param name="tree"></param>
        private static void TraverseAndBuildConditionalFPTreeData(int firstConditionalNode,
                                                                  long minSupport,
                                                                  FPTree conditionalTree,
                                                                  FPTree tree)
        {

            // Build Subtable
            int conditionalNode = firstConditionalNode;

            while (conditionalNode != -1)
            {
                long nextNodeCount = tree.Count(conditionalNode);
                int pathNode = tree.Parent(conditionalNode);
                int prevConditional = -1;

                while (pathNode != 0)
                { // dummy root node
                    int attribute = tree.Attribute(pathNode);
                    if (tree.GetHeaderSupportCount(attribute) < minSupport)
                    {
                        pathNode = tree.Parent(pathNode);
                        continue;
                    }
                    // update and increment the headerTable Counts
                    conditionalTree.AddHeaderCount(attribute, nextNodeCount);

                    int conditional = tree.Conditional(pathNode);
                    // if its a new conditional tree node

                    if (conditional == 0)
                    {
                        tree.SetConditional(pathNode, conditionalTree.CreateConditionalNode(
                          attribute, 0));
                        conditional = tree.Conditional(pathNode);
                        conditionalTree.AddHeaderNext(attribute, conditional);
                    }
                    else
                    {
                        conditionalTree.SinglePath = false;
                    }

                    if (prevConditional != -1)
                    { // if there is a child element
                        conditionalTree.SetParent(prevConditional, conditional);
                    }

                    conditionalTree.AddCount(conditional, nextNodeCount);
                    prevConditional = conditional;

                    pathNode = tree.Parent(pathNode);

                }
                if (prevConditional != -1)
                {
                    conditionalTree.SetParent(prevConditional, FPTree.ROOTNODEID);
                    if (conditionalTree.ChildCount(FPTree.ROOTNODEID) > 1
                        && conditionalTree.SinglePath)
                    {
                        conditionalTree.SinglePath = false;

                    }
                }
                conditionalNode = tree.Next(conditionalNode);
            }

            tree.ClearConditional();
            conditionalTree.ReorderHeaderTable();
            PruneFPTree(minSupport, conditionalTree);
            // prune Conditional Tree

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minSupport"></param>
        /// <param name="tree"></param>
        private static void PruneFPTree(long minSupport, FPTree tree)
        {
            for (int i = 0; i < tree.GetHeaderTableCount(); i++)
            {
                int currentAttribute = tree.GetAttributeAtIndex(i);
                if (tree.GetHeaderSupportCount(currentAttribute) < minSupport)
                {
                    int nextNode = tree.GetHeaderNext(currentAttribute);
                    tree.RemoveHeaderNext(currentAttribute);
                    while (nextNode != -1)
                    {

                        int mychildCount = tree.ChildCount(nextNode);

                        int parentNode = tree.Parent(nextNode);

                        for (int j = 0; j < mychildCount; j++)
                        {
                            int myChildId = tree.ChildAtIndex(nextNode, j);
                            tree.ReplaceChild(parentNode, nextNode, myChildId);
                        }
                        nextNode = tree.Next(nextNode);
                    }

                }
            }

            for (int i = 0; i < tree.GetHeaderTableCount(); i++)
            {
                int currentAttribute = tree.GetAttributeAtIndex(i);
                int nextNode = tree.GetHeaderNext(currentAttribute);

                Dictionary<int, int> prevNode = new Dictionary<int, int>();
                int justPrevNode = -1;
                while (nextNode != -1)
                {

                    int parent = tree.Parent(nextNode);

                    if (prevNode.ContainsKey(parent))
                    {
                        int prevNodeId = prevNode[parent];
                        if (tree.ChildCount(prevNodeId) <= 1 && tree.ChildCount(nextNode) <= 1)
                        {
                            tree.AddCount(prevNodeId, tree.Count(nextNode));
                            tree.AddCount(nextNode, -1 * tree.Count(nextNode));
                            if (tree.ChildCount(nextNode) == 1)
                            {
                                tree.AddChild(prevNodeId, tree.ChildAtIndex(nextNode, 0));
                                tree.SetParent(tree.ChildAtIndex(nextNode, 0), prevNodeId);
                            }
                            tree.SetNext(justPrevNode, tree.Next(nextNode));
                        }
                    }
                    else
                    {
                        prevNode.Add(parent, nextNode);
                    }
                    justPrevNode = nextNode;
                    nextNode = tree.Next(nextNode);
                }
            }

            // prune Conditional Tree
        }

        /**
         * Create FPTree with node counts incremented by addCount variable given the
         * root node and the List of Attributes in transaction sorted by support
         *
         * @param tree
         *          object to which the transaction has to be added to
         * @param myList
         *          List of transactions sorted by support
         * @param addCount
         *          amount by which the Node count has to be incremented
         * @param minSupport
         *          the MutableLong value which contains the current value(dynamic) of
         *          support
         * @param attributeFrequency
         *          the list of attributes and their frequency
         * @return the number of new nodes added
         */
        private static int TreeAddCount(FPTree tree,
                                        int[] myList,
                                        long addCount,
                                        long minSupport,
                                        long[] attributeFrequency)
        {

            int temp = FPTree.ROOTNODEID;
            int ret = 0;
            bool addCountMode = true;

            foreach (int attribute in myList)
            {
                if (attributeFrequency[attribute] < minSupport)
                {
                    return ret;
                }
                int child;
                if (addCountMode)
                {
                    child = tree.ChildWithAttribute(temp, attribute);
                    if (child == -1)
                    {
                        addCountMode = false;
                    }
                    else
                    {
                        tree.AddCount(child, addCount);
                        temp = child;
                    }
                }
                if (!addCountMode)
                {
                    child = tree.CreateNode(temp, attribute, addCount);
                    temp = child;
                    ret++;
                }
            }

            return ret;

        }
    }
}