using MpMigrate.Data.Dal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using MpMigrate.Data.Entity;

namespace MpMigrate.Test
{
    
    
    /// <summary>
    ///This is a test class for MaestroPanel_SQLiteTest and is intended
    ///to contain all MaestroPanel_SQLiteTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MaestroPanel_SQLiteTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetPanelStats
        ///</summary>
        [TestMethod()]
        public void GetPanelStatsTest()
        {
            MaestroPanel_SQLite target = new MaestroPanel_SQLite(); // TODO: Initialize to an appropriate value
            target.LoadConnectionString(@"Data Source=C:\Program Files\MaestroPanel\Web\data\mast.sqlite;Version=3;BinaryGUID=False;New=True");
            
            PanelStats actual = target.GetPanelStats();
            Assert.AreNotEqual(actual, null);            
        }
    }
}
