using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth;
using Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors;
using Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors.String;
using Tup.Mahout4Net.Utils;

namespace Tup.Mahout4Net.TestProject
{
    /// <summary>
    /// UnitTest1 的摘要说明
    /// </summary>
    [TestClass]
    public class FPGrowthTest
    {
        public FPGrowthTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性:
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMaxHeapFPGrowth()
        {
            FPGrowth<string> fp = new FPGrowth<string>();

            List<KeyValuePair<List<string>, long>> transactions = new List<KeyValuePair<List<string>, long>>();
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("E", "A", "D", "B"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("D", "A", "C", "E", "B"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("C", "A", "B", "E"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("B", "A", "D"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("D"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("D", "B"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("A", "D", "E"), 1L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("B", "C"), 1L));

            using (var outWriter = new StringWriter())
            {
                fp.GenerateTopKFrequentPatterns(
                    transactions,
                    fp.GenerateFList(transactions, 3),
                    3,
                    100,
                    new HashSet<string>(),
                    new StringOutputConverter(new SequenceFileOutputCollector<string, TopKStringPatterns>(outWriter, "{0}\t{1},")),
                    new ContextStatusUpdater());

                var res = @"C	([B,C],3),"
                        + @"E	([A,E],4), ([A,B,E],3), ([A,D,E],3),"
                        + @"A	([A],5), ([A,D],4), ([A,E],4), ([A,B],4), ([A,B,E],3), ([A,D,E],3), ([A,B,D],3),"
                        + @"D	([D],6), ([B,D],4), ([A,D],4), ([A,D,E],3), ([A,B,D],3),"
                        + @"B	([B],6), ([A,B],4), ([B,D],4), ([A,B,D],3), ([A,B,E],3), ([B,C],3),";
                Assert.AreEqual(outWriter.ToString(), res);
            }
        }

        /// <summary>
        /// Trivial test for MAHOUT-617
        /// </summary>
        [TestMethod]
        public void TestMaxHeapFPGrowthData1()
        {
            FPGrowth<string> fp = new FPGrowth<string>();

            ICollection<KeyValuePair<List<string>, long>> transactions = new List<KeyValuePair<List<string>, long>>();
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("X"), 12L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("Y"), 4L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("X", "Y"), 10L));

            using (var outWriter = new StringWriter())
            {
                Console.WriteLine(fp.GenerateFList(transactions, 2));

                fp.GenerateTopKFrequentPatterns(
                        transactions,
                        fp.GenerateFList(transactions, 2),
                        2,
                        100,
                        new HashSet<string>(),
                        new StringOutputConverter(new SequenceFileOutputCollector<string, TopKStringPatterns>(outWriter, "({0},{1}), ")),
                        new ContextStatusUpdater());

                var res = @"(Y,([Y],14), ([X,Y],10)), (X,([X],22), ([X,Y],10)), ";

                Assert.AreEqual(outWriter.ToString(), res);
            }
        }

        /// <summary>
        /// Trivial test for MAHOUT-617
        /// </summary>
        [TestMethod]
        public void TestMaxHeapFPGrowthData2() {
            FPGrowth<String> fp = new FPGrowth<String>();

            ICollection<KeyValuePair<List<string>, long>> transactions = new List<KeyValuePair<List<string>, long>>();
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("X"), 12L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("Y"), 4L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("X", "Y"), 10L));
            transactions.Add(new KeyValuePair<List<string>, long>(Arrays.asList("X", "Y", "Z"), 11L));

            using (var outWriter = new StringWriter())
            {
                Console.WriteLine(fp.GenerateFList(transactions, 2));

                fp.GenerateTopKFrequentPatterns(
                        transactions,
                        fp.GenerateFList(transactions, 2),
                        2,
                        100,
                        new HashSet<string>(),
                        new StringOutputConverter(new SequenceFileOutputCollector<string, TopKStringPatterns>(outWriter, "({0},{1}), ")),
                        new ContextStatusUpdater());

                var res = @"(Z,([X,Y,Z],11)), (Y,([Y],25), ([X,Y],21), ([X,Y,Z],11)), (X,([X],33), ([X,Y],21), ([X,Y,Z],11)), ";

                Assert.AreEqual(outWriter.ToString(), res);
            }
        }
    }
}
