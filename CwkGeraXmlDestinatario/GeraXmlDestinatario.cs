using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFeX;

namespace CwkGeraXmlDestinatario
{
    public class GeraXmlDestinatario
    {
        public CwkGeraXmlDestinatario.Enums.NFeAmbiente Ambiente { get; set; }//enum

        public string DiretorioLog { get; set; }
        public string DiretorioXmlDestinatario { get; set; }
//      _spdNFeX.DiretorioLog =  diretorioAplicacao + "\\LogHom\\";
//      _spdNFeX.DiretorioXmlDestinatario = diretorioAplicacao + "\\XmlDestinatarioHom\\";

        public string DiretorioEsquemas { get; set; }
        public string DiretorioTemplates { get; set; }
            //_spdNFeX.DiretorioEsquemas = @"Esquemas\";
            //_spdNFeX.DiretorioTemplates = @"Templates\";

        public string NomeCertificado { get; set; }

        //_spdNFeX.VersaoManual = "4.0";
        public string VersaoManual { get; set; }

        public string ChaveNota { get; set; }
        public string ArquivoEnvioLote { get; set; }
        public string ArquivoConsultaRecibo { get; set; }
        public string ArquivoSaida { get; set; }

        public void GerarXml()
        {
            IspdNFeX nfex = new spdNFeX();
            nfex.ConfigurarSoftwareHouse("09813496000197", "");

            switch (Ambiente)
            {
                case Enums.NFeAmbiente.Producao: nfex.Ambiente = NFeX.Ambiente.akProducao; break;
                case Enums.NFeAmbiente.Homologacao: nfex.Ambiente = NFeX.Ambiente.akHomologacao; break;
            }

            nfex.DiretorioLog = this.DiretorioLog;
            nfex.DiretorioXmlDestinatario = this.DiretorioXmlDestinatario;
            nfex.DiretorioEsquemas = this.DiretorioEsquemas;
            nfex.DiretorioTemplates = this.DiretorioTemplates;
            nfex.NomeCertificado = this.NomeCertificado;
            nfex.VersaoManual = this.VersaoManual;

            nfex.GeraXMLEnvioDestinatario(this.ChaveNota, this.ArquivoEnvioLote, this.ArquivoConsultaRecibo, this.ArquivoSaida);
        }

        public static string[] GetCertificados()
        {
            IspdNFeX nfex = new spdNFeX();
            nfex.ConfigurarSoftwareHouse("09813496000197", "");
            string certificados = nfex.ListarCertificados("|");

            if (!String.IsNullOrEmpty(certificados))
            {
                return certificados.Split('|');
            }
            else
                return new string[] { };
        }
    }
}
