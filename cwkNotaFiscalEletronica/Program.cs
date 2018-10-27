using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFeX;

namespace cwkNotaFiscalEletronica
{
    public class Program
    {
        public static void Main(string[] args)
        {

            IspdNFeX nfex = new spdNFeX();
            nfex.ConfigurarSoftwareHouse("09813496000197", "");
            nfex.Ambiente = Ambiente.akProducao;
            nfex.DiretorioLog = "Log\\";
            nfex.VersaoManual = "3.0";

            nfex.DiretorioEsquemas = @"Esquemas\";
            nfex.DiretorioTemplates = @"Templates\";
            nfex.TipoCertificado = TipoCertificado.ckFile;

            Console.WriteLine("Certificado:");
            nfex.NomeCertificado = Console.ReadLine();

            Console.WriteLine("Chave da nota:");
            string chaveNfe = Console.ReadLine();

            Console.WriteLine("Nome do loag envio lote:");
            string logEnvioLote = Console.ReadLine();;

            Console.WriteLine("Nome do log consulta Recibo:");
            string logConsultaRecibo = Console.ReadLine();
            nfex.GeraXMLEnvioDestinatario(chaveNfe, logEnvioLote, logConsultaRecibo, "notaSaida");


        }
    }
}
