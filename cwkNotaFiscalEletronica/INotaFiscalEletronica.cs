using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using cwkNotaFiscalEletronica.Interfaces;

namespace cwkNotaFiscalEletronica
{
    public abstract class INotaFiscalEletronica2
    {
        public Danfe Danfe { get; set; }
        public DanfeNFCe DanfeNFCe { get; set; }
        public IEmpresa Empresa { get; set; }
        public INota Nota { get; set; }
        public string Observacoes { get; set; }
        public TipoDoCertificado TipoDoCertificado { get; set; }
        public string DiretorioPadrao { get; private set; }
        protected cwkAmbiente CwkAmbiente { get; set; }
        protected TipoEmissao FormaEmissao { get; set; }

        INotaFiscalEletronicaZeus nfe;

        public INotaFiscalEletronica2(TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string _diretorioPadrao)
        {
            CwkAmbiente = _ambiente;
            SetDiretorioPadrao(_diretorioPadrao);
            //SetSpdNFeX(_tipoServidor, _tipoCertificado);

            
        }

        private void SetDiretorioPadrao(string _diretorioPadrao)
        {
            if (String.IsNullOrEmpty(_diretorioPadrao) || !Directory.Exists(_diretorioPadrao))
                DiretorioPadrao = GetDiretorioSistema();
            else
                DiretorioPadrao = _diretorioPadrao;
        }
        private static string GetDiretorioSistema()
        {
            string dir = Assembly.GetEntryAssembly().Location;
            return Path.Combine(Path.GetDirectoryName(dir), String.Empty);
        }


    }
}
