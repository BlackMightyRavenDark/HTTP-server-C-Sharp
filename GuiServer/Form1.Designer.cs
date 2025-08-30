namespace GuiServer
{
	partial class Form1
	{
		/// <summary>
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.btnStartServer = new System.Windows.Forms.Button();
			this.btnStopServer = new System.Windows.Forms.Button();
			this.labelPort = new System.Windows.Forms.Label();
			this.numericUpDownServerPort = new System.Windows.Forms.NumericUpDown();
			this.labelLog = new System.Windows.Forms.Label();
			this.listBoxLog = new System.Windows.Forms.ListBox();
			this.checkBoxAutoscroll = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnBrowsePublicDirectory = new System.Windows.Forms.Button();
			this.textBoxPublicDirectory = new System.Windows.Forms.TextBox();
			this.checkBoxAutostart = new System.Windows.Forms.CheckBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageServer = new System.Windows.Forms.TabPage();
			this.tabPageSettings = new System.Windows.Forms.TabPage();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPageServer.SuspendLayout();
			this.tabPageSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnStartServer
			// 
			this.btnStartServer.Location = new System.Drawing.Point(7, 6);
			this.btnStartServer.Name = "btnStartServer";
			this.btnStartServer.Size = new System.Drawing.Size(75, 23);
			this.btnStartServer.TabIndex = 0;
			this.btnStartServer.Text = "Start";
			this.btnStartServer.UseVisualStyleBackColor = true;
			this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
			// 
			// btnStopServer
			// 
			this.btnStopServer.Enabled = false;
			this.btnStopServer.Location = new System.Drawing.Point(88, 6);
			this.btnStopServer.Name = "btnStopServer";
			this.btnStopServer.Size = new System.Drawing.Size(75, 23);
			this.btnStopServer.TabIndex = 1;
			this.btnStopServer.Text = "Stop";
			this.btnStopServer.UseVisualStyleBackColor = true;
			this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
			// 
			// labelPort
			// 
			this.labelPort.AutoSize = true;
			this.labelPort.Location = new System.Drawing.Point(10, 9);
			this.labelPort.Name = "labelPort";
			this.labelPort.Size = new System.Drawing.Size(80, 13);
			this.labelPort.TabIndex = 2;
			this.labelPort.Text = "Порт сервера:";
			// 
			// numericUpDownServerPort
			// 
			this.numericUpDownServerPort.Location = new System.Drawing.Point(96, 5);
			this.numericUpDownServerPort.Maximum = new decimal(new int[] {
			65000,
			0,
			0,
			0});
			this.numericUpDownServerPort.Minimum = new decimal(new int[] {
			1025,
			0,
			0,
			0});
			this.numericUpDownServerPort.Name = "numericUpDownServerPort";
			this.numericUpDownServerPort.Size = new System.Drawing.Size(61, 20);
			this.numericUpDownServerPort.TabIndex = 3;
			this.numericUpDownServerPort.Value = new decimal(new int[] {
			5555,
			0,
			0,
			0});
			this.numericUpDownServerPort.ValueChanged += new System.EventHandler(this.numericUpDownServerPort_ValueChanged);
			// 
			// labelLog
			// 
			this.labelLog.AutoSize = true;
			this.labelLog.Location = new System.Drawing.Point(7, 32);
			this.labelLog.Name = "labelLog";
			this.labelLog.Size = new System.Drawing.Size(75, 13);
			this.labelLog.TabIndex = 4;
			this.labelLog.Text = "Лог событий:";
			// 
			// listBoxLog
			// 
			this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxLog.BackColor = System.Drawing.Color.Black;
			this.listBoxLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.listBoxLog.ForeColor = System.Drawing.Color.Lime;
			this.listBoxLog.FormattingEnabled = true;
			this.listBoxLog.HorizontalScrollbar = true;
			this.listBoxLog.ItemHeight = 20;
			this.listBoxLog.Location = new System.Drawing.Point(7, 48);
			this.listBoxLog.Name = "listBoxLog";
			this.listBoxLog.Size = new System.Drawing.Size(571, 244);
			this.listBoxLog.TabIndex = 5;
			// 
			// checkBoxAutoscroll
			// 
			this.checkBoxAutoscroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAutoscroll.AutoSize = true;
			this.checkBoxAutoscroll.Checked = true;
			this.checkBoxAutoscroll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxAutoscroll.Location = new System.Drawing.Point(476, 295);
			this.checkBoxAutoscroll.Name = "checkBoxAutoscroll";
			this.checkBoxAutoscroll.Size = new System.Drawing.Size(102, 17);
			this.checkBoxAutoscroll.TabIndex = 6;
			this.checkBoxAutoscroll.Text = "Автопрокрутка";
			this.checkBoxAutoscroll.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.btnBrowsePublicDirectory);
			this.groupBox1.Controls.Add(this.textBoxPublicDirectory);
			this.groupBox1.Location = new System.Drawing.Point(13, 31);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(565, 67);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Папка для общего доступа";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(483, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Внимание! Всё содержимое этой папки станет доступно всем в локальной сети и интер" +
	"нете!";
			// 
			// btnBrowsePublicDirectory
			// 
			this.btnBrowsePublicDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowsePublicDirectory.Location = new System.Drawing.Point(516, 17);
			this.btnBrowsePublicDirectory.Name = "btnBrowsePublicDirectory";
			this.btnBrowsePublicDirectory.Size = new System.Drawing.Size(43, 23);
			this.btnBrowsePublicDirectory.TabIndex = 1;
			this.btnBrowsePublicDirectory.Text = "...";
			this.btnBrowsePublicDirectory.UseVisualStyleBackColor = true;
			this.btnBrowsePublicDirectory.Click += new System.EventHandler(this.btnBrowsePublicDirectory_Click);
			// 
			// textBoxPublicDirectory
			// 
			this.textBoxPublicDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxPublicDirectory.Location = new System.Drawing.Point(7, 19);
			this.textBoxPublicDirectory.Name = "textBoxPublicDirectory";
			this.textBoxPublicDirectory.Size = new System.Drawing.Size(503, 20);
			this.textBoxPublicDirectory.TabIndex = 0;
			this.textBoxPublicDirectory.TextChanged += new System.EventHandler(this.textBoxPublicDirectory_TextChanged);
			// 
			// checkBoxAutostart
			// 
			this.checkBoxAutostart.AutoSize = true;
			this.checkBoxAutostart.Location = new System.Drawing.Point(167, 8);
			this.checkBoxAutostart.Name = "checkBoxAutostart";
			this.checkBoxAutostart.Size = new System.Drawing.Size(85, 17);
			this.checkBoxAutostart.TabIndex = 8;
			this.checkBoxAutostart.Text = "Автозапуск";
			this.toolTip1.SetToolTip(this.checkBoxAutostart, "Автоматически запускать сервер вместе с программой");
			this.checkBoxAutostart.UseVisualStyleBackColor = true;
			this.checkBoxAutostart.CheckedChanged += new System.EventHandler(this.checkBoxAutostart_CheckedChanged);
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPageServer);
			this.tabControl1.Controls.Add(this.tabPageSettings);
			this.tabControl1.Location = new System.Drawing.Point(4, 4);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(597, 344);
			this.tabControl1.TabIndex = 9;
			// 
			// tabPageServer
			// 
			this.tabPageServer.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.tabPageServer.Controls.Add(this.listBoxLog);
			this.tabPageServer.Controls.Add(this.btnStopServer);
			this.tabPageServer.Controls.Add(this.btnStartServer);
			this.tabPageServer.Controls.Add(this.labelLog);
			this.tabPageServer.Controls.Add(this.checkBoxAutoscroll);
			this.tabPageServer.Location = new System.Drawing.Point(4, 22);
			this.tabPageServer.Name = "tabPageServer";
			this.tabPageServer.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageServer.Size = new System.Drawing.Size(589, 318);
			this.tabPageServer.TabIndex = 0;
			this.tabPageServer.Text = "Сервер";
			// 
			// tabPageSettings
			// 
			this.tabPageSettings.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.tabPageSettings.Controls.Add(this.labelPort);
			this.tabPageSettings.Controls.Add(this.checkBoxAutostart);
			this.tabPageSettings.Controls.Add(this.numericUpDownServerPort);
			this.tabPageSettings.Controls.Add(this.groupBox1);
			this.tabPageSettings.Location = new System.Drawing.Point(4, 22);
			this.tabPageSettings.Name = "tabPageSettings";
			this.tabPageSettings.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSettings.Size = new System.Drawing.Size(589, 318);
			this.tabPageSettings.TabIndex = 1;
			this.tabPageSettings.Text = "Настройки";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(598, 351);
			this.Controls.Add(this.tabControl1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(614, 350);
			this.Name = "Form1";
			this.Text = "HTTP server";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabPageServer.ResumeLayout(false);
			this.tabPageServer.PerformLayout();
			this.tabPageSettings.ResumeLayout(false);
			this.tabPageSettings.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnStartServer;
		private System.Windows.Forms.Button btnStopServer;
		private System.Windows.Forms.Label labelPort;
		private System.Windows.Forms.NumericUpDown numericUpDownServerPort;
		private System.Windows.Forms.Label labelLog;
		private System.Windows.Forms.ListBox listBoxLog;
		private System.Windows.Forms.CheckBox checkBoxAutoscroll;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnBrowsePublicDirectory;
		private System.Windows.Forms.TextBox textBoxPublicDirectory;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBoxAutostart;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPageServer;
		private System.Windows.Forms.TabPage tabPageSettings;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}

