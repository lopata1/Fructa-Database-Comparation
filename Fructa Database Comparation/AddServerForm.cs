using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fructa_Database_Comparation
{
    public partial class AddServerForm : DevExpress.XtraEditors.XtraForm
    {
        public AddServerForm()
        {
            InitializeComponent();
        }

        public string name = "";
        public string ipAddress = "";
        public string database = "";
        public string user = "";
        public string password = "";
        public string code = "";
        public bool cancelled = true;

        public void UpdateValues()
        {
            textEditName.Text = name;
            textEditIp.Text = ipAddress;
            textEditDatabase.Text = database;
            textEditUser.Text = user;
            textEditPassword.Text = password;
            textEditCode.Text = code;
        }

        public void SwitchToEdit()
        {
            UpdateValues();
            this.Text = "Izmjeni server";
            buttonAdd.Text = "Izmjeni";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            name = textEditName.Text;
            ipAddress = textEditIp.Text;
            database = textEditDatabase.Text;
            user = textEditUser.Text;
            password = textEditPassword.Text;
            code = textEditCode.Text;
            if(name == "" || ipAddress == "" || database == "" || user == "" || password == "" || code == "")
            {
                MessageBox.Show("Polja ne mogu biti prazna", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            cancelled = false;
            Close();
        }
    }
}