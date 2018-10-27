using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Interfaces
{
    public interface ICliente
    {
        string RazaoSocial { get; }
        bool Tipo { get; }
        string Cpf { get; }
        string Cnpj { get; }
        string Inscricao { get; }
        string InscricaoSuframa { get; }
        string Email { get; }
        string Endereco { get; }
        string Numero { get; }
        string Complemento { get; }
        string Bairro { get; }
        string Cep { get; }
        string Cidade { get; }
        string CidadeIBGE { get; }
        string UF { get; }
        string UFIBGE { get; }
        string Pais { get; }
        string PaisIBGE { get; }
        string Telefone { get; }
        string Observacao { get; }
        bool bContribuinte { get; }
    }
}
