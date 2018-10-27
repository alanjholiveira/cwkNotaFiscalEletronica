using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface INotaParcela
    {
        int Sequencia { get; }
        DateTime Vencimento { get; }
        decimal Valor { get; }

        string FaturaNumero { get; }
        decimal FaturaValorOriginal { get; }
        decimal FaturaValorLiquido { get; }

        String FormaPagamento { get; }

    }
}
