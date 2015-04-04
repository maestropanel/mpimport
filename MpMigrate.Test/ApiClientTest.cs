//using MpMigrate.MaestroPanel.Api;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using MpMigrate.MaestroPanel.Api.Entity;
//using System.Collections.Generic;

//namespace MpMigrate.Test
//{
    
    
//    /// <summary>
//    ///This is a test class for ApiClientTest and is intended
//    ///to contain all ApiClientTest Unit Tests
//    ///</summary>
//    [TestClass()]
//    public class ApiClientTest
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
//        ///A test for Whoami
//        ///</summary>
//        //[TestMethod()]
//        //public void WhoamiTest()
//        //{
//        //    string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//        //    string apiHostdomain = "localhost";
//        //    int port = 28411;
//        //    bool ssl = false;
//        //    string format = "XML";
//        //    bool suppressResponse = false;
            
//        //    ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse); // TODO: Initialize to an appropriate value
            
//        //    var actual = target.Whoami();
            
//        //    var who = actual.Details;

//        //    Assert.AreEqual(0, actual.ErrorCode);            
//        //}

//        /// <summary>
//        ///A test for DomainCreate
//        ///</summary>
//        [TestMethod()]
//        public void DomainCreateTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "JSON";
//            bool suppressResponse = false;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);
//            string name = "akamai.com";
//            string planAlias = "default";
//            string username = "akamai.com";
//            string password = "osman12!";
//            bool activedomainuser = true;
//            string firstName = "Hakan";
//            string lastName = "Akyol";
//            string email = "hakyol@mail.com";
//            DateTime? expiration = DateTime.Now.AddYears(1);
            
            
//            var actual = target.DomainCreate(name, planAlias, username, password, activedomainuser, firstName, lastName, email, expiration);

//            Assert.AreEqual(0, actual.ErrorCode);
            
//        }

//        /// <summary>
//        ///A test for DomainDelete
//        ///</summary>
//        [TestMethod()]
//        public void DomainDeleteTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = false;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse); // TODO: Initialize to an appropriate value
//            string name = "akamai.com";
            
//            ApiResult<DomainOperationsResult> actual = target.DomainDelete(name);

//            Assert.AreEqual(0, actual.ErrorCode);            
//        }

//        /// <summary>
//        ///A test for ResellerSetLimit
//        ///</summary>
//        [TestMethod()]
//        public void ResellerSetLimitTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = false;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);
//            string username = "c1982";
//            int maxdomain = 11; // TODO: Initialize to an appropriate value
//            int maxdiskspace = 700; // TODO: Initialize to an appropriate value
//            int maxmailbox = 500; // TODO: Initialize to an appropriate value
//            int maxftpuser = 11; // TODO: Initialize to an appropriate value
//            int maxsubdomain = 12; // TODO: Initialize to an appropriate value
//            int maxdomainalias = 13; // TODO: Initialize to an appropriate value
//            int totalwebtraffic = 14; // TODO: Initialize to an appropriate value
//            int totalmailspace = 15; // TODO: Initialize to an appropriate value
//            int maxwebtraffic = 16; // TODO: Initialize to an appropriate value
//            int maxftptraffic =17; // TODO: Initialize to an appropriate value
//            int maxmailtraffic = 18; // TODO: Initialize to an appropriate value
//            int maxmysql = 10; // TODO: Initialize to an appropriate value
//            int maxmysqluser = 10;
//            int maxmysqlspace = 10;
//            int maxmssql = 10;
//            int maxmssqluser = 10;
//            int maxmssqlspace = 10;
            
//            ApiResult<ResellerOperationResult> actual = target.ResellerSetLimit(username, maxdomain, 
//                                                                                    maxdiskspace, 
//                                                                                    maxmailbox,
//                                                                                    maxftpuser,
//                                                                                    maxsubdomain, 
//                                                                                    maxdomainalias, 
//                                                                                    totalwebtraffic, 
//                                                                                    totalmailspace, 
//                                                                                    maxwebtraffic, 
//                                                                                    maxftptraffic, 
//                                                                                    maxmailtraffic,
//                                                                                    maxmysql, 
//                                                                                    maxmysqluser, 
//                                                                                    maxmysqlspace, 
//                                                                                    maxmssql, 
//                                                                                    maxmssqluser, 
//                                                                                    maxmssqlspace);

//            Assert.AreEqual(0, actual.ErrorCode);            
//        }

//        /// <summary>
//        ///A test for SetForwarding
//        ///</summary>
//        [TestMethod()]
//        public void SetForwardingTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = true;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);

//            string name = "bayidomain.com";
//            bool enabled = false;
//            string destination = "http://www.oguzhan.info";
//            bool exacDestination = false;
//            bool childOnly = false;
//            string statusCode = "Found";
            
