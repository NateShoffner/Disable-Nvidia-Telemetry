namespace DisableNvidiaTelemetry.Forms
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.chkFileLogging = new System.Windows.Forms.CheckBox();
            this.cbTaskTrigger = new System.Windows.Forms.ComboBox();
            this.btnUpdatecheck = new System.Windows.Forms.Button();
            this.chkUpdates = new System.Windows.Forms.CheckBox();
            this.chkBackgroundTask = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lblVersion = new System.Windows.Forms.LinkLabel();
            this.pbDonate = new System.Windows.Forms.PictureBox();
            this.lblGithub = new System.Windows.Forms.LinkLabel();
            this.pbGithub = new System.Windows.Forms.PictureBox();
            this.lblCopyright = new System.Windows.Forms.LinkLabel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtLicense = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnDefaults = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDonate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGithub)).BeginInit();
            this.tabControl2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(561, 242);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(553, 216);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Telemetry";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(553, 216);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Event Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(547, 210);
            this.textBox1.TabIndex = 0;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.chkFileLogging);
            this.tabPage6.Controls.Add(this.cbTaskTrigger);
            this.tabPage6.Controls.Add(this.btnUpdatecheck);
            this.tabPage6.Controls.Add(this.chkUpdates);
            this.tabPage6.Controls.Add(this.chkBackgroundTask);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(553, 216);
            this.tabPage6.TabIndex = 3;
            this.tabPage6.Text = "Settings";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // chkFileLogging
            // 
            this.chkFileLogging.AutoSize = true;
            this.chkFileLogging.Location = new System.Drawing.Point(6, 15);
            this.chkFileLogging.Name = "chkFileLogging";
            this.chkFileLogging.Size = new System.Drawing.Size(96, 17);
            this.chkFileLogging.TabIndex = 6;
            this.chkFileLogging.Text = "Enable logging";
            this.chkFileLogging.UseVisualStyleBackColor = true;
            this.chkFileLogging.CheckedChanged += new System.EventHandler(this.chkFileLogging_CheckedChanged);
            // 
            // cbTaskTrigger
            // 
            this.cbTaskTrigger.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTaskTrigger.Enabled = false;
            this.cbTaskTrigger.FormattingEnabled = true;
            this.cbTaskTrigger.Items.AddRange(new object[] {
            "Every Day",
            "After Windows Startup"});
            this.cbTaskTrigger.Location = new System.Drawing.Point(358, 36);
            this.cbTaskTrigger.Name = "cbTaskTrigger";
            this.cbTaskTrigger.Size = new System.Drawing.Size(166, 21);
            this.cbTaskTrigger.TabIndex = 5;
            this.cbTaskTrigger.SelectedIndexChanged += new System.EventHandler(this.cbTaskTrigger_SelectedIndexChanged);
            // 
            // btnUpdatecheck
            // 
            this.btnUpdatecheck.Location = new System.Drawing.Point(6, 84);
            this.btnUpdatecheck.Name = "btnUpdatecheck";
            this.btnUpdatecheck.Size = new System.Drawing.Size(127, 23);
            this.btnUpdatecheck.TabIndex = 4;
            this.btnUpdatecheck.Text = "Check for Updates";
            this.btnUpdatecheck.UseVisualStyleBackColor = true;
            this.btnUpdatecheck.Click += new System.EventHandler(this.btnUpdatecheck_Click);
            // 
            // chkUpdates
            // 
            this.chkUpdates.AutoSize = true;
            this.chkUpdates.Location = new System.Drawing.Point(6, 61);
            this.chkUpdates.Name = "chkUpdates";
            this.chkUpdates.Size = new System.Drawing.Size(232, 17);
            this.chkUpdates.TabIndex = 2;
            this.chkUpdates.Text = "Check for updates when program is opened";
            this.chkUpdates.UseVisualStyleBackColor = true;
            this.chkUpdates.CheckedChanged += new System.EventHandler(this.chkUpdates_CheckedChanged);
            // 
            // chkBackgroundTask
            // 
            this.chkBackgroundTask.AutoSize = true;
            this.chkBackgroundTask.Location = new System.Drawing.Point(6, 38);
            this.chkBackgroundTask.Name = "chkBackgroundTask";
            this.chkBackgroundTask.Size = new System.Drawing.Size(346, 17);
            this.chkBackgroundTask.TabIndex = 1;
            this.chkBackgroundTask.Text = "Run in background and disable Nvidia telemetry services and tasks:";
            this.chkBackgroundTask.UseVisualStyleBackColor = true;
            this.chkBackgroundTask.CheckedChanged += new System.EventHandler(this.chkBackroundTask_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lblVersion);
            this.tabPage3.Controls.Add(this.pbDonate);
            this.tabPage3.Controls.Add(this.lblGithub);
            this.tabPage3.Controls.Add(this.pbGithub);
            this.tabPage3.Controls.Add(this.lblCopyright);
            this.tabPage3.Controls.Add(this.tabControl2);
            this.tabPage3.Controls.Add(this.pictureBox1);
            this.tabPage3.Controls.Add(this.lblName);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(553, 216);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "About";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.ActiveLinkColor = System.Drawing.Color.Blue;
            this.lblVersion.AutoSize = true;
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblVersion.LinkArea = new System.Windows.Forms.LinkArea(12, 13);
            this.lblVersion.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lblVersion.Location = new System.Drawing.Point(82, 37);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(26, 17);
            this.lblVersion.TabIndex = 81;
            this.lblVersion.Text = "v1.0";
            this.lblVersion.UseCompatibleTextRendering = true;
            this.lblVersion.VisitedLinkColor = System.Drawing.Color.Blue;
            this.lblVersion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblVersion_LinkClicked);
            // 
            // pbDonate
            // 
            this.pbDonate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbDonate.Image = global::DisableNvidiaTelemetry.Properties.Resources.btn_donate_92x26;
            this.pbDonate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pbDonate.Location = new System.Drawing.Point(348, 60);
            this.pbDonate.Name = "pbDonate";
            this.pbDonate.Size = new System.Drawing.Size(92, 26);
            this.pbDonate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbDonate.TabIndex = 80;
            this.pbDonate.TabStop = false;
            this.pbDonate.Click += new System.EventHandler(this.pbDonate_Click);
            // 
            // lblGithub
            // 
            this.lblGithub.ActiveLinkColor = System.Drawing.Color.Blue;
            this.lblGithub.AutoSize = true;
            this.lblGithub.BackColor = System.Drawing.Color.Transparent;
            this.lblGithub.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblGithub.LinkArea = new System.Windows.Forms.LinkArea(0, 14);
            this.lblGithub.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lblGithub.Location = new System.Drawing.Point(464, 73);
            this.lblGithub.Name = "lblGithub";
            this.lblGithub.Size = new System.Drawing.Size(81, 13);
            this.lblGithub.TabIndex = 79;
            this.lblGithub.TabStop = true;
            this.lblGithub.Text = "View on GitHub";
            this.lblGithub.VisitedLinkColor = System.Drawing.Color.Blue;
            this.lblGithub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblGithub_LinkClicked);
            // 
            // pbGithub
            // 
            this.pbGithub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbGithub.Image = global::DisableNvidiaTelemetry.Properties.Resources.GitHub_Mark_64px;
            this.pbGithub.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pbGithub.Location = new System.Drawing.Point(474, 6);
            this.pbGithub.Name = "pbGithub";
            this.pbGithub.Size = new System.Drawing.Size(64, 64);
            this.pbGithub.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbGithub.TabIndex = 78;
            this.pbGithub.TabStop = false;
            this.pbGithub.Click += new System.EventHandler(this.pbGithub_Click);
            // 
            // lblCopyright
            // 
            this.lblCopyright.ActiveLinkColor = System.Drawing.Color.Blue;
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.BackColor = System.Drawing.Color.Transparent;
            this.lblCopyright.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblCopyright.LinkArea = new System.Windows.Forms.LinkArea(12, 13);
            this.lblCopyright.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lblCopyright.Location = new System.Drawing.Point(82, 56);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(165, 17);
            this.lblCopyright.TabIndex = 77;
            this.lblCopyright.TabStop = true;
            this.lblCopyright.Text = "Copyright © Nate Shoffner 2017";
            this.lblCopyright.UseCompatibleTextRendering = true;
            this.lblCopyright.VisitedLinkColor = System.Drawing.Color.Blue;
            this.lblCopyright.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblCopyright_LinkClicked);
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Location = new System.Drawing.Point(6, 76);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(541, 134);
            this.tabControl2.TabIndex = 74;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.txtLicense);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(533, 108);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "License";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtLicense
            // 
            this.txtLicense.BackColor = System.Drawing.SystemColors.Window;
            this.txtLicense.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLicense.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLicense.Location = new System.Drawing.Point(3, 3);
            this.txtLicense.Multiline = true;
            this.txtLicense.Name = "txtLicense";
            this.txtLicense.ReadOnly = true;
            this.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLicense.Size = new System.Drawing.Size(527, 102);
            this.txtLicense.TabIndex = 62;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.textBox2);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(533, 108);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "Credits";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Window;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox2.Location = new System.Drawing.Point(3, 3);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(527, 102);
            this.textBox2.TabIndex = 63;
            this.textBox2.Text = "Icon made by freepik from www.flaticon.com ";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::DisableNvidiaTelemetry.Properties.Resources.binoculars_64;
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(6, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 72;
            this.pictureBox1.TabStop = false;
            // 
            // lblName
            // 
            this.lblName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblName.AutoSize = true;
            this.lblName.BackColor = System.Drawing.Color.Transparent;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lblName.Location = new System.Drawing.Point(76, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(278, 26);
            this.lblName.TabIndex = 69;
            this.lblName.Text = "Disable Nvidia Telemetry";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRefresh.Location = new System.Drawing.Point(12, 272);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Enabled = false;
            this.btnApply.Location = new System.Drawing.Point(456, 272);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(117, 23);
            this.btnApply.TabIndex = 3;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnDefaults
            // 
            this.btnDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDefaults.Location = new System.Drawing.Point(329, 272);
            this.btnDefaults.Name = "btnDefaults";
            this.btnDefaults.Size = new System.Drawing.Size(121, 23);
            this.btnDefaults.TabIndex = 2;
            this.btnDefaults.Text = "Restore Defaults";
            this.btnDefaults.UseVisualStyleBackColor = true;
            this.btnDefaults.Click += new System.EventHandler(this.btnDefaults_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 307);
            this.Controls.Add(this.btnDefaults);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Disable Nvidia Telemetry";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDonate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbGithub)).EndInit();
            this.tabControl2.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnDefaults;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox txtLicense;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.PictureBox pbGithub;
        private System.Windows.Forms.LinkLabel lblGithub;
        private System.Windows.Forms.PictureBox pbDonate;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.CheckBox chkBackgroundTask;
        private System.Windows.Forms.CheckBox chkUpdates;
        private System.Windows.Forms.Button btnUpdatecheck;
        private System.Windows.Forms.ComboBox cbTaskTrigger;
        private System.Windows.Forms.LinkLabel lblVersion;
        private System.Windows.Forms.LinkLabel lblCopyright;
        private System.Windows.Forms.CheckBox chkFileLogging;
    }
}

