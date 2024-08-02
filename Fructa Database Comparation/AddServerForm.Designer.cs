namespace Fructa_Database_Comparation
{
    partial class AddServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            textEditName = new DevExpress.XtraEditors.TextEdit();
            textEditIp = new DevExpress.XtraEditors.TextEdit();
            textEditDatabase = new DevExpress.XtraEditors.TextEdit();
            textEditPassword = new DevExpress.XtraEditors.TextEdit();
            labelControl2 = new DevExpress.XtraEditors.LabelControl();
            labelControl3 = new DevExpress.XtraEditors.LabelControl();
            labelControl4 = new DevExpress.XtraEditors.LabelControl();
            buttonAdd = new FructaButton();
            textEditUser = new DevExpress.XtraEditors.TextEdit();
            labelControl5 = new DevExpress.XtraEditors.LabelControl();
            textEditCode = new DevExpress.XtraEditors.TextEdit();
            labelControl6 = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)textEditName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEditIp.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEditDatabase.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEditPassword.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEditUser.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)textEditCode.Properties).BeginInit();
            SuspendLayout();
            // 
            // labelControl1
            // 
            labelControl1.Location = new Point(41, 57);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new Size(70, 13);
            labelControl1.TabIndex = 0;
            labelControl1.Text = "Naziv servera:";
            // 
            // textEditName
            // 
            textEditName.Location = new Point(117, 50);
            textEditName.Name = "textEditName";
            textEditName.Size = new Size(232, 28);
            textEditName.TabIndex = 1;
            // 
            // textEditIp
            // 
            textEditIp.Location = new Point(117, 84);
            textEditIp.Name = "textEditIp";
            textEditIp.Size = new Size(232, 28);
            textEditIp.TabIndex = 2;
            // 
            // textEditDatabase
            // 
            textEditDatabase.Location = new Point(117, 118);
            textEditDatabase.Name = "textEditDatabase";
            textEditDatabase.Size = new Size(232, 28);
            textEditDatabase.TabIndex = 3;
            // 
            // textEditPassword
            // 
            textEditPassword.Location = new Point(117, 186);
            textEditPassword.Name = "textEditPassword";
            textEditPassword.Size = new Size(232, 28);
            textEditPassword.TabIndex = 4;
            // 
            // labelControl2
            // 
            labelControl2.Location = new Point(61, 91);
            labelControl2.Name = "labelControl2";
            labelControl2.Size = new Size(50, 13);
            labelControl2.TabIndex = 5;
            labelControl2.Text = "IP adresa:";
            // 
            // labelControl3
            // 
            labelControl3.Location = new Point(36, 125);
            labelControl3.Name = "labelControl3";
            labelControl3.Size = new Size(75, 13);
            labelControl3.TabIndex = 6;
            labelControl3.Text = "Baza podataka:";
            // 
            // labelControl4
            // 
            labelControl4.Location = new Point(85, 193);
            labelControl4.Name = "labelControl4";
            labelControl4.Size = new Size(26, 13);
            labelControl4.TabIndex = 7;
            labelControl4.Text = "Šifra:";
            // 
            // buttonAdd
            // 
            buttonAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAdd.BackColor = Color.FromArgb(214, 228, 243);
            buttonAdd.FlatAppearance.BorderSize = 0;
            buttonAdd.FlatStyle = FlatStyle.Flat;
            buttonAdd.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonAdd.ImageAlign = ContentAlignment.MiddleRight;
            buttonAdd.Location = new Point(242, 237);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(125, 35);
            buttonAdd.TabIndex = 13;
            buttonAdd.Text = "Dodaj";
            buttonAdd.UseVisualStyleBackColor = false;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // textEditUser
            // 
            textEditUser.Location = new Point(117, 152);
            textEditUser.Name = "textEditUser";
            textEditUser.Size = new Size(232, 28);
            textEditUser.TabIndex = 14;
            // 
            // labelControl5
            // 
            labelControl5.Location = new Point(71, 159);
            labelControl5.Name = "labelControl5";
            labelControl5.Size = new Size(40, 13);
            labelControl5.TabIndex = 15;
            labelControl5.Text = "Korisnik:";
            // 
            // textEditCode
            // 
            textEditCode.Location = new Point(117, 16);
            textEditCode.Name = "textEditCode";
            textEditCode.Size = new Size(232, 28);
            textEditCode.TabIndex = 16;
            // 
            // labelControl6
            // 
            labelControl6.Location = new Point(41, 23);
            labelControl6.Name = "labelControl6";
            labelControl6.Size = new Size(66, 13);
            labelControl6.TabIndex = 17;
            labelControl6.Text = "Šifra servera:";
            // 
            // AddServerForm
            // 
            Appearance.BackColor = SystemColors.ActiveCaption;
            Appearance.Options.UseBackColor = true;
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(379, 284);
            Controls.Add(labelControl6);
            Controls.Add(textEditCode);
            Controls.Add(labelControl5);
            Controls.Add(textEditUser);
            Controls.Add(buttonAdd);
            Controls.Add(labelControl4);
            Controls.Add(labelControl3);
            Controls.Add(labelControl2);
            Controls.Add(textEditPassword);
            Controls.Add(textEditDatabase);
            Controls.Add(textEditIp);
            Controls.Add(textEditName);
            Controls.Add(labelControl1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "AddServerForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dodaj server";
            ((System.ComponentModel.ISupportInitialize)textEditName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEditIp.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEditDatabase.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEditPassword.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEditUser.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)textEditCode.Properties).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.TextEdit textEditName;
        private DevExpress.XtraEditors.TextEdit textEditIp;
        private DevExpress.XtraEditors.TextEdit textEditDatabase;
        private DevExpress.XtraEditors.TextEdit textEditPassword;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private FructaButton buttonAdd;
        private DevExpress.XtraEditors.TextEdit textEditUser;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.TextEdit textEditCode;
        private DevExpress.XtraEditors.LabelControl labelControl6;
    }
}