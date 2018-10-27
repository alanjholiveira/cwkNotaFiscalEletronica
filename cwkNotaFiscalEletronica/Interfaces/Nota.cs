using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface INota
    {
        int Id { get; }
        IEmpresa Empresa { get; }
        TipoNotaEntSaida TipoNota { get; }
        int Ent_Sai { get; }
        DateTime DtSaida { get; }
        ICliente Cliente { get; }
        string PessoaNome { get; }
        string PessoaCNPJCPF { get; }
        string PessoaInscRG { get; }
        string PessoaEndereco { get; }
        string PessoaBairro { get; }
        string PessoaCidade { get; }
        string PessoaCidadeIBGE { get; }
        string PessoaCEP { get; }
        string PessoaUF { get; }
        string PessoaTelefone { get; }
        string PessoaEmail { get; }
        decimal TotalProduto { get; }
        decimal PercDesconto { get; }
        decimal ValorDesconto { get; }
        decimal ValorAcrescimo { get; }
        decimal TotalNota { get; }
        string Observacao1 { get; }
        string Observacao2 { get; }
        string Observacao3 { get; }
        string Observacao4 { get; }
        string Observacao5 { get; }
        string Serie { get; }
        int Numero { get; }
        string TransNome { get; }
        string TransEndereco { get; }
        string TransCidade { get; }
        string TransUF { get; }
        string TransCNPJCPF { get; }
        string TransInscricao { get; }
        string TransPlaca { get; }
        string TransPlacaUF { get; }
        string VolumeQuant { get; }
        string VolumeEspecie { get; }
        string VolumeMarca { get; }
        string VolumeNumero { get; }
        decimal VolumePesoBruto { get; }
        decimal VolumePesoLiquido { get; }
        string CancUsuario { get; set; }
        string CancMotivo { get; set; }
        DateTime? CancDt { get; set; }
        string ChaveNota { get; set; }
        string NumeroRecibo { get; set; }
        string NumeroProtocolo { get; set; }
        string LogEnvio { get; set; }
        string LogRecibo { get; set; }
        decimal ValorFrete { get; }
        string ObservacaoSistema { get; }
        string ObservacaoUsuario { get; }
        string TipoFrete { get; }
        decimal TotalIpi { get; }
        bool bImpressa { get; set; }
        string PessoaNumero { get; }
        int ModeloDocto { get; set; }
        string Status { get; set; }
        string StatusMotivo { get; set; }
        string UltimoXmlRecebido { get; set; }
        string XmlLogEnvNFe { get; set; }
        string XmlLogRecNFe { get; set; }
        string XmlDestinatarioNFe { get; set; }
        string PessoaSUFRAMA { get; }
        DateTime DtEmissao { get; }
        string ZA02_UFEmbarq { get; }
        string ZA03_xLocEmbarq { get; }
        decimal W11_vII { get; }
        decimal ValorSeguro { get; }
        decimal OutrasDespesas { get; }
        bool EnviaTagTotalImposto { get; }

        IList<INotaItem> NotaItems { get; }
        IList<INotaParcela> NotaParcelas { get; }
        IEnderecoEntrega EntregaEndreco { get; }
        INota NotaComplementada { get; }
        INota NotaReferenciada { get; }

        int idDest { get; }
        bool BConsumidorFinal { get; }
    }
}
