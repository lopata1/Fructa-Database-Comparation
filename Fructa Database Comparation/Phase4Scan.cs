using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using DevExpress.Pdf.Native.BouncyCastle.Utilities.Encoders;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using DevExpress.CodeParser;

namespace Fructa_Database_Comparation
{
    internal class Phase4Scan
    {
        private int _threads;
        private DatabaseManager _databaseManager;
        private Database _backOffice;
        private string _documentsSql;
        public Phase4Scan(Database backOffice, int threads, string documentsSql)
        {
            _backOffice = backOffice;
            _threads = threads;
            _databaseManager = DatabaseManager.GetDatabaseManager();
            _documentsSql = documentsSql;
        }

        public DataTable CheckBackOffice()
        {
            Database headOffice = _databaseManager.headOffice;


            string databaseName = _databaseManager.databases.FirstOrDefault(x => x.Value.address == _backOffice.address).Key;

            ChecksumComparator checksumComparator = new ChecksumComparator(_threads);

            string location = _databaseManager.locationIds[databaseName];

            string date = DateTime.Now.ToString("yyyy-MM-dd");

            DataTable missingData = _backOffice.executeReadQuery($"SELECT DISTINCT rs.fld_Veza FROM tbl_Robno_Nalozi rn RIGHT JOIN tbl_Robno_Stavke rs ON rn.ident = rs.fld_Veza WHERE rn.ident IS NULL AND rs.fld_Veza IS NOT NULL AND rn.fld_DatNaloga < '{date}'", 300);// AND (rs.Lokacija={location} OR rs.fld_OrgJedinicaUlaz={location} OR rs.fld_OrgJedinicaIzlaz={location})", 300);
            missingData.Columns.AddRange([
                new DataColumn("2", typeof(string)),
                new DataColumn("3", typeof(string)),
                new DataColumn("4", typeof(string)),
                new DataColumn("5", typeof(string)),
                new DataColumn("6", typeof(string)),
                new DataColumn("7", typeof(string)),
                new DataColumn("8", typeof(string))
                ]);
            for (int i = 0; i < missingData.Rows.Count; ++i)
                missingData.Rows[i][1] = missingData.Rows[i][2] = missingData.Rows[i][3] = missingData.Rows[i][4] = missingData.Rows[i][5] = missingData.Rows[i][6] = missingData.Rows[i][7] = "";
            return missingData;
        }
    }
}
