using MpMigrate.Data.Dal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MpMigrate.Test
{
    
    
    /// <summary>
    ///This is a test class for WebSitePanel_MsSQLTest and is intended
    ///to contain all WebSitePanel_MsSQLTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WebSitePanel_MsSQLTest
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
        ///A test for SetCryptoKey
        ///</summary>
        [TestMethod()]
        public void SetCryptoKeyTest()
        {
            WebSitePanel_MsSQL target = new WebSitePanel_MsSQL(); // TODO: Initialize to an appropriate value
            target.SetCryptoKey();

            Assert.IsNotNull(target);
        }
    }
}