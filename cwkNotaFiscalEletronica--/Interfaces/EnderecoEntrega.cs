using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface IEnderecoEntrega
    {
        string Logradouro { get; }
        string Numero { get; }
        string Complemente { get; }
        string Bairro { get; }
        string CidadeCodigoIBGE { get; }
        string CidadeNome { get; }
        string UFSigla { get; set; }
    }
}
