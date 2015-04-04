using MpMigrate.Core.Discovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace MpMigrate.Test
{
    
    
    /// <summary>
    ///This is a test class for WebsitePanel_DiscoverTest and is intended
    ///to contain all WebsitePanel_DiscoverTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WebsitePanel_DiscoverTest
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
        ///A test for SetDatabase
        ///</summary>
        [TestMethod()]
        public void SetDatabaseTest()
        {
            WebsitePanel_Discover target = new WebsitePanel_Discover(); // TODO: Initialize to an appropriate value
            target.SetDatabase();

            Debug.WriteLine("DB Name: "+ target.GetDatabaseName());
            Debug.WriteLine("Username: " + target.GetDatabaseUsername());
            Debug.WriteLine("Password: " + target.GetPanelPassword());

            Assert.IsNotNull(target);
        }
    }
}
