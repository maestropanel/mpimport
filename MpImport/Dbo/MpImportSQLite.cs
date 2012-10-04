namespace PleskImport
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using PleskImport.Entity;
    using PleskImport.Properties;

    public class MpImportSQLite : DboFactory
    {
        //Domain, MsFTP modülü ile.
        public override List<Domain> GetDomains()
        {
            var _tmp = new List<Domain>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();

                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT Domain.DomainId, 
                                                                        Domain.Name, Domain.ExpirationDate, Domain.Status,
                                                                    DomainMsFTPUser.Username as ftpUserName, DomainMsFTPUser.Password as FtpPassword, 
                                                                    LoginAccount.UserName as DomainUser, LoginAccount.Password as DomainPassword
                                                                FROM Domain 
                                                                    LEFT JOIN DomainMsFtp ON DomainMsFtp.DomainId = Domain.DomainId
                                                                    LEFT JOIN DomainMsFTPUser ON DomainMsFTPUser.FtpId = DomainMsFtp.Id
                                                                    LEFT JOIN LoginAccount ON LoginAccount.LoginId = Domain.OwnerLoginId AND LoginAccount.UserName <> 'admin'", _conn))
                {
                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Domain();
                            _d.Id = Convert.ToInt32(_read["DomainId"]);
                            _d.Name = _read["Name"].ToString();
                            _d.Username = _read["ftpUserName"].ToString();
                            _d.Password = _read["FtpPassword"].ToString();

                            _d.Status = Convert.ToInt32(_read["Status"]);

                            if (!_read.IsDBNull(6))
                                _d.ClientName = _read["DomainUser"].ToString();

                            if (!_read.IsDBNull(7))
                                _d.DomainPassword = _read["DomainPassword"].ToString();

                            if (!_read.IsDBNull(2))
                                _d.Expiration = DateTime.Parse(_read["ExpirationDate"].ToString());

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        //MailEnabel Modülü için
        public override List<Email> GetEmails(string domainName)
        {
            var _tmp = new List<Email>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT  Domain.Name as name, Pu.Username as mail_name, Pu.Password as password,Pu.Quota as mbox_quota 
                                        FROM Domain 
                                            LEFT JOIN DomainPostOffice AS Po ON Po.DomainId = Domain.DomainId
                                            LEFT JOIN DomainPostOfficeUser As Pu ON Pu.PostOfficeId = Po.Id
                                            WHERE Domain.Name = $NAME", _conn))
                {
                    _cmd.CommandType = CommandType.Text;
                    _cmd.Parameters.AddWithValue("$NAME", domainName);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Email();
                            _d.Name = _read["mail_name"].ToString();
                            _d.DomainName = _read["name"].ToString();
                            _d.Password = _read["password"].ToString();
                            _d.Redirect = "";
                            _d.RedirectedEmail = "";
                            _d.Quota = _read.IsDBNull(3) ? -1d : Convert.ToDouble(_read["mbox_quota"]);

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override List<Database> GetDatabases(string domainName)
        {
            var _tmp = new List<Database>();
            _tmp.AddRange(GetMySqlDatabases(domainName));
            _tmp.AddRange(GetMsSqlDatabases(domainName));

            return _tmp;
        }

        private List<Database> GetMySqlDatabases(string domainName)
        {
            var _tmp = new List<Database>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT Sql.Id as db_id, D.Name as domain, Sql.Name as name FROM DomainMySQL as Sql
                                                                    LEFT JOIN Domain as D ON Sql.DomainId = D.DomainId
                                                                        WHERE D.Name = $NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("$NAME", domainName);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Database();
                            _d.Id = Convert.ToInt32(_read["db_id"]);
                            _d.Name = _read["name"].ToString();
                            _d.Domain = _read["domain"].ToString();
                            _d.DbType = "mysql";
                            _d.Users = GetMySqlDatabaseUsers(_d.Id);

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        private List<DatabaseUser> GetMySqlDatabaseUsers(int databaseId)
        {
            var _tmp = new List<DatabaseUser>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT Username, Password FROM DomainMySQLUser WHERE DatabaseId = $ID", _conn))
                {
                    _cmd.Parameters.AddWithValue("$ID", databaseId);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DatabaseUser();
                            _d.Username = _read["Username"].ToString();
                            _d.Password = _read["Password"].ToString();

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        private List<Database> GetMsSqlDatabases(string domainName)
        {
            var _tmp = new List<Database>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT Sql.Id as db_id, D.Name as domain, Sql.Name as name, ""mssql"" as type FROM DomainMsSQL as Sql
                                                                    LEFT JOIN Domain as D ON Sql.DomainId = D.DomainId
                                                                        WHERE D.Name = $NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("$NAME", domainName);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Database();
                            _d.Id = Convert.ToInt32(_read["db_id"]);
                            _d.Name = _read["name"].ToString();
                            _d.Domain = _read["domain"].ToString();
                            _d.DbType = "mssql";
                            _d.Users = GetMsSqlDatabaseUsers(_d.Id);

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        private List<DatabaseUser> GetMsSqlDatabaseUsers(int databaseId)
        {
            var _tmp = new List<DatabaseUser>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT Username, Password FROM DomainMsSQLUser WHERE DatabaseId = $ID", _conn))
                {
                    _cmd.Parameters.AddWithValue("$ID", databaseId);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DatabaseUser();
                            _d.Username = _read["Username"].ToString();
                            _d.Password = _read["Password"].ToString();

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override List<Subdomain> GetSubdomains(string domainName)
        {
            var _tmp = new List<Subdomain>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT SD.Name as name, D.Name as domain, SD.Username as login, FU.Password as password
		                                                        FROM SubDomain As SD 
		                                                        LEFT JOIN Domain As D ON D.DomainId = SD.DomainId
		                                                        LEFT JOIN DomainMsFTP As Ftp ON Ftp.DomainId = D.DomainId 
		                                                        LEFT JOIN DomainMsFTPUser As FU ON FU.FtpId = Ftp.Id AND FU.Username = SD.Username
                                                                WHERE D.Name = $NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("$NAME", domainName);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new Subdomain();
                            _d.Domain = _read["domain"].ToString();
                            _d.Login = _read["login"].ToString();
                            _d.Password = _read["password"].ToString();
                            _d.Name = _read["name"].ToString();
                            _d.UserType = "";

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }

        public override List<DomainAlias> GetDomainAliases(string domainName)
        {
            var _tmp = new List<DomainAlias>();

            using (SQLiteConnection _conn = new SQLiteConnection(Settings.Default.connectionString))
            {
                _conn.Open();
                using (SQLiteCommand _cmd = new SQLiteCommand(@"SELECT D.Name as domain, DA.Hostname as name FROM DomainAlias As DA 
                                                                LEFT JOIN Domain As D ON D.DomainId = Da.DomainId WHERE D.Name = $NAME", _conn))
                {
                    _cmd.Parameters.AddWithValue("$NAME", domainName);

                    using (SQLiteDataReader _read = _cmd.ExecuteReader())
                    {
                        while (_read.Read())
                        {
                            var _d = new DomainAlias();
                            _d.Domain = _read["domain"].ToString();
                            _d.Alias = _read["alias"].ToString();

                            _tmp.Add(_d);
                        }
                    }
                }
                _conn.Close();
            }

            return _tmp;
        }
    }
}
