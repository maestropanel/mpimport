//using MpMigrate.Data.Dal;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using MpMigrate.Data.Entity;
//using System.Collections.Generic;

//namespace MpMigrate.Test
//{
    
    
//    /// <summary>
//    ///This is a test class for Plesk_9_AccessTest and is intended
//    ///to contain all Plesk_9_AccessTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class Plesk_9_AccessTest
//    {


//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion


//        /// <summary>
//        ///A test for GetDomains
//        ///</summary>
//        [TestMethod()]
//        public void Plesk9AccessGetDomainsTest()
//        {
//            Plesk_9_Access target = new Plesk_9_Access();
//            target.LoadConnectionString("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=O:\\psa\\Plesk95.mdb;User Id=;Password=;");
//            List<Domain> actual = target.GetDomains();

//            Assert.AreNotEqual(null, actual);            
//        }

//        /// <summary>
//        ///A test for GetResellers
//        ///</summary>
//        [TestMethod()]
//        public void Plesk9AccessGetResellersTest()
//        {
//            Plesk_9_Access target = new Plesk_9_Access(); // TODO: Initialize to an appropriate value    
//            target.LoadConnectionString("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=O:\\psa\\Plesk95.mdb;User Id=;Password=;");

//            List<Reseller> actual = target.GetResellers();

//            Assert.AreNotEqual(null, actual);     
            
//        }
//    }
//}
