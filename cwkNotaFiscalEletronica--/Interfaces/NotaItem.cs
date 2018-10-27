using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface INotaItem
    {
        int Sequencia { get; }
        string ProdutoCodigo { get; }
        string ProdutoNome { get; }
        string ProdutoNCM { get; }
        string CFOPCodigo { get; }
        string ProdutoDescReduzida { get; }
        decimal Quantidade { get; }
        string Unidade { get; }
        decimal ValorCalculado { get; }
        decimal Valor { get; }
        decimal SubTotal { get; }
        decimal PercDesconto { get; }
        decimal ValorDesconto { get; }
        decimal RAT_Desconto { get; }
        decimal RAT_Acrescimo { get; }
        decimal Total { get; }
        decimal PesoBruto { get; }
        decimal PesoLiquido { get; }
        DateTime Dt { get; }
        int Ent_Sai { get; }
        bool CalcCustoMedio { get; }
        decimal ValorCustoMedia { get; }
        decimal BaseICMS { get; }
        decimal AliqICMS { get; }
        decimal AliqICMSNormal { get; }
        decimal ValorICMS { get; }
        decimal ValorIsentoICMS { get; }
        decimal ValorOutroICMS { get; }
        decimal ValorRetidoICMS { get; }
        string SitTrib { get; }
        decimal BaseICMSSubst { get; }
        decimal ValorICMSReducao { get; }
        decimal ValorICMSReducaoDif { get; }
        string TextoLei { get; }
        string TAG_CST { get; }
        int modBC_N13 { get; }
        int modBCST_N18 { get; }
        string CST_Pis { get; }
        decimal vBC_Q07 { get; }
        decimal pPIS_Q08 { get; }
        decimal vPIS_Q09 { get; }
        string CST_Cofins { get; }
        string CST_Ipi { get; }
        int orig_N11 { get; }
        decimal vBC_S07 { get; }
        decimal pCOFINS_S08 { get; }
        decimal vCOFINS_S11 { get; }
        decimal vBC_O10 { get; }
        decimal pIPI_O13 { get; }
        decimal vIPI_O14 { get; }
        decimal pRedBC_N14 { get; }
        decimal pICMSST_N22 { get; }
        string cEnq_O06 { get; }
        string InfAdicionais { get; }
        string AliqCupom { get; }
        string CFOPDescricao { get; }

        //4.0
        decimal pCredSN_N29 { get; }
        decimal vCredICMSSN_N30 { get; }
        decimal pMVAST_N19 { get; }
        decimal pRedBCST_N20 { get; }
        string cBenef_I05f { get; }
        int indEscala_I05d { get; }
        string CNPJFab_I05e { get; }
        decimal vBCFCP_N17a { get; }
        decimal pFCP_N17b { get; }
        decimal vFCP_N17c { get; }
        decimal vBCFCPST_N23a { get; }
        decimal pFCPST_N23b { get; }
        decimal vFCPST_N23d { get; }

        //cEAN
        string cEAN { get; }
        string cEANTrib { get; }

        string I19_nDI { get; }
        DateTime? I20_dDI { get; }
        string I21_xLocDesemb { get; }
        string I22_UFDesemb { get; }
        DateTime? I23_dDesemb { get; }
        string I24_cExportador { get; }
        decimal P02_vBC { get; }
        decimal P03_vDespAdu { get; }
        decimal P04_vII { get; }
        decimal P05_vIOF { get; }
        IList<IAdicaoNotaItem> AdicoesNotaItem { get; }
        decimal ValorFrete { get; }
        decimal ValorSeguro { get; }
        decimal OutrasDespesas { get; }
        decimal? TotalImpostos { get; }
        string FCI { get; }

        string NumDrawBack { get; }
        decimal vICMSDeson { get; }
        int motDesICMS { get; }

        int ViaTransp { get; }
        decimal ValorFreteRenovacaoMarinhaMercante { get; }
        string Cest { get; }
        decimal AliqInterna { get; }
        decimal vICMSUFDest_NA15 { get; }
        decimal vICMSUFRemet_NA17 { get;  }
        decimal pICMSInter { get; }


    } 
}
