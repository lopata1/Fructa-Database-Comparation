using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using DevExpress.DataProcessing.InMemoryDataProcessor;
using System.Numerics;
using DevExpress.Mvvm.Native;
using System.Diagnostics;
using System.Security.Cryptography;
using DevExpress.CodeParser;
using DevExpress.PivotGrid.Internal;
using DevExpress.Diagram.Core.Native;
using DevExpress.DataAccess.Native.ObjectBinding;
using DevExpress.XtraPrinting.Native;

namespace Fructa_Database_Comparation
{
    internal class Phase1Scan
    {
        private DatabaseManager _databaseManager;
        private int _threads;
        private Database _backOffice;
        private string _documentsSql;

        public Phase1Scan(Database backOffice, int threads, string documentsSql)
        {
            _backOffice = backOffice;
            _databaseManager = DatabaseManager.GetDatabaseManager();
            _threads = threads;
            _documentsSql = documentsSql;
        }

        public DataTable CheckBackOffice()
        {
            string databaseName = _databaseManager.databases.FirstOrDefault(x => x.Value.address == _backOffice.address).Key;
            Database headOffice = _databaseManager.headOffice;

            ChecksumComparator checksumComparator = new ChecksumComparator(_threads);

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string location = _databaseManager.locationIds[databaseName];

            List<Object> idents = _backOffice.executeOneAttributeReadQuery($"SELECT ident FROM tbl_Robno_Nalozi WHERE fld_GenOznaka in ({_documentsSql}) AND fld_DatNaloga < '{date}'");
            List<Object> identsHead = headOffice.executeOneAttributeReadQuery($"SELECT ident FROM tbl_Robno_Nalozi WHERE (Lokacija = {location}  OR fld_OrgJedinicaUlaz LIKE '{location}%' OR fld_OrgJedinicaIzlaz LIKE '{location}%') AND fld_GenOznaka in ({_documentsSql}) AND fld_DatNaloga < '{date}'");
            if (Properties.Settings.Default.cancelScan) return new DataTable();

            Dictionary<int, string> hashToIdentMap = new Dictionary<int, string>();

            List<int> identsHash = new List<int>();
            for (int i = 0; i < idents.Count; ++i)
            {
                string ident = idents[i].ToString();
                int hash = Checksum.Get(ident);
                hashToIdentMap[hash] = ident;
                identsHash.Add(hash);
            }

            List<int> identsHeadHash = new List<int>();
            for (int i = 0; i < identsHead.Count; ++i)
            {
                string ident = identsHead[i].ToString();
                int hash = Checksum.Get(ident);
                hashToIdentMap[hash] = ident;
                identsHeadHash.Add(hash);
            }

            List<int> missingDataIdents = checksumComparator.Compare(identsHeadHash, identsHash);
            List<int> missingDataIdents2 = checksumComparator.Compare(identsHash, identsHeadHash);

            if (missingDataIdents.Count == 0) return new DataTable();

            int index = missingDataIdents[0];

            StringBuilder stringBuilder = new StringBuilder($"'{hashToIdentMap[index]}'", missingDataIdents.Count * hashToIdentMap[index].Length);

            for (int j = 1; j < missingDataIdents.Count; ++j) stringBuilder.Append($", '{hashToIdentMap[missingDataIdents[j]]}'");

            string missingDataSqlList = stringBuilder.ToString();

            DataTable missingDataBackOffice = _backOffice.executeReadQuery($"SELECT ident, fld_GenOznaka, fld_OpisNalogaUlaz, fld_OpisNalogaIzlaz, fld_StatusNaloga, fld_OrgJedinicaUlaz, fld_OrgJedinicaIzlaz, fld_Naziv, fld_DatNaloga, fld_DatDokumenta, sys_ChangeTime FROM tbl_Robno_Nalozi WHERE ident in ({missingDataSqlList})");
            missingDataBackOffice.Columns.Add("ErrorLocation");

            for (int i = 0; i < missingDataBackOffice.Rows.Count; ++i)
                missingDataBackOffice.Rows[i]["ErrorLocation"] = "Centrala";

            if (missingDataIdents2.Count == 0) return missingDataBackOffice;

            index = missingDataIdents2[0];
            stringBuilder = new StringBuilder($"'{hashToIdentMap[index]}'", missingDataIdents2.Count * hashToIdentMap[index].Length);

            for (int j = 1; j < missingDataIdents2.Count; ++j) stringBuilder.Append($", '{hashToIdentMap[missingDataIdents2[j]]}'");

            missingDataSqlList = stringBuilder.ToString();

            DataTable missingDataBackOffice2 = headOffice.executeReadQuery($"SELECT ident, fld_GenOznaka, fld_OpisNalogaUlaz, fld_OpisNalogaIzlaz, fld_StatusNaloga, fld_OrgJedinicaUlaz, fld_OrgJedinicaIzlaz, fld_Naziv, fld_DatNaloga, fld_DatDokumenta, sys_ChangeTime FROM tbl_Robno_Nalozi WHERE ident in ({missingDataSqlList})");

            missingDataBackOffice2.Columns.Add("ErrorLocation");
            for (int i = 0; i < missingDataBackOffice2.Rows.Count; ++i)
                missingDataBackOffice2.Rows[i]["ErrorLocation"] = "Lokal";

            missingDataBackOffice.Merge(missingDataBackOffice2);

            return missingDataBackOffice;
        }
    }
}
