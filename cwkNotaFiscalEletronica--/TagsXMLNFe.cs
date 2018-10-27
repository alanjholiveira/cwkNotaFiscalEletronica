using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica
{
    internal static class TagsXMLNFe
    {
        public static class RetornoENVIO
        {
            private static Dictionary<string, string> _dicionario;

            public static Dictionary<string, string> Tags { get { return _dicionario; } }

            static RetornoENVIO()
            {
                _dicionario = new Dictionary<string, string>();
                _dicionario.Add("retEnviNFe", "Retorno de envio");
                _dicionario.Add("tpAmb", "Ambiente");
                _dicionario.Add("cStat", "Status");
                _dicionario.Add("xMotivo", "Razão");
                _dicionario.Add("cUF", "Estado de Origem");
                _dicionario.Add("infRec", "Informações");
                _dicionario.Add("nRec", "Número do recibo");
                _dicionario.Add("dhRecbto", "Data de recebimento");
                _dicionario.Add("tMed", "tMed???");
                _dicionario.Add("verAplic", "Versão");
                _dicionario.Add("chNFe", "Chave");
                _dicionario.Add("nProt", "Protocolo");

            }
                
            
        }
    }
}
