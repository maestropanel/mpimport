namespace MpMigrate.MaestroPanel.Api
{
    using MpMigrate.MaestroPanel.Api.Entity;
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;

    public class ApiClient
    {
        private string _apiKey;
        private string _apiUri;

        public ApiClient(string ApiKey, string apiHostdomain, int port = 9715, bool ssl = false)
        {
            _apiKey = ApiKey;
            _apiUri = String.Format("{2}://{0}:{1}/Api/v1", apiHostdomain, port, ssl ? "https" : "http");
        }

        public ApiResult DomainDelete(string name)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);

            return SendApi("Domain/Delete", "DELETE", _args);
        }

        public ApiResult DomainStart(string name)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);

            return SendApi("Domain/Start", "POST", _args);
        }

        public ApiResult DomainStop(string name)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);

            return SendApi("Domain/Stop", "POST", _args);
        }

        public string GeneratePassword(int Length)
        {
            return System.Web.Security.Membership.GeneratePassword(8, 2);
        }

        public ApiResult DomainCreate(string name, string planAlias, string username, string password, bool activedomainuser,
                                        string firstName = "", string lastName = "", string email = "", DateTime? expiration = null)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("planAlias", planAlias);
            _args.Add("username", username);
            _args.Add("password", password);
            _args.Add("activedomainuser", activedomainuser.ToString());
            _args.Add("firstname", firstName);
            _args.Add("lastname", lastName);
            _args.Add("email", email);

            if (expiration.HasValue)
                _args.Add("expiration", expiration.Value.ToString("yyyy-MM-dd"));

            return SendApi("Domain/Create", "POST", _args);
        }

        private ApiResult SendApi(string action, string method, NameValueCollection _parameters)
        {
            var _result = new ApiResult();
            var _uri = new Uri(String.Format("{0}/{1}", _apiUri, action));
            var contentType = String.Empty;

            try
            {
                HttpWebRequest request = WebRequest.Create(_uri) as HttpWebRequest;
                request.Method = method;
                request.Timeout = 240 * 1000;
                request.ContentType = "application/x-www-form-urlencoded";

                WriteData(ref request, _parameters);
                var _responseText = GetData(request, out contentType);

                _result = ApiResult.DeSerializeObject<ApiResult>(_responseText);
            }
            catch (Exception ex)
            {
                _result.Code = -1;
                _result.Message = ex.Message;
                _result.OperationResultString = ex.StackTrace;
            }

            return _result;
        }

        private T SendApi<T>(string action, string method, NameValueCollection _parameters)
        {
            var _result = default(T);
            var contentType = String.Empty;

            var _uri = 
                method == "GET" ? 
                new Uri(String.Format("{0}/{1}?{2}", _apiUri, action, ToQueryString(_parameters))):
                new Uri(String.Format("{0}/{1}", _apiUri, action));
                        
            HttpWebRequest request = WebRequest.Create(_uri) as HttpWebRequest;
            request.Method = method;
            request.Timeout = 240 * 1000;                        
            request.ContentType = "application/x-www-form-urlencoded";

            if(method != "GET")
                WriteData(ref request, _parameters);

            
            var _responseText = GetData(request, out contentType);

            if (contentType.StartsWith("text/xml"))
                _result = ApiResult.DeSerializeObject<T>(_responseText);
            else
                _result = JsonHelper<T>.JsonDeserialize(_responseText);
            
            return _result;
        }

        public ApiResult AddMailBox(string name, string account, string password, double quota, string redirect, string redirectEmail)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("account", account);
            _args.Add("password", password);
            _args.Add("quota", quota.ToString());
            _args.Add("redirect", redirect);
            _args.Add("remail", redirectEmail);

            return SendApi("Domain/AddMailBox", "POST", _args);
        }

        public ApiResult AddDatabase(string name, string dbtype, string database, string username, string password, int quota)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("dbtype", dbtype);
            _args.Add("database", database);
            _args.Add("username", username);
            _args.Add("password", password);
            _args.Add("quota", quota.ToString());

            return SendApi("Domain/AddDatabase", "POST", _args);
        }

        public ApiResult AddDatabaseUser(string name, string dbtype, string database, string username, string password)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("dbtype", dbtype);
            _args.Add("database", database);
            _args.Add("username", username);
            _args.Add("password", password);

            return SendApi("Domain/AddDatabaseUser", "POST", _args);
        }

        public ApiResult AddSubDomain(string name, string subdomain, string username, string password)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("subdomain", subdomain);
            _args.Add("ftpuser", username);

            return SendApi("Domain/AddSubDomain", "POST", _args);
        }

        public ApiResult AddAlias(string name, string alias)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("alias", alias);

            return SendApi("Domain/AddDomainAlias", "POST", _args);
        }

        public ApiResult AddFtpUser(string name, string account, string password, string homePath = "/", bool ronly = false)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("name", name);
            _args.Add("account", account);
            _args.Add("password", password);
            _args.Add("homePath", homePath);
            _args.Add("ronly", ronly.ToString());

            return SendApi("Domain/AddFtpAccount", "POST", _args);
        }

        public ApiResult ResellerCreate(string username, string password, string planAlias,
            string firstName, string lastName, string email, string country, string organization,
                string address1, string address2, string city, string province, string postalcode,
                    string phone, string fax)
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("username", username);
            _args.Add("password", password);
            _args.Add("planAlias", planAlias);
            _args.Add("firstName", firstName);
            _args.Add("lastname", lastName);
            _args.Add("email", email);
            _args.Add("country", country);
            _args.Add("organization", organization);
            _args.Add("address1", address1);
            _args.Add("address2", address2);
            _args.Add("city", city);
            _args.Add("province", province);
            _args.Add("postalcode", postalcode);
            _args.Add("phone", phone);
            _args.Add("fax", fax);

            return SendApi("Reseller/Create", "POST", _args);
        }

        public Whoami Whoami()
        {
            var _args = new NameValueCollection();
            _args.Add("key", _apiKey);
            _args.Add("format", "JSON");

            return SendApi<Whoami>("User/Whoami", "GET", _args);
        }

        private void WriteData(ref HttpWebRequest _request, NameValueCollection _parameters)
        {
            byte[] byteData = CreateParameters(_parameters);
            _request.ContentLength = byteData.Length;

            using (Stream postStream = _request.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }
        }

        private string GetData(HttpWebRequest _request, out string contentType)
        {
            contentType = String.Empty;
            var _response = String.Empty;
            using (HttpWebResponse response = _request.GetResponse() as HttpWebResponse)
            {                
                
                contentType = response.ContentType;
                StreamReader reader = new StreamReader(response.GetResponseStream());                
                _response = reader.ReadToEnd();
            }

            return _response;
        }

        private byte[] CreateParameters(NameValueCollection _parameters)
        {
            var _sb = new StringBuilder(_parameters.Count);

            foreach (var item in _parameters.AllKeys)
                _sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(item), HttpUtility.UrlEncode(_parameters[item]));

            _sb.Length -= 1;

            return UTF8Encoding.UTF8.GetBytes(_sb.ToString());
        }

        private string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            return string.Join("&", array);
        }
    }
}
