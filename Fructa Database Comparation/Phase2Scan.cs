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
    internal class Phase2Scan
    {
        private int _threads;
        private DatabaseManager _databaseManager;
        private Database _backOffice;
        private string _documentsSql;
        public Phase2Scan(Database backOffice, int threads, string documentsSql)
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

            DataTable missingData = headOffice.executeReadQuery($"SELECT DISTINCT rn.ident, rn.fld_GenOznaka, rn.fld_OpisNalogaUlaz, rn.fld_OpisNalogaIzlaz, rn.fld_StatusNaloga, rn.fld_OrgJedinicaUlaz, rn.fld_OrgJedinicaIzlaz, rn.fld_Naziv, rn.fld_DatNaloga, rn.fld_DatDokumenta, rn.sys_ChangeTime FROM tbl_Robno_Nalozi rn LEFT JOIN tbl_Robno_Stavke rs ON rn.ident = rs.fld_Veza WHERE rs.fld_Veza IS NULL AND (rn.Lokacija = {location} OR rn.fld_OrgJedinicaUlaz LIKE '0{location}%' OR rn.fld_OrgJedinicaUlaz LIKE '{location}%' OR rn.fld_OrgJedinicaIzlaz LIKE '0{location}%' OR rn.fld_OrgJedinicaIzlaz LIKE '{location}%') AND rn.fld_GenOznaka in ({_documentsSql}) AND rn.fld_DatNaloga < '{date}'", 300);
            return missingData;
        }
    }
}
