using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Erros
{
    public class NFeException : Exception
    {
        IDictionary<string, string> xml;
            
        public NFeException(IDictionary<string, string> _retornoXml, string mensagem) : base(mensagem)
        {
            xml = _retornoXml;
        }

        public NFeException(IDictionary<string, string> _retornoXml)
        {
            xml = _retornoXml;
        }
    }
}
