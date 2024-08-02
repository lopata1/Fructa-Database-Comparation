using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fructa_Database_Comparation
{
    internal class DatabaseManager
    {

        public Database headOffice;

        public Dictionary<string, Database> databases = new Dictionary<string, Database>();
        public Dictionary<string, string> locationIds = new Dictionary<string, string>();
        private Dictionary<string, string[]> _credentialsList;

        private DatabaseManager()
        {

            if (_credentialsList == null)
            {
                _credentialsList = new Dictionary<string, string[]>();
            }
        }

        private static readonly DatabaseManager _databaseManager = new DatabaseManager();

        public static DatabaseManager GetDatabaseManager()
        {
            return _databaseManager;
        }

        public void Add(string name, string address, string database, string user, string password, string code)
        {
            databases[name] = new Database(address, database, user, password, code);
        }

        public void Remove(string name)
        {
            databases.Remove(name);
        }
    }
}
