using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface IAdicaoNotaItem
    {
        decimal I26_nAdicao { get; }
        decimal I27_nSeqAdic { get; }
        string I28_cFabricante { get; }
        decimal I29_vDescDI { get; }
        string I30_xPed { get; }
        decimal I31_nItemPed { get; }
    }
}
