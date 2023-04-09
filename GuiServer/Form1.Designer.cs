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
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownServerPort = new System.Windows.Forms.NumericUpDown();
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Порт сервера:";
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 56);
            this.Controls.Add(this.numericUpDownServerPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.btnStartServer);
            this.Name = "Form1";
            this.Text = "HTTP server";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownServerPort;
    }
}

