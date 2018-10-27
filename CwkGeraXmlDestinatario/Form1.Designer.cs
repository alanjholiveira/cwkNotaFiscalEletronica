namespace CwkGeraXmlDestinatario
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.cbAmbiente = new System.Windows.Forms.ComboBox();
            this.cbLayout = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnDiretorioLog = new System.Windows.Forms.Button();
            this.btnDiretorioXmlDest = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtXmlDest = new System.Windows.Forms.TextBox();
            this.btnEsquemas = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtEsquemas = new System.Windows.Forms.TextBox();
            this.btnTemplates = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtTemplates = new System.Windows.Forms.TextBox();
            this.cbCertificado = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnXmlSaida = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.txtChaveNota = new System.Windows.Forms.TextBox();
            this.txtXmlSaida = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.btnConsRecibo = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.txtLogEnvio = new System.Windows.Forms.TextBox();
            this.txtConsRecibo = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btnEnvLot = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnGerar = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ambiente:";
            // 
            // cbAmbiente
            // 
            this.cbAmbiente.FormattingEnabled = true;
            this.cbAmbiente.Items.AddRange(new object[] {
            "Homologacao",
            "Producao"});
            this.cbAmbiente.Location = new System.Drawing.Point(110, 19);
            this.cbAmbiente.Name = "cbAmbiente";
            this.cbAmbiente.Size = new System.Drawing.Size(121, 21);
            this.cbAmbiente.TabIndex = 2;
            // 
            // cbLayout
            // 
            this.cbLayout.FormattingEnabled = true;
            this.cbLayout.Items.AddRange(new object[] {
            "3.0",
            "4.0"});
            this.cbLayout.Location = new System.Drawing.Point(110, 46);
            this.cbLayout.Name = "cbLayout";
            this.cbLayout.Size = new System.Drawing.Size(121, 21);
            this.cbLayout.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(62, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Layout:";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(110, 73);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(259, 20);
            this.txtLog.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Dir. de Log:";
            // 
            // btnDiretorioLog
            // 
            this.btnDiretorioLog.Location = new System.Drawing.Point(375, 73);
            this.btnDiretorioLog.Name = "btnDiretorioLog";
            this.btnDiretorioLog.Size = new System.Drawing.Size(25, 20);
            this.btnDiretorioLog.TabIndex = 7;
            this.btnDiretorioLog.Text = "...";
            this.btnDiretorioLog.UseVisualStyleBackColor = true;
            this.btnDiretorioLog.Click += new System.EventHandler(this.btnDiretorioLog_Click);
            // 
            // btnDiretorioXmlDest
            // 
            this.btnDiretorioXmlDest.Location = new System.Drawing.Point(375, 99);
            this.btnDiretorioXmlDest.Name = "btnDiretorioXmlDest";
            this.btnDiretorioXmlDest.Size = new System.Drawing.Size(25, 20);
            this.btnDiretorioXmlDest.TabIndex = 10;
            this.btnDiretorioXmlDest.Text = "...";
            this.btnDiretorioXmlDest.UseVisualStyleBackColor = true;
            this.btnDiretorioXmlDest.Click += new System.EventHandler(this.btnDiretorioXmlDest_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Dir. de XmlDest:";
            // 
            // txtXmlDest
            // 
            this.txtXmlDest.Location = new System.Drawing.Point(110, 99);
            this.txtXmlDest.Name = "txtXmlDest";
            this.txtXmlDest.Size = new System.Drawing.Size(259, 20);
            this.txtXmlDest.TabIndex = 8;
            // 
            // btnEsquemas
            // 
            this.btnEsquemas.Location = new System.Drawing.Point(375, 151);
            this.btnEsquemas.Name = "btnEsquemas";
            this.btnEsquemas.Size = new System.Drawing.Size(25, 20);
            this.btnEsquemas.TabIndex = 16;
            this.btnEsquemas.Text = "...";
            this.btnEsquemas.UseVisualStyleBackColor = true;
            this.btnEsquemas.Click += new System.EventHandler(this.btnEsquemas_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 154);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Dir. de Esquemas:";
            // 
            // txtEsquemas
            // 
            this.txtEsquemas.Location = new System.Drawing.Point(110, 151);
            this.txtEsquemas.Name = "txtEsquemas";
            this.txtEsquemas.Size = new System.Drawing.Size(259, 20);
            this.txtEsquemas.TabIndex = 14;
            // 
            // btnTemplates
            // 
            this.btnTemplates.Location = new System.Drawing.Point(375, 125);
            this.btnTemplates.Name = "btnTemplates";
            this.btnTemplates.Size = new System.Drawing.Size(25, 20);
            this.btnTemplates.TabIndex = 13;
            this.btnTemplates.Text = "...";
            this.btnTemplates.UseVisualStyleBackColor = true;
            this.btnTemplates.Click += new System.EventHandler(this.btnTemplates_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 129);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Dir. de Templates:";
            // 
            // txtTemplates
            // 
            this.txtTemplates.Location = new System.Drawing.Point(110, 125);
            this.txtTemplates.Name = "txtTemplates";
            this.txtTemplates.Size = new System.Drawing.Size(259, 20);
            this.txtTemplates.TabIndex = 11;
            // 
            // cbCertificado
            // 
            this.cbCertificado.FormattingEnabled = true;
            this.cbCertificado.Location = new System.Drawing.Point(110, 177);
            this.cbCertificado.Name = "cbCertificado";
            this.cbCertificado.Size = new System.Drawing.Size(259, 21);
            this.cbCertificado.TabIndex = 18;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(44, 180);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Certificado:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbAmbiente);
            this.groupBox1.Controls.Add(this.cbCertificado);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnEsquemas);
            this.groupBox1.Controls.Add(this.cbLayout);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtLog);
            this.groupBox1.Controls.Add(this.txtEsquemas);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.btnTemplates);
            this.groupBox1.Controls.Add(this.btnDiretorioLog);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtXmlDest);
            this.groupBox1.Controls.Add(this.txtTemplates);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnDiretorioXmlDest);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 209);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Informações de configuração";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnXmlSaida);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.txtChaveNota);
            this.groupBox2.Controls.Add(this.txtXmlSaida);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.btnConsRecibo);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.txtLogEnvio);
            this.groupBox2.Controls.Add(this.txtConsRecibo);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.btnEnvLot);
            this.groupBox2.Location = new System.Drawing.Point(428, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(426, 128);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Informações da Nota";
            // 
            // btnXmlSaida
            // 
            this.btnXmlSaida.Location = new System.Drawing.Point(390, 96);
            this.btnXmlSaida.Name = "btnXmlSaida";
            this.btnXmlSaida.Size = new System.Drawing.Size(25, 20);
            this.btnXmlSaida.TabIndex = 16;
            this.btnXmlSaida.Text = "...";
            this.btnXmlSaida.UseVisualStyleBackColor = true;
            this.btnXmlSaida.Click += new System.EventHandler(this.btnXmlSaida_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(45, 99);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "Xml de Saída:";
            // 
            // txtChaveNota
            // 
            this.txtChaveNota.Location = new System.Drawing.Point(125, 19);
            this.txtChaveNota.Name = "txtChaveNota";
            this.txtChaveNota.Size = new System.Drawing.Size(259, 20);
            this.txtChaveNota.TabIndex = 5;
            // 
            // txtXmlSaida
            // 
            this.txtXmlSaida.Location = new System.Drawing.Point(125, 96);
            this.txtXmlSaida.Name = "txtXmlSaida";
            this.txtXmlSaida.Size = new System.Drawing.Size(259, 20);
            this.txtXmlSaida.TabIndex = 14;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(37, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(82, 13);
            this.label12.TabIndex = 6;
            this.label12.Text = "Chave da Nota:";
            // 
            // btnConsRecibo
            // 
            this.btnConsRecibo.Location = new System.Drawing.Point(390, 72);
            this.btnConsRecibo.Name = "btnConsRecibo";
            this.btnConsRecibo.Size = new System.Drawing.Size(25, 20);
            this.btnConsRecibo.TabIndex = 13;
            this.btnConsRecibo.Text = "...";
            this.btnConsRecibo.UseVisualStyleBackColor = true;
            this.btnConsRecibo.Click += new System.EventHandler(this.btnConsRecibo_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 77);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(113, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "Cons Recibo (pro-rec):";
            // 
            // txtLogEnvio
            // 
            this.txtLogEnvio.Location = new System.Drawing.Point(125, 46);
            this.txtLogEnvio.Name = "txtLogEnvio";
            this.txtLogEnvio.Size = new System.Drawing.Size(259, 20);
            this.txtLogEnvio.TabIndex = 8;
            // 
            // txtConsRecibo
            // 
            this.txtConsRecibo.Location = new System.Drawing.Point(125, 72);
            this.txtConsRecibo.Name = "txtConsRecibo";
            this.txtConsRecibo.Size = new System.Drawing.Size(259, 20);
            this.txtConsRecibo.TabIndex = 11;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(20, 50);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(99, 13);
            this.label14.TabIndex = 9;
            this.label14.Text = "Log Envio (env-lot):";
            // 
            // btnEnvLot
            // 
            this.btnEnvLot.Location = new System.Drawing.Point(390, 46);
            this.btnEnvLot.Name = "btnEnvLot";
            this.btnEnvLot.Size = new System.Drawing.Size(25, 20);
            this.btnEnvLot.TabIndex = 10;
            this.btnEnvLot.Text = "...";
            this.btnEnvLot.UseVisualStyleBackColor = true;
            this.btnEnvLot.Click += new System.EventHandler(this.btnEnvLot_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnGerar
            // 
            this.btnGerar.Location = new System.Drawing.Point(429, 147);
            this.btnGerar.Name = "btnGerar";
            this.btnGerar.Size = new System.Drawing.Size(425, 74);
            this.btnGerar.TabIndex = 21;
            this.btnGerar.Text = "Gerar";
            this.btnGerar.UseVisualStyleBackColor = true;
            this.btnGerar.Click += new System.EventHandler(this.btnGerar_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 228);
            this.Controls.Add(this.btnGerar);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbAmbiente;
        private System.Windows.Forms.ComboBox cbLayout;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnDiretorioLog;
        private System.Windows.Forms.Button btnDiretorioXmlDest;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtXmlDest;
        private System.Windows.Forms.Button btnEsquemas;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtEsquemas;
        private System.Windows.Forms.Button btnTemplates;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtTemplates;
        private System.Windows.Forms.ComboBox cbCertificado;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnXmlSaida;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtChaveNota;
        private System.Windows.Forms.TextBox txtXmlSaida;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnConsRecibo;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtLogEnvio;
        private System.Windows.Forms.TextBox txtConsRecibo;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btnEnvLot;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnGerar;
    }
}

