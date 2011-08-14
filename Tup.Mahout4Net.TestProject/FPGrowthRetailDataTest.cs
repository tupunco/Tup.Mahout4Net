using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Tup.Hadoop.Mapred;
using Tup.Mahout4Net.Common.Iterator;
using Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth;
using Tup.Mahout4Net.Fpm.PfpGrowth.FPGrowth.Convertors;

namespace Tup.Mahout4Net.TestProject
{
    /// <summary>
    /// FPGrowthRetailDataTest 的摘要说明
    /// </summary>
    [TestClass]
    public class FPGrowthRetailDataTest
    {
        public FPGrowthRetailDataTest()
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
        public void TestSpecificCaseFromRetailDataMinSup500()
        {
            FPGrowth<string> fp = new FPGrowth<string>();

            StringRecordIterator it = new StringRecordIterator(File.ReadAllLines(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "resources/retail.dat")), @"\s+");
            int pattern_41_36_39 = 0;
            foreach (var next in it)
            {
                List<string> items = next.Key;
                if (items.Contains("41") && items.Contains("36") && items.Contains("39"))
                {
                    pattern_41_36_39++;
                }
            }

            var results = new Dictionary<string, long>();

            var returnableFeatures = new HashSet<string>();
            returnableFeatures.Add("36");
            returnableFeatures.Add("39");
            returnableFeatures.Add("41");

            fp.GenerateTopKFrequentPatterns(it,
                fp.GenerateFList(it, 500),
                500,
                1000,
                returnableFeatures,
                new OutputCollector(results),
                new StatusUpdater());

            Assert.AreEqual(pattern_41_36_39, results["36,39,41"]);
        }

        /// <summary>
        /// 
        /// </summary>
        public class StatusUpdater : IStatusUpdater
        {
            #region IStatusUpdater 成员
            public void Update(string status)
            {
            }
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        public class OutputCollector
            : IOutputCollector<string, List<KeyValuePair<List<string>, long>>>
        {
            private Dictionary<string, long> m_results;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="results"></param>
            public OutputCollector(Dictionary<string, long> results)
            {
                m_results = results;
            }

            #region IOutputCollector<string,List<KeyValuePair<List<string>,long>>> 成员
            public void Collect(string paramK, List<KeyValuePair<List<string>, long>> paramV)
            {
                foreach (var v in paramV)
                {
                    m_results[string.Join(",", v.Key)] = v.Value;
                }
            }
            #endregion
        }
    }
}
