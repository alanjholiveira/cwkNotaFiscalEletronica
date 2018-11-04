using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface IEmpresa
    {
        string Certificado { get; }
        int Ambiente { get; }
        string Cidade { get; }
        string CidadeIBGE { get; }
        string UF { get; }
        string UFIBGE { get; }

        string Nome { get; }
        string Fantasia { get; }
        string Cnpj { get; }
        string Inscricao { get; }
        string Endereco { get; }
        string Bairro { get; }
        string Telefone { get; }
        string Fax { get; }
        int FilialNF { get; }
        string Tipo { get; }
        string Numero { get; }
        string Responsavel { get; }
        string CEP { get; }
        EmpresaCRT TipoCRT { get; }
        decimal AliqSimplesSubst { get; }
        TipoST TipoST { get; }
        string ServidorSMTP { get; }
        string EmailUsuario { get; }
        string EmailSenha { get; }
        int PortaSMTP { get; }
        DateTime ContingenciaDataHora { get; }
        string ContingenciaJustificativa { get; }
        string Complemento { get; }
        string VersaoEsquema { get; }
        string DiretorioPadrao { get; }
        string PinNfe { get; }
        bool GMailAutenticacao { get; }
        int ComponenteDfe { get; }
        string FileCertificado { get; }
        string CIdToken { get; }
        string Csc { get; }
        int QrCode { get; }
        bool CacheCertificado { get; }
        string PinCert { get; }
    }
}
