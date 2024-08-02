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
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace Fructa_Database_Comparation
{
    internal class Phase3Scan
    {
        private int _threads;
        private DatabaseManager _databaseManager;
        private string _documentsSql;
        private string _excludeIdentsSql;
        private int _timeout;
        public Phase3Scan(Database backOffice, int threads, string documentsSql, string excludeIdentsSql, int timeout)
        {
            _timeout = timeout;
            _backOffice = backOffice;
            _threads = threads;
            _databaseManager = DatabaseManager.GetDatabaseManager();
            _documentsSql = documentsSql;
            _excludeIdentsSql = excludeIdentsSql;
        }
        private Database _backOffice;

        public DataTable CheckBackOffice()
        {
            string databaseName = _databaseManager.databases.FirstOrDefault(x => x.Value.address == _backOffice.address).Key;
            Database headOffice = _databaseManager.headOffice;
            ChecksumComparator checksumComparator = new ChecksumComparator(_threads);

            string location = _databaseManager.locationIds[databaseName];

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            DataTable identsHeadTable = headOffice.executeReadQuery($"SELECT rs.ident, CHECKSUM(CONCAT(rs.ident, rs.fld_KolicinaIzlaz, rs.fld_KolicinaUlaz)) FROM tbl_Robno_Stavke rs INNER JOIN tbl_Robno_Nalozi rn ON rn.ident = rs.fld_Veza WHERE (rn.Lokacija = {location} OR rn.fld_OrgJedinicaUlaz LIKE '0{location}%' OR rn.fld_OrgJedinicaUlaz LIKE '{location}%' OR rn.fld_OrgJedinicaIzlaz LIKE '0{location}%' OR rn.fld_OrgJedinicaIzlaz LIKE '{location}%') AND rn.fld_GenOznaka in ({_documentsSql}) AND rn.fld_DatNaloga < '{date}'", _timeout);
            DataTable identsTable = _backOffice.executeReadQuery($"SELECT rs.ident, CHECKSUM(CONCAT(rs.ident, rs.fld_KolicinaIzlaz, rs.fld_KolicinaUlaz)) FROM tbl_Robno_Stavke rs INNER JOIN tbl_Robno_Nalozi rn ON rn.ident = rs.fld_Veza WHERE rn.fld_GenOznaka in ({_documentsSql}) AND rn.fld_DatNaloga < '{date}'", _timeout);
            if (Properties.Settings.Default.cancelScan) return new DataTable();

            List<Object> identsHead = new List<Object>(identsHeadTable.Rows.Count);
            for (int i = 0; i < identsHeadTable.Rows.Count; ++i) identsHead.Add(identsHeadTable.Rows[i][0]);

            List<Object> idents = new List<Object>(identsTable.Rows.Count);
            for (int i = 0; i < identsTable.Rows.Count; ++i) idents.Add(identsTable.Rows[i][0]);


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

            string missingDataSqlList = "NULL";
            int index;
            if (missingDataIdents.Count > 0)
            {
                index = missingDataIdents[0];
                StringBuilder stringBuilder = new StringBuilder($"'{hashToIdentMap[index]}'", idents.Count * hashToIdentMap[index].Length);
                for (int j = 1; j < missingDataIdents.Count; ++j) stringBuilder.Append($", '{hashToIdentMap[missingDataIdents[j]]}'");
                missingDataSqlList = stringBuilder.ToString();
            }

            Dictionary<int, string> checksumToIdentMap = new Dictionary<int, string>();
            List<int> checksumHead = new List<int>(identsHeadTable.Rows.Count);
            for (int i = 0; i < identsHeadTable.Rows.Count; ++i) checksumHead.Add(int.Parse(identsHeadTable.Rows[i][1].ToString()));

            List<int> checksum = new List<int>(identsTable.Rows.Count);
            for (int i = 0; i < identsTable.Rows.Count; ++i)
            {
                checksum.Add(int.Parse(identsTable.Rows[i][1].ToString()));
                checksumToIdentMap[checksum[i]] = identsTable.Rows[i][0].ToString();
            }

            ChecksumComparator missingDataComparationInt = new ChecksumComparator(_threads);
            List<int> unequalList = missingDataComparationInt.Compare(checksumHead, checksum);

            List<int> unequalListNormalized = new List<int>();
            for(int i = 0; i < unequalList.Count; ++i)
            {
                int normalized = Checksum.Get(checksumToIdentMap[unequalList[i]]);
                unequalListNormalized.Add(normalized);
                hashToIdentMap[normalized] = checksumToIdentMap[unequalList[i]];
            }

            ChecksumComparator missingDataComparationIntFilter = new ChecksumComparator(_threads);

            List<int> unequalListFiltered = missingDataComparationInt.Compare(missingDataIdents, unequalListNormalized);

            string checksumSqlList = "NULL";
            if (unequalListFiltered.Count > 0)
            {
                index = unequalListFiltered[0];
                StringBuilder stringBuilder = new StringBuilder($"'{hashToIdentMap[index]}'", idents.Count * hashToIdentMap[index].Length);
                for (int j = 1; j < unequalListFiltered.Count; ++j) stringBuilder.Append($", '{hashToIdentMap[unequalListFiltered[j]]}'");
                checksumSqlList = stringBuilder.ToString();
            }

            DataTable missingDataBackOffice = _backOffice.executeReadQuery($"SELECT DISTINCT rn.ident, rn.fld_GenOznaka, rn.fld_OpisNalogaUlaz, rn.fld_OpisNalogaIzlaz, rn.fld_StatusNaloga, rn.fld_OrgJedinicaUlaz, rn.fld_OrgJedinicaIzlaz, rn.fld_Naziv, rn.fld_DatNaloga, rn.fld_DatDokumenta, rn.sys_ChangeTime FROM tbl_Robno_Nalozi as rn INNER JOIN tbl_Robno_Stavke as rs ON rs.fld_Veza=rn.ident WHERE rs.ident in ({missingDataSqlList + ", " + checksumSqlList}) AND rn.ident NOT in ({_excludeIdentsSql})", 600);

            missingDataBackOffice.Columns.Add("ErrorLocation");
            for (int i = 0; i < missingDataBackOffice.Rows.Count; ++i)
                missingDataBackOffice.Rows[i]["ErrorLocation"] = "Centrala";

            missingDataIdents = checksumComparator.Compare(identsHash, identsHeadHash);

            missingDataSqlList = "NULL";
            if (missingDataIdents.Count > 0)
            {
                index = missingDataIdents[0];
                StringBuilder stringBuilder = new StringBuilder($"'{hashToIdentMap[index]}'", idents.Count * hashToIdentMap[index].Length);
                for (int j = 1; j < missingDataIdents.Count; ++j) stringBuilder.Append($", '{hashToIdentMap[missingDataIdents[j]]}'");
                missingDataSqlList = stringBuilder.ToString();
            }

            DataTable missingDataBackOffice2  = headOffice.executeReadQuery($"SELECT DISTINCT rn.ident, rn.fld_GenOznaka, rn.fld_OpisNalogaUlaz, rn.fld_OpisNalogaIzlaz, rn.fld_StatusNaloga, rn.fld_OrgJedinicaUlaz, rn.fld_OrgJedinicaIzlaz, rn.fld_Naziv, rn.fld_DatNaloga, rn.fld_DatDokumenta, rn.sys_ChangeTime FROM tbl_Robno_Nalozi as rn INNER JOIN tbl_Robno_Stavke as rs ON rs.fld_Veza=rn.ident WHERE rs.ident in ({missingDataSqlList}) AND rn.ident NOT in ({_excludeIdentsSql})", 600);
            missingDataBackOffice2.Columns.Add("ErrorLocation");
            for (int i = 0; i < missingDataBackOffice2.Rows.Count; ++i)
                missingDataBackOffice2.Rows[i]["ErrorLocation"] = "Lokal";
            missingDataBackOffice.Merge(missingDataBackOffice2);
            return missingDataBackOffice;
        }


    }
}
