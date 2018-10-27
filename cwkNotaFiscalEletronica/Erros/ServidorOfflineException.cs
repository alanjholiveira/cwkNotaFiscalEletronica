using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Erros
{
    public class ServidorOfflineException : NFeException
    {
        public ServidorOfflineException(IDictionary<string, string> _retornoXml) : base(_retornoXml) { }
        public ServidorOfflineException(IDictionary<string, string> _retornoXml, string mensagem) : base(_retornoXml, mensagem) { }
    }
}
