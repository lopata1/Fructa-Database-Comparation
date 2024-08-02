using DevExpress.CodeParser;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraExport.Helpers;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using DevExpress.XtraTab.ViewInfo;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraDiagram.Bars;
using System.Diagnostics;
using DevExpress.XtraReports.UserDesigner;
using DevExpress.Pdf.ContentGeneration.Interop;
using DevExpress.XtraRichEdit.Fields;
using System.Xml.Linq;
using DevExpress.DataAccess.Excel;
using System.Collections;
using DevExpress.XtraReports.UI;
using DevExpress.Mvvm.Native;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Fructa_Database_Comparation
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        private Point _buttonSettingsLocationDiff, _buttonSettingsLocation;

        private int _panelDropServersHeight;
        private int _menuButtonHeight;

        private DataTable _backOfficesDataTable = new DataTable();
        private DataTable _headOfficesDataTable = new DataTable();
        private DataTable _missingDocumentsDataTable = new DataTable();
        private DataTable _documentsDataTable = new DataTable();
        private DataTable _historyDataTable = new DataTable();
        private DataTable _serverProgressDataTable = new DataTable();

        private DatabaseManager _databaseManager;

        private string _key;
        private const string _csvFilesPath = "C:\\Fructa Database Comparation";

        private bool _close = false;
        private int _nextBackOfficeServerNumber = 1;
        private int _nextHeadOfficeServerNumber = 1;
        public MainForm()
        {
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show("Program je već pokrenut", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            InitializeComponent();
            _databaseManager = DatabaseManager.GetDatabaseManager();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            _panelDropServersHeight = panelDropCoders.Height;
            _menuButtonHeight = buttonCoders.Height;
            MaximizedBounds = Screen.PrimaryScreen.WorkingArea;

            _buttonSettingsLocation = buttonSettings.Location;
            _buttonSettingsLocationDiff = new Point(buttonSettings.Location.X, buttonSettings.Location.Y - 204);

            CodersMenuSet(0);

            _backOfficesDataTable.Columns.AddRange([
                new DataColumn("Number", typeof(string)),
                new DataColumn("Name", typeof(string)),
                new DataColumn("IP", typeof(string)),
                new DataColumn("Database", typeof(string)),
                new DataColumn("User", typeof(string)),
                new DataColumn("Password", typeof(string)),
                new DataColumn("Code", typeof(string))
            ]);
            gridControlBackOffices.DataSource = _backOfficesDataTable;

            _missingDocumentsDataTable.Columns.AddRange([
                new DataColumn("Ident", typeof(string)),
                new DataColumn("GenOznaka", typeof(string)),
                new DataColumn("Error", typeof(string)),
                new DataColumn("ServerCode", typeof(string)),
                new DataColumn("ServerName", typeof(string)),
                new DataColumn("IPAdress", typeof(string)),
                new DataColumn("DocumentName", typeof(string)),
                new DataColumn("DocumentType", typeof(string)),
                new DataColumn("DocumentTypeName", typeof(string)),
                new DataColumn("DocumentNumber", typeof(string)),
                new DataColumn("DocumentStatus", typeof(string)),
                new DataColumn("StorageCode", typeof(string)),
                new DataColumn("StorageName", typeof(string)),
                new DataColumn("ErrorLocation", typeof(string)),
                new DataColumn("Date", typeof(string)),
                new DataColumn("DocumentDate", typeof(string)),
                new DataColumn("ChangeDate", typeof(string))
            ]);

            gridControlMissingDocuments.DataSource = _missingDocumentsDataTable;

            _serverProgressDataTable.Columns.AddRange([
                new DataColumn("Error", typeof(string)),
                new DataColumn("ServerCode", typeof(string)),
                new DataColumn("ServerName", typeof(string)),
                new DataColumn("IPAdress", typeof(string)),
                new DataColumn("ProgressHide", typeof(string)),
                new DataColumn("Status2", typeof(string)),
            ]);

            gridControlServerProgress.DataSource = _serverProgressDataTable;


            _historyDataTable.Columns.AddRange([
                new DataColumn("Date", typeof(string)),
                new DataColumn("StartTime", typeof(string)),
                new DataColumn("Duration", typeof(string)),
                new DataColumn("Time", typeof(string)),
                new DataColumn("Filename", typeof(string)),
                new DataColumn("Status2", typeof(string))
            ]);

            gridControlHistory.DataSource = _historyDataTable;

            _headOfficesDataTable = _backOfficesDataTable.Clone();
            gridControlHeadOffices.DataSource = _headOfficesDataTable;

            if (Properties.Settings.Default.headOfficesDataTable == "")
            {
                // Initial servers here
            }
            else
            {
                while (true)
                {
                    if (PromptPassword()) break;
                }
            }

            ReadCsvFiles();
            LoadPreviousState();

            timerSchedule.Start();
        }

        public bool PromptPassword()
        {
            try
            {
                _key = Interaction.InputBox("Potreban je ključ za dekripciju podataka", "Unesite ključ", "");

                if (_key == "")
                {
                    _close = true;
                    Close();
                }

                Aes256 aes256 = new Aes256(Encoding.ASCII.GetBytes(_key));
                string encryptedHeadOffices = Properties.Settings.Default.headOfficesDataTable;
                string decryptedHeadOffices = aes256.Decrypt(System.Convert.FromBase64String(encryptedHeadOffices));
                _headOfficesDataTable = JsonConvert.DeserializeObject<DataTable>(decryptedHeadOffices);

                string encryptedBackOffices = Properties.Settings.Default.backOfficesDataTable;
                string decryptedBackOffices = aes256.Decrypt(System.Convert.FromBase64String(encryptedBackOffices));
                _backOfficesDataTable = JsonConvert.DeserializeObject<DataTable>(decryptedBackOffices);

                for (int i = 0; i < _backOfficesDataTable.Rows.Count; i++)
                {
                    _databaseManager.Add((string)_backOfficesDataTable.Rows[i][1],
                                         (string)_backOfficesDataTable.Rows[i][2],
                                         (string)_backOfficesDataTable.Rows[i][3],
                                         (string)_backOfficesDataTable.Rows[i][4],
                                         (string)_backOfficesDataTable.Rows[i][5],
                                         (string)_backOfficesDataTable.Rows[i][6]);
                }

                gridControlHeadOffices.DataSource = _headOfficesDataTable;
                gridControlBackOffices.DataSource = _backOfficesDataTable;
                return true;
            }
            catch
            {
                MessageBox.Show("Netačan ključ", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void LoadPreviousState()
        {

            _documentsDataTable = JsonConvert.DeserializeObject<DataTable>(Properties.Settings.Default.documentsDataTable);

            gridControlDocuments.DataSource = _documentsDataTable;

            if (Properties.Settings.Default.documentsChecked != "")
            {
                int[] documentsChecked = JsonConvert.DeserializeObject<int[]>(Properties.Settings.Default.documentsChecked);
                for (int i = 0; i < documentsChecked.Length; i++) gridViewDocuments.SelectRow(gridViewDocuments.GetRowHandle(documentsChecked[i]));
            }

            if (Properties.Settings.Default.backOfficesChecked != "")
            {

                int[] backOfficesChecked = JsonConvert.DeserializeObject<int[]>(Properties.Settings.Default.backOfficesChecked);
                for (int i = 0; i < backOfficesChecked.Length; i++) gridViewBackOffices.SelectRow(gridViewBackOffices.GetRowHandle(backOfficesChecked[i]));
            }

            if (Properties.Settings.Default.headOfficesChecked != "")
            {
                int[] headOfficesChecked = JsonConvert.DeserializeObject<int[]>(Properties.Settings.Default.headOfficesChecked);
                for (int i = 0; i < headOfficesChecked.Length; i++) gridViewHeadOffices.SelectRow(gridViewHeadOffices.GetRowHandle(headOfficesChecked[i]));
            }

            textEditThreads.Text = Properties.Settings.Default.threads.ToString();
            checkEditSchedule.Checked = Properties.Settings.Default.scheduleChecked;
            timeEditSchedule.Text = Properties.Settings.Default.schedule;

            textEditServerGroup.Text = Properties.Settings.Default.serverGroup.ToString();
            textEditTimeout.Text = Properties.Settings.Default.timeout.ToString();

            gridViewBackOffices.BestFitColumns();
            gridViewHeadOffices.BestFitColumns();
            gridViewDocuments.BestFitColumns();
        }

        private void ReadCsvFiles()
        {
            if (!Directory.Exists(_csvFilesPath)) return;
            _historyDataTable.Rows.Clear();
            string[] files = Directory.GetFiles(_csvFilesPath);
            files = files.Reverse().ToArray();
            for (int i = 0; i < files.Length; ++i)
            {
                if (!files[i].EndsWith(".csv") || files[i].EndsWith("servers.csv")) continue;
                DateTime creation = File.GetCreationTime(files[i]);

                string startDate = "", duration = "", status = "success";

                try
                {
                    using (StreamReader outputFile = new StreamReader(Path.Combine(_csvFilesPath + "\\", files[i].Replace(".csv", "-meta.txt")), true))
                    {
                        startDate = outputFile.ReadLine();
                        duration = outputFile.ReadLine();
                        status = outputFile.ReadLine();
                    }
                }
                catch
                {
                    status = "error";
                }


                string date = creation.Date.ToString("dd.MM.yyyy");
                string time = creation.ToString("HH:mm:ss");

                _historyDataTable.Rows.Add(date, startDate, duration, time, files[i], status);
            }
            gridViewHistory.BestFitColumns();
        }

        private float _serversMenuAnimationPercent = 0;
        private bool _serversMenuOpened = false;
        private void CodersMenuSet(float percent)
        {
            float scale = (100 - percent) / 100 * -1;
            panelDropCoders.Size = new Size(panelDropCoders.Width, (int)(_panelDropServersHeight * percent / 100));

            Point settingsDiff = ScalePoints(_buttonSettingsLocationDiff, scale);
            buttonSettings.Location = AddPoints(_buttonSettingsLocation, settingsDiff);
        }

        private Point AddPoints(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        private Point ScalePoints(Point p, float scale)
        {
            return new Point((int)(p.X * scale), (int)(p.Y * scale));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (_close) Close();
        }

        private void buttonServers_Click(object sender, EventArgs e)
        {
            timerAnimation.Start();
        }

        private void timerAnimation_Tick(object sender, EventArgs e)
        {
            float sine = (float)Math.Sin(_serversMenuAnimationPercent / 100 * 3 + 0.1f);
            _serversMenuAnimationPercent += _serversMenuOpened ? -10f * sine : 10f * sine;
            _serversMenuAnimationPercent = Math.Min(100, Math.Max(0, _serversMenuAnimationPercent));
            if (_serversMenuOpened && _serversMenuAnimationPercent == 0 ||
                !_serversMenuOpened && _serversMenuAnimationPercent == 100)
            {
                timerAnimation.Stop();
                _serversMenuOpened = !_serversMenuOpened;
            }
            CodersMenuSet(_serversMenuAnimationPercent);
        }

        private bool mouseDown;
        private Point lastLocation;
        private void panelControlTop_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void panelControlTop_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void panelControlTop_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            notifyIcon.Visible = true;
        }

        private void buttonMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
                Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
                sidePanel2.Width = 400;
                return;
            }
            Region = null;
            WindowState = FormWindowState.Maximized;
            sidePanel2.Width = 900;

        }

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void buttonDashboard_Click(object sender, EventArgs e)
        {
            xtraTabPageDashboard.PageVisible = true;
            xtraTabControlNavigation.SelectedTabPage = xtraTabPageDashboard;
        }

        private void buttonHeadOffice_Click(object sender, EventArgs e)
        {
            xtraTabPageHeadOffice.PageVisible = true;
            xtraTabPageBackOffice.PageVisible = true;
            xtraTabControlNavigation.SelectedTabPage = xtraTabPageHeadOffice;
        }

        private void xtraTabControlNavigation_CloseButtonClick(object sender, EventArgs e)
        {
            ClosePageButtonEventArgs arg = e as ClosePageButtonEventArgs;
            (arg.Page as XtraTabPage).PageVisible = false;
        }

        private void buttonAddBackOfficeServer_Click(object sender, EventArgs e)
        {
            AddServerForm addServerForm = new AddServerForm();
            addServerForm.ShowDialog();
            if (addServerForm.cancelled) return;

            _backOfficesDataTable.Rows.Add(_nextBackOfficeServerNumber++.ToString(), addServerForm.name, addServerForm.ipAddress, addServerForm.database, addServerForm.user, addServerForm.password, addServerForm.code);


            _databaseManager.Add(addServerForm.name, addServerForm.ipAddress, addServerForm.database, addServerForm.user, addServerForm.password, addServerForm.code);
        }

        private void tabNavigationPageServer_Paint(object sender, PaintEventArgs e)
        {

        }
        private Task _comparationTask;
        private const int _robnoNaloziRowSize = 1963;
        private const int _robnoStavkeRowSize = 879;
        private void buttonComparation_Click(object sender, EventArgs e)
        {
            if (_cancel)
            {
                MessageBox.Show("Druga komparacija je u toku otkazivanja", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (timerEstimate.Enabled)
            {
                MessageBox.Show("Druga komparacija je u toku", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_databaseManager.headOffice == null)
            {
                MessageBox.Show("Niste ozačili Head Office server.", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _missingDocumentsDataTable.Rows.Clear();
            panelControlLoading.Visible = true;
            _serverProgressDataTable.Rows.Clear();

            int[] backOfficesSelected = gridViewBackOffices.GetSelectedRows();

            if (backOfficesSelected.Length == 0)
            {
                MessageBox.Show("Niste ozačili Back Office servere.", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Database[] backOffices = new Database[backOfficesSelected.Length];
            for (int i = 0; i < backOfficesSelected.Length; i++)
            {
                string databaseName = gridViewBackOffices.GetDataRow(backOfficesSelected[i])[1].ToString();
                backOffices[i] = _databaseManager.databases[databaseName];
                _serverProgressDataTable.Rows.Add("", backOffices[i].code, databaseName, backOffices[i].address, 0);
                for (int j = 0; j < _backOfficesDataTable.Rows.Count; ++j)
                {
                    if ((string)_backOfficesDataTable.Rows[j]["Name"] == databaseName)
                    {
                        _databaseManager.locationIds[databaseName] = (string)_backOfficesDataTable.Rows[j]["Code"];
                        break;
                    }

                }

            }

            Threadify.threadCount = int.Parse(textEditThreads.Text);
            timerEstimate.Start();

            try
            {
                for (int i = 0; i < _threads.Length; ++i)
                {
                    if (_threads[i] == null) continue;
                    _threads[i].Abort();
                }
            }
            catch
            {

            }
            progressBarControlScan.EditValue = 0;
            _comparationTask = new Task(() => { Compare(backOffices); });
            _comparationTask.Start();
        }

        private string FormatEstimate(int estimate)
        {
            if (estimate < 0) return "Procjena: 00:00";
            int minutes = estimate / 60;
            int seconds = estimate % 60;
            return String.Format("Procjena: {0:D2}:{1:D2}", minutes, seconds);
        }

        private string GetDocumentsSql()
        {
            int[] documentsSelected = gridViewDocuments.GetSelectedRows();
            if (documentsSelected.Length == 0) return "";
            string document = gridViewDocuments.GetDataRow(documentsSelected[0])[0].ToString();
            string res = $"'{document}'";
            for (int i = 0; i < documentsSelected.Count(); i++)
            {
                document = gridViewDocuments.GetDataRow(documentsSelected[i])[0].ToString();
                res += $", '{document}'";
            }
            return res;
        }

        private int _estimate = 0;
        Thread[] _threads = new Thread[0];

        private void InsertMissingData(DataRow missingData, Database server, string error, string errorLocation, bool check = true)
        {

            string serverName = "";
            for (int i = 0; i < _databaseManager.databases.Count; ++i)
            {
                (serverName, Database s) = _databaseManager.databases.ElementAt(i);
                if (server.address == s.address) break;
            }
            string organizationUnit = "";
            string storageName = "";
            string documentId = "";
            string name = "";
            string type = "";
            string nameType = "";

            if (check)
            {
                documentId = (string)missingData[1];
                DataRow documentRow = _documentsDataTable.Rows[0];
                for (int k = 0; k < _documentsDataTable.Rows.Count; ++k)
                {
                    if ((string)_documentsDataTable.Rows[k]["ID"] == documentId)
                    {
                        documentRow = _documentsDataTable.Rows[k];
                        break;
                    }
                }
                name = (string)documentRow["Name"];
                type = (string)documentRow["Type"];
                nameType = (string)documentRow["NameType"];

                organizationUnit = (string)((string)documentRow["NameType"] == "Ulazni" ? missingData[5] : missingData[6]);
                List<Object> storageNameList = _databaseManager.headOffice.executeOneAttributeReadQuery($"SELECT fld_Naziv from tbl_Org_Jedinice WHERE fld_Sifra='{organizationUnit}'");
                if (storageNameList.Count > 0) storageName = (string)storageNameList[0];
            }

            string dateChange = DateTime.Parse((string)missingData[10]).ToString("dd.MM.yyyy");

            if (nameType == "Ulazni")
            {
                _missingDocumentsDataTable.Rows.Add(missingData[0],
                                                    missingData[1],
                                                    error,
                                                    server.code,
                                                    serverName,
                                                    server.address,
                                                    name,
                                                    type,
                                                    nameType,
                                                    missingData[2],
                                                    (string)missingData[4] == "K" ? "Proknjižen" : "Neproknjižen",
                                                    organizationUnit,
                                                    storageName,
                                                    errorLocation,
                                                    DateTime.Parse((string)missingData[8]).ToString("dd.MM.yyyy"),
                                                    DateTime.Parse((string)missingData[9]).ToString("dd.MM.yyyy"),
                                                    dateChange);
            }
            else if (nameType == "Izlazni")
            {
                _missingDocumentsDataTable.Rows.Add(missingData[0],
                                                    missingData[1],
                                                    error,
                                                    server.code,
                                                    serverName,
                                                    server.address,
                                                    name,
                                                    type,
                                                    nameType,
                                                    missingData[3],
                                                    (string)missingData[4] == "K" ? "Proknjižen" : "Neproknjižen",
                                                    organizationUnit,
                                                    storageName,
                                                    errorLocation,
                                                    DateTime.Parse((string)missingData[8]).ToString("dd.MM.yyyy"),
                                                    DateTime.Parse((string)missingData[9]).ToString("dd.MM.yyyy"),
                                                    dateChange);
            }
            else
            {
                _missingDocumentsDataTable.Rows.Add(missingData[0],
                                                    missingData[1],
                                                    error,
                                                    server.code,
                                                    serverName,
                                                    server.address,
                                                    name,
                                                    type,
                                                    nameType,
                                                    "",
                                                    "",
                                                    organizationUnit,
                                                    storageName,
                                                    errorLocation,
                                                    DateTime.Parse((string)missingData[8]).ToString("dd.MM.yyyy"),
                                                    DateTime.Parse((string)missingData[9]).ToString("dd.MM.yyyy"),
                                                    dateChange);
            }
        }
        private List<int> _serverProgresses = new List<int>();
        private DateTime _startDate;
        private Stopwatch _sw;
        private int _errorCount;
        private float _progressBarValue = 0;
        private void Compare(Database[] backOffices)
        {

            int threadsNumber = int.Parse(textEditThreads.Text);

            string documentsSql = GetDocumentsSql();
            if (documentsSql == "")
            {
                MessageBox.Show("Niste ozačili naloge.", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _sw = new Stopwatch();
            _sw.Start();

            _startDate = DateTime.Now;


            Invoke(() =>
            {
                xtraTabPageDocumentsMissing.PageVisible = true;
                xtraTabControlNavigation.SelectedTabPage = xtraTabPageDocumentsMissing;
                progressBarControlScan.Increment(1);
            });


            bool error = false;
            _threads = new Thread[backOffices.Length];
            Database headOffice = _databaseManager.headOffice;
            int headOfficeChecksumNalozi = int.Parse((string)headOffice.executeReadQuery("SELECT BINARY_CHECKSUM((SELECT ident FROM tbl_Robno_Nalozi ORDER BY ident FOR XML RAW))", 10000).Rows[0][0]);

            _errorCount = 0;

            if (_cancel)
            {
                _cancel = false;
                _estimate = 0;
                return;
            }

            for (int i = 0; i < backOffices.Length; ++i)
            {


                Database backOffice = (Database)backOffices[i].Clone();
                int backOfficeIndex = new int();
                backOfficeIndex = i;
                bool isPartial = false;
                if (_cancel) return;
                Thread t = new Thread(() =>
                {
                    string databaseName = _databaseManager.databases.FirstOrDefault(x => x.Value.address == backOffice.address).Key;
                    try
                    {

                        if (!_databaseManager.locationIds.ContainsKey(databaseName))
                        {
                            _databaseManager.locationIds[databaseName] = (string)backOffice.executeOneAttributeReadQuery("SELECT TOP 1 Lokacija FROM tbl_Robno_Nalozi")[0];
                        }
                        DataTable? missingDataNalozi = null;
                        Phase1Scan phase1Scan = new Phase1Scan(backOffice, threadsNumber, documentsSql);

                        missingDataNalozi = phase1Scan.CheckBackOffice();
                        for (int j = 0; j < missingDataNalozi.Rows.Count; ++j)
                        {
                            Invoke(() =>
                            {
                                InsertMissingData(missingDataNalozi.Rows[j], backOffice, "ERROR - nalog nedostaje", (string)missingDataNalozi.Rows[j]["ErrorLocation"]);
                            });
                        }

                        Invoke(() =>
                        {
                            gridView4.BestFitColumns();
                            _serverProgressDataTable.Rows[backOfficeIndex][4] = 25;
                            _progressBarValue += 100 / (4 * backOffices.Length);
                            progressBarControlScan.EditValue = (int)_progressBarValue;
                        });
                        isPartial = true;

                        Phase2Scan phase2Scan = new Phase2Scan(backOffice, threadsNumber, documentsSql);
                        DataTable missingData = phase2Scan.CheckBackOffice();
                        for (int j = 0; j < missingData.Rows.Count; ++j)
                            Invoke(() =>
                            {
                                InsertMissingData(missingData.Rows[j], backOffice, "ERROR - stavke nedostaju", "Centrala");
                            });

                        Invoke(() =>
                        {
                            _serverProgressDataTable.Rows[backOfficeIndex][4] = 50;
                            _progressBarValue += 100 / (4 * backOffices.Length);
                            progressBarControlScan.EditValue = (int)_progressBarValue;
                        });
                        missingDataNalozi.Merge(missingData);
                        string excludeIdentsSql = "NULL";
                        if (missingDataNalozi != null && missingDataNalozi.Rows.Count > 0)
                        {

                            excludeIdentsSql = $"'{missingDataNalozi.Rows[0][0]}'";
                            for (int j = 1; j < missingDataNalozi.Rows.Count; ++j)
                            {
                                excludeIdentsSql += $", '{missingDataNalozi.Rows[j][0]}'";
                            }
                        }

                        Phase3Scan phase3Scan = new Phase3Scan(backOffice, threadsNumber, documentsSql, excludeIdentsSql, int.Parse(textEditTimeout.Text));
                        missingData = phase3Scan.CheckBackOffice();

                        for (int j = 0; j < missingData.Rows.Count; ++j)
                            Invoke(() =>
                            {
                                InsertMissingData(missingData.Rows[j], backOffice, "ERROR - saldo stavki", (string)missingData.Rows[j]["ErrorLocation"]);//"ERROR - " + missingData.Rows[j][missingData.Columns.Count - 1]);
                            });

                        Invoke(() =>
                        {
                            _serverProgressDataTable.Rows[backOfficeIndex][4] = 75;
                            _progressBarValue += 100 / (4 * backOffices.Length);
                            progressBarControlScan.EditValue = (int)_progressBarValue;
                        });

                        Phase4Scan phase4Scan = new Phase4Scan(backOffice, threadsNumber, documentsSql);
                        missingData = phase4Scan.CheckBackOffice();

                        for (int j = 0; j < missingData.Rows.Count; ++j)
                            Invoke(() =>
                            {
                                InsertMissingData(missingData.Rows[j], backOffice, "ERROR - ima stavke, nema nalog", "Lokal", false);//"ERROR - " + missingData.Rows[j][missingData.Columns.Count - 1]);
                            });

                        Invoke(() =>
                        {
                            _serverProgressDataTable.Rows[backOfficeIndex][4] = 100;
                            _progressBarValue += 100 / (4 * backOffices.Length);
                            progressBarControlScan.EditValue = (int)_progressBarValue;
                            _serverProgressDataTable.Rows[backOfficeIndex]["Status2"] = "success";
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex.GetType() == typeof(ThreadAbortException) || ex.GetType() == typeof(ThreadInterruptedException)) return;
                        Invoke(() =>
                        {
                            ++_errorCount;
                            var st = new StackTrace(ex, true);
                            var frame = st.GetFrame(0);
                            var line = frame.GetFileLineNumber();
                            _serverProgressDataTable.Rows[backOfficeIndex]["Error"] = ex.Message + ", file: " + frame.GetFileName() + ", line: " + line.ToString();
                            _serverProgressDataTable.Rows[backOfficeIndex]["Status2"] = isPartial ? "partial" : "error";
                        });
                    }
                });

                t.Start();
                _threads[i] = t;
                int serverGroup = int.Parse(textEditServerGroup.Text);
                if ((i + 1) % serverGroup == 0)
                {
                    for (int j = ((i + 1) / serverGroup - 1) * 10; j < Math.Min(_threads.Length, (i + 1) / serverGroup * 10); ++j)
                    {
                        if (_threads[j] == null)
                        {
                            continue;
                        }
                        _threads[j].Join();
                    }
                }
            }

            for (int j = 0; j < _threads.Length; ++j)
            {
                if (_threads[j] == null) return;
                _threads[j].Join();
            }

            if (_cancel)
            {
                _cancel = false;
                _estimate = 0;
                return;
            }

            _threads = new Thread[0];
            _sw.Stop();

            Invoke(FinishScan);

            if (error)
            {
                MessageBox.Show("Komparacija je nije uspješno završena", "Obavještenje", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Komparacija je uspješno završena", "Obavještenje", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FinishScan()
        {
            Invoke(() =>
            {
                panelControlLoading.Visible = false;
                Directory.CreateDirectory(_csvFilesPath);
                timerEstimate.Stop();
                progressBarControlScan.EditValue = 0;
                _estimate = 0;
            });

            string filename = DateTime.Now.ToString("yyyy-dd-MM-HH-mm-ss");

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(_csvFilesPath + "\\", filename + "-meta.txt"), true))
            {
                outputFile.WriteLine(_startDate.ToString("HH:mm:ss"));
                outputFile.WriteLine(String.Format("{0:D2}:{1:D2}:{2:D2}", _sw.Elapsed.Hours, _sw.Elapsed.Minutes, _sw.Elapsed.Seconds));
                string status = "success";
                if (_errorCount != 0 && _errorCount < _databaseManager.databases.Count) status = "partial";
                if (_errorCount == _databaseManager.databases.Count) status = "error";
                outputFile.WriteLine(status);
            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(_csvFilesPath + "\\", filename + ".csv"), true))
            {

                for (int i = 0; i < _missingDocumentsDataTable.Rows.Count; ++i)
                {
                    DataRow row = _missingDocumentsDataTable.Rows[i];
                    string output = $"{_missingDocumentsDataTable.Rows[i][0]}";
                    for (int j = 1; j < _missingDocumentsDataTable.Columns.Count; ++j)
                    {
                        output += $", {_missingDocumentsDataTable.Rows[i][j]}";
                    }
                    outputFile.WriteLine(output);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(_csvFilesPath + "\\", filename + "-servers.csv"), true))
            {

                for (int i = 0; i < _serverProgressDataTable.Rows.Count; ++i)
                {
                    DataRow row = _serverProgressDataTable.Rows[i];
                    string output = $"{_serverProgressDataTable.Rows[i][0]}";
                    for (int j = 1; j < _serverProgressDataTable.Columns.Count; ++j)
                    {
                        output += $";{_serverProgressDataTable.Rows[i][j]}";
                    }
                    outputFile.WriteLine(output);
                }
            }

            Invoke(ReadCsvFiles);

            _cancel = false;
            timerEstimate.Stop();
            _estimate = 0;
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            xtraTabPageSettings.PageVisible = true;
            xtraTabControlNavigation.SelectedTabPage = xtraTabPageSettings;
        }

        private void buttonAddHeadOffice_Click(object sender, EventArgs e)
        {
            AddServerForm addServerForm = new AddServerForm();
            addServerForm.ShowDialog();
            if (addServerForm.cancelled) return;

            _headOfficesDataTable.Rows.Add(_nextHeadOfficeServerNumber++.ToString(), addServerForm.name, addServerForm.ipAddress, addServerForm.database, addServerForm.user, addServerForm.password, addServerForm.code);
            _databaseManager.Add(addServerForm.name, addServerForm.ipAddress, addServerForm.database, addServerForm.user, addServerForm.password, addServerForm.code);
        }

        private void buttonDocuments_Click(object sender, EventArgs e)
        {
            xtraTabPageDocuments.PageVisible = true;
            xtraTabControlNavigation.SelectedTabPage = xtraTabPageDocuments;
        }

        bool lockSelection = false;
        private int _selectedHeadOffice = -1;
        private void gridViewHeadOffices_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            if (lockSelection) return;
            _selectedHeadOffice = e.ControllerRow;
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            int[] selectedRows = view.GetSelectedRows();
            lockSelection = true;
            foreach (int selectedRow in selectedRows)
            {
                if (selectedRow != e.ControllerRow)
                {
                    view.UnselectRow(selectedRow);
                }
                else
                {
                    DataRowView row = (DataRowView)view.GetRow(selectedRow);
                    _databaseManager.headOffice = new Database(row[2] as string, row[3] as string, row[4] as string, row[5] as string, row[6] as string);
                }
            }
            lockSelection = false;
        }

        private void gridViewHeadOffices_SelectionChanging(object sender, DevExpress.Data.SelectionChangingEventArgs e)
        {
            if (e.ControllerRow == _selectedHeadOffice) e.Cancel = true;
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Visible = true;
                notifyIcon.Visible = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                notifyIcon.ContextMenuStrip.Show();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.backOfficesDataTable == "")
            {
                while (true)
                {
                    _key = Interaction.InputBox("Potreban je ključ za enkripciju podataka", "Unesite ključ", "");
                    if (_key != "") break;
                    MessageBox.Show("Ključ ne može biti prazan", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            Aes256 aes256 = new Aes256(Encoding.ASCII.GetBytes(_key));
            string serializedHeadOffices = JsonConvert.SerializeObject(_headOfficesDataTable);
            Properties.Settings.Default.headOfficesDataTable = System.Convert.ToBase64String(aes256.Encrypt(serializedHeadOffices));

            string serializedBackOffices = JsonConvert.SerializeObject(_backOfficesDataTable);
            Properties.Settings.Default.backOfficesDataTable = System.Convert.ToBase64String(aes256.Encrypt(serializedBackOffices));

            Properties.Settings.Default.documentsDataTable = JsonConvert.SerializeObject(_documentsDataTable);

            int[] documentsChecked = gridViewDocuments.GetSelectedRows();
            for (int i = 0; i < documentsChecked.Length; ++i)
            {
                documentsChecked[i] = gridViewDocuments.GetDataSourceRowIndex(documentsChecked[i]);
            }
            Properties.Settings.Default.documentsChecked = JsonConvert.SerializeObject(documentsChecked);

            int[] backOfficesChecked = gridViewBackOffices.GetSelectedRows();
            for (int i = 0; i < backOfficesChecked.Length; ++i)
            {
                backOfficesChecked[i] = gridViewBackOffices.GetDataSourceRowIndex(backOfficesChecked[i]);
            }
            Properties.Settings.Default.backOfficesChecked = JsonConvert.SerializeObject(backOfficesChecked);

            int[] headOfficesChecked = gridViewHeadOffices.GetSelectedRows();
            for (int i = 0; i < headOfficesChecked.Length; ++i)
            {
                headOfficesChecked[i] = gridViewHeadOffices.GetDataSourceRowIndex(headOfficesChecked[i]);
            }
            Properties.Settings.Default.headOfficesChecked = JsonConvert.SerializeObject(headOfficesChecked);

            Properties.Settings.Default.threads = int.Parse(textEditThreads.Text);
            Properties.Settings.Default.serverGroup = int.Parse(textEditServerGroup.Text);
            Properties.Settings.Default.timeout = int.Parse(textEditTimeout.Text);
            Properties.Settings.Default.scheduleChecked = checkEditSchedule.Checked;
            Properties.Settings.Default.schedule = timeEditSchedule.Text;

            Properties.Settings.Default.Save();
            try
            {
                for (int i = 0; i < _threads.Length; ++i)
                    _threads[i].Abort();
            }
            catch
            {

            }

            Environment.Exit(0);
        }

        private void buttonDeleteBackOffice_Click(object sender, EventArgs e)
        {
            int index = gridViewBackOffices.FocusedRowHandle;
            _backOfficesDataTable.Rows.RemoveAt(index);
            _databaseManager.Remove(_databaseManager.databases.ElementAt(index).Key);
        }

        private void buttonEditBackOffice_Click(object sender, EventArgs e)
        {
            EditOffice(_backOfficesDataTable, gridViewBackOffices);
        }

        private void buttonEditHeadOffice_Click(object sender, EventArgs e)
        {
            EditOffice(_headOfficesDataTable, gridViewHeadOffices);
        }

        private void EditOffice(DataTable office, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            int index = grid.FocusedRowHandle;
            DataRow row = grid.GetDataRow(index);

            AddServerForm addServerForm = new AddServerForm();
            string oldName = (string)row[1];
            addServerForm.name = (string)row[1];
            addServerForm.ipAddress = (string)row[2];
            addServerForm.database = (string)row[3];
            addServerForm.user = (string)row[4];
            addServerForm.password = (string)row[5];
            addServerForm.code = (string)row[6];
            addServerForm.SwitchToEdit();

            addServerForm.ShowDialog();
            if (addServerForm.cancelled) return;

            row.BeginEdit();
            row.ItemArray = [row[0].ToString(), addServerForm.name, addServerForm.ipAddress, addServerForm.database, addServerForm.user, addServerForm.password, addServerForm.code];

            row.EndEdit();
            if (office == _backOfficesDataTable)
            {
                _databaseManager.Remove(oldName);
                _databaseManager.Add(addServerForm.name, addServerForm.ipAddress, addServerForm.database, addServerForm.user, addServerForm.password, addServerForm.code);
            }
        }

        private void buttonDeleteHeadOffice_Click(object sender, EventArgs e)
        {
            int index = gridViewHeadOffices.FocusedRowHandle;
            _headOfficesDataTable.Rows.RemoveAt(index);
        }

        private void buttonDocumentsSync_Click(object sender, EventArgs e)
        {
            if (_databaseManager.headOffice == null)
            {
                MessageBox.Show("Head Office nije označen.", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Database documentsDatabase = (Database)_databaseManager.headOffice.Clone();
            documentsDatabase.databaseName = "A2D2sdb-Baza";


            try
            {
                _documentsDataTable = documentsDatabase.executeReadQuery($"SELECT fld_Sifra, fld_Naziv, fld_TipNaloga FROM [{documentsDatabase.databaseName}].dbo.tbl_NaloziRN");
                _documentsDataTable.Columns[0].ColumnName = "ID";
                _documentsDataTable.Columns[1].ColumnName = "Name";
                _documentsDataTable.Columns[2].ColumnName = "Type";
                _documentsDataTable.Columns.Add("NameType");

                string[] enter = { "PRD-KONS", "PRD-MPP", "PRD-MSK", "PRD-CAR", "PRD-VP-MPK" };
                string[] exit = { "PRD-MP-MPK", "PRD-MP-VPK", "PRD-VP-VPK" };

                for (int i = 0; i < _documentsDataTable.Rows.Count; ++i)
                {
                    switch (_documentsDataTable.Rows[i]["Type"])
                    {
                        case "I":
                            _documentsDataTable.Rows[i]["NameType"] = "Izlazni";
                            continue;
                        case "K":
                            if (enter.Contains(_documentsDataTable.Rows[i]["ID"])) _documentsDataTable.Rows[i]["NameType"] = "Izlazni";
                            if (exit.Contains(_documentsDataTable.Rows[i]["ID"])) _documentsDataTable.Rows[i]["NameType"] = "Ulazni";
                            continue;
                        case "U":
                        case "N":
                            _documentsDataTable.Rows[i]["NameType"] = "Ulazni";
                            continue;
                    }
                    _documentsDataTable.Rows[i]["NameType"] = "Ostalo";
                }

                gridControlDocuments.DataSource = _documentsDataTable;

                gridViewDocuments.BestFitColumns();
                MessageBox.Show("Nalozi uspješno sinhronizovani.", "Operacija uspješna", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void buttonCheckBackOffices_Click(object sender, EventArgs e)
        {
            CheckServers(gridViewBackOffices, _backOfficesDataTable);
        }

        Dictionary<string, bool> _connectionMap = new Dictionary<string, bool>();

        private void CheckServers(DevExpress.XtraGrid.Views.Grid.GridView gridView, DataTable office)
        {
            _connectionMap.Clear();
            gridView.RefreshData();
            int[] checkedServers = gridView.GetSelectedRows();
            for (int i = 0; i < checkedServers.Length; i++)
            {
                int index = new int();
                index = checkedServers[i];
                Thread t = new Thread(() =>
                {
                    Database database;
                    string databaseId = (string)gridView.GetDataRow(index)[1];
                    if (office == _backOfficesDataTable)
                    {
                        database = _databaseManager.databases[databaseId];
                    }
                    else
                    {
                        database = _databaseManager.headOffice;
                    }
                    try
                    {
                        database.executeReadQuery("SELECT TOP 1 ident FROM tbl_Robno_Nalozi");
                        Invoke(() =>
                        {
                            _connectionMap[databaseId] = true;
                            gridView.RefreshRow(index);
                        });

                    }
                    catch (Exception ex)
                    {
                        Invoke(() =>
                        {
                            _connectionMap[databaseId] = false;
                            MessageBox.Show(ex.Message, "Greška pri povezivanju sa bazom " + database.databaseName);
                        });
                    }
                });
                t.Start();
            }
        }

        private void buttonCheckHeadOffices_Click(object sender, EventArgs e)
        {
            CheckServers(gridViewHeadOffices, _headOfficesDataTable);
        }

        private void gridViewHistory_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs ea = e as DXMouseEventArgs;
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.InRow || info.InRowCell)
            {
                xtraTabPageDocumentsMissing.PageVisible = true;
                xtraTabControlNavigation.SelectedTabPage = xtraTabPageDocumentsMissing;
                panelControlLoading.Visible = false;

                string[] files = Directory.GetFiles(_csvFilesPath);
                files = files.Where(file => file.EndsWith(".csv")).ToArray();

                DataRow row = view.GetDataRow(info.RowHandle);

                StreamReader sr = new StreamReader(row[4].ToString());
                _missingDocumentsDataTable.Rows.Clear();
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] cols = line.Split(",");

                    _missingDocumentsDataTable.Rows.Add(cols);
                    line = sr.ReadLine();
                }
                gridView4.BestFitColumns();

                _serverProgressDataTable.Rows.Clear();

                using (StreamReader outputFile = new StreamReader(Path.Combine(_csvFilesPath + "\\", ((string)row[4]).Replace(".csv", "-servers.csv")), true))
                {
                    line = outputFile.ReadLine();
                    while (line != null)
                    {
                        string[] cols = line.Split(";");
                        _serverProgressDataTable.Rows.Add(cols);

                        _serverProgressDataTable.Rows[_serverProgressDataTable.Rows.Count - 1]["Status2"] = "success";
                        if (int.Parse((string)_serverProgressDataTable.Rows[_serverProgressDataTable.Rows.Count - 1]["ProgressHide"]) < 100) _serverProgressDataTable.Rows[_serverProgressDataTable.Rows.Count - 1]["Status2"] = "partial";
                        if (int.Parse((string)_serverProgressDataTable.Rows[_serverProgressDataTable.Rows.Count - 1]["ProgressHide"]) == 0) _serverProgressDataTable.Rows[_serverProgressDataTable.Rows.Count - 1]["Status2"] = "error";
                        line = outputFile.ReadLine();
                    }
                }
            }
        }

        private bool _scheduleCompared = false;
        private void timerSchedule_Tick(object sender, EventArgs e)
        {
            if (!checkEditSchedule.Checked) return;
            DateTime dateTime = DateTime.Parse(timeEditSchedule.Text);
            DateTime now = DateTime.Now;
            if (now > dateTime && now < dateTime.AddMinutes(1))
            {
                if (_scheduleCompared) return;
                _scheduleCompared = true;
                buttonComparation_Click(null, null);
            }
            else
            {
                _scheduleCompared = false;
            }

        }

        private void timeEditSchedule_EditValueChanged(object sender, EventArgs e)
        {
            _scheduleCompared = false;
        }
        private bool _cancel = false;
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _cancel = true;
            for (int i = 0; i < _threads.Length; ++i)
                try
                {
                    if (_threads[i] == null) continue;
                    _threads[i].Abort();
                }
                catch
                {

                }
            panelControlLoading.Visible = false;
            _errorCount = 1;
            FinishScan();
            MessageBox.Show("Skeniranje uspješno otkazano", "Obavještenje", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void timerEstimate_Tick(object sender, EventArgs e)
        {
            labelControlEstimate.Text = FormatEstimate(--_estimate);
        }

        private void gridViewHistory_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            var gridView = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (e.Column.Caption == "Status komparacije")
            {
                e.DefaultDraw();
                string status = gridView.GetDataRow(e.RowHandle)["Status2"].ToString();

                Bitmap image = Properties.Resources.greenCircle;
                if (status == "partial") image = Properties.Resources.yellowCircle;
                if (status == "error") image = Properties.Resources.redCircle;
                e.Cache.DrawImage(image, e.Bounds.X + (e.Bounds.Width - image.Width) / 2, e.Bounds.Y + (e.Bounds.Height - image.Height) / 2);
            }
            else if (e.Column.Caption == "Status ažuriranja")
            {
                e.DefaultDraw();
                Bitmap image = Properties.Resources.redCircle;
                e.Cache.DrawImage(image, e.Bounds.X + (e.Bounds.Width - image.Width) / 2, e.Bounds.Y + (e.Bounds.Height - image.Height) / 2);
            }
        }

        private void ConnectionCellDraw(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            var gridView = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            string databaseName = ((DataRowView)gridView.GetRow(e.RowHandle))[1].ToString();
            if (e.Column.Caption == "Konekcija" && _connectionMap.ContainsKey(databaseName))
            {
                e.DefaultDraw();
                Bitmap image = _connectionMap[databaseName] ? Properties.Resources.greenCircle : Properties.Resources.redCircle;
                e.Cache.DrawImage(image, e.Bounds.X + (e.Bounds.Width - image.Width) / 2, e.Bounds.Y + (e.Bounds.Height - image.Height) / 2);
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            Close();
        }

        private void gridViewServerProgress_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            int progress = int.Parse((string)gridViewServerProgress.GetDataRow(e.RowHandle)[4]);
            bool[] phases = [progress > 0, progress > 25, progress > 50, progress > 75];
            Dictionary<string, int> phaseIndexMap = new Dictionary<string, int>();
            phaseIndexMap["Progress1"] = 0;
            phaseIndexMap["Progress2"] = 1;
            phaseIndexMap["Progress3"] = 2;
            phaseIndexMap["Progress4"] = 3;
            if (e.Column.FieldName == "Progress1" || e.Column.FieldName == "Progress2" || e.Column.FieldName == "Progress3" || e.Column.FieldName == "Progress4")
            {
                var rect = e.Bounds;
                rect.Width = rect.Width * (phases[phaseIndexMap[e.Column.FieldName]] ? 1 : 0);
                e.Cache.FillRectangle(Color.Blue, rect);
                e.Appearance.DrawString(e.Cache, e.DisplayText, e.Bounds);
                e.Handled = true;
            }
            var gridView = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (e.Column.Caption == "Status")
            {
                e.DefaultDraw();
                string status = gridView.GetDataRow(e.RowHandle)["Status2"].ToString();
                Bitmap? image = null;
                if (status == "success") image = Properties.Resources.greenCircle;
                if (status == "partial") image = Properties.Resources.yellowCircle;
                if (status == "error") image = Properties.Resources.redCircle;
                if (image == null) return;
                e.Cache.DrawImage(image, e.Bounds.X + (e.Bounds.Width - image.Width) / 2, e.Bounds.Y + (e.Bounds.Height - image.Height) / 2);
            }
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        private void gridViewHistory_CustomColumnSort(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnSortEventArgs e)
        {
            if (e.Column.FieldName == "Date")
            {
                DateTime val1 = Convert.ToDateTime(e.Value1);
                DateTime val2 = Convert.ToDateTime(e.Value2);
                e.Result = Comparer.Default.Compare(val1, val2);
                e.Handled = true;
            }

        }
    }
}