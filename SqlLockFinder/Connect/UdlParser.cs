using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SqlLockFinder.Connect
{
    public class UdlParser
    {
        private string _default = "[oledb]" + Environment.NewLine +
                                  "; Everything after this line is an OLE DB initstring" + Environment.NewLine +
                                  "Provider=SQLOLEDB.1;";
        private bool initialized = false;

        public string ParseConnectionString(Stream file)
        {
            using (var reader = new StreamReader(file))
            {
                var udl = reader.ReadToEnd();
                ParseFile(udl);
            }

            return ConnectionString;
        }
        public string Catalog
        {
            get
            {
                GuardInitialized();
                var rex = new Regex("Catalog=([^;]*)");
                var m = rex.Match(ConnectionString);
                if (m.Success)
                {
                    return m.Groups[1].ToString();
                }

                return string.Empty;
            }
        }

        public string ConnectionString { get; set; }

        public string DataSource
        {
            get
            {
                GuardInitialized();
                var rex = new Regex("Data Source=([^;]*)");
                var m = rex.Match(ConnectionString);
                if (m.Success)
                {
                    return m.Groups[1].ToString();
                }

                return string.Empty;
            }
        }

        public string Password
        {
            get
            {
                GuardInitialized();
                var rex = new Regex("Password=([^;]*)");
                var m = rex.Match(ConnectionString);
                if (m.Success)
                {
                    var password = m.Groups[1].ToString();

                    if (password.StartsWith("\""))
                    {
                        rex = new Regex("Password=\"(.*)\"");
                        m = rex.Match(ConnectionString);
                        if (m.Success)
                        {
                            password = m.Groups[1].ToString();
                            password = password.Replace("\"\"", "\"");
                        }
                    }

                    return password;
                }

                return string.Empty;
            }
        }

        public string Provider { get; set; }

        public bool Success { get; set; }

        public string Username
        {
            get
            {
                GuardInitialized();
                var rex = new Regex("User ID=([^;]*)");
                var m = rex.Match(ConnectionString);
                if (m.Success)
                {
                    return m.Groups[1].ToString();
                }

                return string.Empty;
            }
        }
        private string ParseFile(string udl)
        {
            initialized = true;
            var rex = new Regex("(Provider[^;]*);(.*)", RegexOptions.Multiline);

            var match = rex.Match(udl);

            if (match.Success)
            {
                Provider = match.Groups[1].ToString();

                ConnectionString = match.Groups[2].ToString() + ";MultipleActiveResultSets=true";
                ConnectionString = ConnectionString.Replace(";Server SPN=\"\"", "");
                    
                Success = true;
            }

            return ConnectionString;
        }

        private void GuardInitialized()
        {
            if(! initialized) throw new Exception("Did not parse UDL file");
        }
    }
}