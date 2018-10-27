using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Erros
{
    class SemRespostaDoServidorException: NFeException
    {
        public SemRespostaDoServidorException(IDictionary<string, string> _retornoXml) : base(_retornoXml) { }
        public SemRespostaDoServidorException(IDictionary<string, string> _retornoXml, string mensagem) : base(_retornoXml, mensagem) { }
    }
}
