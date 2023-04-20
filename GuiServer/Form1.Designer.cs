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
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.labelPort = new System.Windows.Forms.Label();
            this.numericUpDownServerPort = new System.Windows.Forms.NumericUpDown();
            this.labelLog = new System.Windows.Forms.Label();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.checkBoxAutoscroll = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(169, 11);
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
            this.btnStopServer.Location = new System.Drawing.Point(250, 11);
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
            this.labelPort.Location = new System.Drawing.Point(16, 16);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(80, 13);
            this.labelPort.TabIndex = 2;
            this.labelPort.Text = "Порт сервера:";
            // 
            // numericUpDownServerPort
            // 
            this.numericUpDownServerPort.Location = new System.Drawing.Point(102, 12);
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
            // 
            // labelLog
            // 
            this.labelLog.AutoSize = true;
            this.labelLog.Location = new System.Drawing.Point(12, 47);
            this.labelLog.Name = "labelLog";
            this.labelLog.Size = new System.Drawing.Size(73, 13);
            this.labelLog.TabIndex = 4;
            this.labelLog.Text = "Лог собитий:";
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
            this.listBoxLog.ItemHeight = 20;
            this.listBoxLog.Location = new System.Drawing.Point(12, 74);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(574, 164);
            this.listBoxLog.TabIndex = 5;
            // 
            // checkBoxAutoscroll
            // 
            this.checkBoxAutoscroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoscroll.AutoSize = true;
            this.checkBoxAutoscroll.Checked = true;
            this.checkBoxAutoscroll.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoscroll.Location = new System.Drawing.Point(484, 244);
            this.checkBoxAutoscroll.Name = "checkBoxAutoscroll";
            this.checkBoxAutoscroll.Size = new System.Drawing.Size(102, 17);
            this.checkBoxAutoscroll.TabIndex = 6;
            this.checkBoxAutoscroll.Text = "Автопрокрутка";
            this.checkBoxAutoscroll.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 270);
            this.Controls.Add(this.checkBoxAutoscroll);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.labelLog);
            this.Controls.Add(this.numericUpDownServerPort);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.btnStartServer);
            this.Name = "Form1";
            this.Text = "HTTP server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.NumericUpDown numericUpDownServerPort;
        private System.Windows.Forms.Label labelLog;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.CheckBox checkBoxAutoscroll;
    }
}