//            ApiResult<DomainOperationsResult> actual = target.SetForwarding(name, enabled, destination, exacDestination, childOnly, statusCode);

//            Assert.AreEqual(0, actual.ErrorCode);    
//        }

//        /// <summary>
//        ///A test for SetDnsZone
//        ///</summary>
//        [TestMethod()]
//        public void SetDnsZoneTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = true;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);

//            string name = "bayidomain.com";
//            int soa_expired = 3600;
//            int soa_ttl = 172800;
//            int soa_refresh = 8640;
//            string soa_email = "api.bayidomain.com";
//            int soa_retry = 7200;
//            int soa_serial = 111;
//            string primaryServer = "ns1.zamazing.com";

//            List<DnsZoneRecordItem> records = new List<DnsZoneRecordItem>();
//            records.Add(new DnsZoneRecordItem() { name = "cms", type = "A", value = "10.0.0.1" });
//            records.Add(new DnsZoneRecordItem() { name = "ftp", type = "A", value = "10.0.0.1" });
//            records.Add(new DnsZoneRecordItem() { name = "webmail", type = "CNAME", value = "bayidomain.com" });
//            records.Add(new DnsZoneRecordItem() { name = "@", type = "NS", value = "ns1.bayidomain.com" });
//            records.Add(new DnsZoneRecordItem() { name = "@", type = "NS", value = "ns2.bayidomain.com" });
//            records.Add(new DnsZoneRecordItem() { name = "@", type = "MX", value = "apmx1.google.com" });
            
//            ApiResult<DomainOperationsResult> actual = target.SetDnsZone(name, soa_expired, soa_ttl, soa_refresh, soa_email, soa_retry, soa_serial, primaryServer, records);
//            Assert.AreEqual(0, actual.ErrorCode);            
//        }

//        [TestMethod()]
//        public void SetLimitsTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = true;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);

//            string name = "bayidomain.com";
            
//            int maxdiskspace = 700;
//            int maxmailbox = 500;
            
//            int maxftpuser = 11;

//            int maxsubdomain = 12;
//            int maxdomainalias = 13;
//            int totalwebtraffic = 14;
//            int totalmailspace = 15;
//            int maxwebtraffic = 16;
//            int maxftptraffic = 17;
//            int maxmailtraffic = 18;
//            int maxmysql = 10;
//            int maxmysqluser = 10;
//            int maxmysqlspace = 10;
//            int maxmssql = 10;
//            int maxmssqluser = 10;
//            int maxmssqlspace = 10;

//            ApiResult<DomainOperationsResult> actual = target.SetLimits(name,
//                                                                        maxdiskspace,
//                                                                        maxmailbox,
//                                                                        maxftpuser,
//                                                                        maxsubdomain,
//                                                                        maxdomainalias,
//                                                                        totalwebtraffic,
//                                                                        totalmailspace,
//                                                                        maxwebtraffic,
//                                                                        maxftptraffic,
//                                                                        maxmailtraffic,
//                                                                        maxmysql,
//                                                                        maxmysqluser,
//                                                                        maxmysqlspace,
//                                                                        maxmssql,
//                                                                        maxmssqluser,
//                                                                        maxmssqlspace);

//            Assert.AreEqual(0, actual.ErrorCode); 
//        }


//        [TestMethod()]
//        public void AddDnsRecordTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = true;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);
//            string name = "bayidomain.com";

//            ApiResult<DomainOperationsResult> actual = target.AddDnsRecord(name, "A", "mail", "10.0.0.2", 0);
//            Assert.AreEqual(0, actual.ErrorCode); 
//        }

//        [TestMethod()]
//        public void DeleteDnsRecordTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = true;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);
//            string name = "bayidomain.com";

//            ApiResult<DomainOperationsResult> actual = target.DeleteDnsRecord(name, "A", "mail", "10.0.0.2", 0);
//            Assert.AreEqual(0, actual.ErrorCode);
//        }

//        [TestMethod()]
//        public void ChangeResellerTest()
//        {
//            string ApiKey = "1_9bd61d3da73040c3a8b214afb25e4656";
//            string apiHostdomain = "localhost";
//            int port = 28411;
//            bool ssl = false;
//            string format = "XML";
//            bool suppressResponse = true;

//            ApiClient target = new ApiClient(ApiKey, apiHostdomain, port, ssl, format, suppressResponse);

//            ApiResult<DomainOperationsResult> actual = target.ChangeReseller("bayidomainx.com", "c1982");
//            Assert.AreEqual(0, actual.ErrorCode);
//        }


//    }
//}
