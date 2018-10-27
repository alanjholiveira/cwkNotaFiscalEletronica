using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CwkGeraXmlDestinatario
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            cbCertificado.Items.AddRange(GeraXmlDestinatario.GetCertificados());
        }

        private void btnDiretorioLog_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtLog.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnDiretorioXmlDest_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtXmlDest.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnTemplates_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtTemplates.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnEsquemas_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            txtEsquemas.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnEnvLot_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtLogEnvio.Text = openFileDialog1.FileName;
        }

        private void btnConsRecibo_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            txtConsRecibo.Text = openFileDialog1.FileName;
        }

        private void btnXmlSaida_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            txtXmlSaida.Text = saveFileDialog1.FileName;
        }

        private void btnGerar_Click(object sender, EventArgs e)
        {
            GeraXmlDestinatario gerarXml = new GeraXmlDestinatario();
            switch (cbAmbiente.SelectedIndex)
            {
                case 0: gerarXml.Ambiente = Enums.NFeAmbiente.Homologacao; break;
                case 1: gerarXml.Ambiente = Enums.NFeAmbiente.Producao; break;
            }

            gerarXml.ArquivoConsultaRecibo = txtConsRecibo.Text;
            gerarXml.ArquivoEnvioLote = txtLogEnvio.Text;
            gerarXml.ArquivoSaida = txtXmlSaida.Text;
            gerarXml.ChaveNota = txtChaveNota.Text;
            gerarXml.DiretorioEsquemas = txtEsquemas.Text;
            gerarXml.DiretorioLog = txtLog.Text;
            gerarXml.DiretorioTemplates = txtTemplates.Text;
            gerarXml.DiretorioXmlDestinatario = txtXmlSaida.Text;
            gerarXml.NomeCertificado = cbCertificado.Text;
            
            gerarXml.VersaoManual = cbLayout.Text;

            try
            {
                gerarXml.GerarXml();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }


    }
}
