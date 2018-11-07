using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFeX;
using System.Reflection;
using System.IO;
using NFe.Classes.Informacoes.Identificacao.Tipos;

namespace cwkNotaFiscalEletronica
{
    internal static class FactoryIspdNFeDPECX
    {
        public static IspdNFeDPECX Build(TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string diretorioPadrao)
        {
            TipoCertificado tipo;
            switch (_tipoCertificado)
            {
                case TipoDoCertificado.ckFile: tipo = TipoCertificado.ckFile; break;
                case TipoDoCertificado.ckSmartCard: tipo = TipoCertificado.ckSmartCard; break;
                case TipoDoCertificado.ckMemory: tipo = TipoCertificado.ckMemory; break;
                case TipoDoCertificado.ckLocalMachine: tipo = TipoCertificado.ckLocalMashine; break;
                case TipoDoCertificado.ckActiveDirectory: tipo = TipoCertificado.ckActiveDirectory; break;
                default:
                    tipo = TipoCertificado.ckFile;
                    break;
            }
            try
            {
                IspdNFeDPECX spdNFeDPECX = BuildNormal(_ambiente, diretorioPadrao);
                spdNFeDPECX.TipoCertificado = tipo;
                return spdNFeDPECX;
            }
            catch (Exception)
            {
                return null;                
            }
            
            // Código legado -- retirada dos métodos relacionados à SCAN, em favor do SVCAN/SVCRS (Ticket #2875)
            //switch (_tipoServidor)
            //{
            //    case TipoEmissao.Normal: spdNFeX = BuildNormal(_ambiente, diretorioPadrao); spdNFeX.TipoCertificado = tipo; return spdNFeX;
            //    case TipoEmissao.SCAN: spdNFeX = BuildSCAN(_ambiente); spdNFeX.TipoCertificado = tipo; return spdNFeX;
            //}
        }

        private static IspdNFeDPECX BuildNormal(cwkAmbiente _ambiente, string diretorioPadrao)
        {
            IspdNFeDPECX _spdNFeDPECX = new spdNFeDPECX();            
            switch (_ambiente)
            {
                case cwkAmbiente.Producao:
                    _spdNFeDPECX.Ambiente = Ambiente.akProducao;
                    _spdNFeDPECX.DiretorioLog = diretorioPadrao + "\\Log\\";
                    break;
                case cwkAmbiente.Homologacao:
                    _spdNFeDPECX.Ambiente = Ambiente.akHomologacao;
                    _spdNFeDPECX.DiretorioLog = diretorioPadrao + "\\LogHom\\";
                    break;
            }
            return _spdNFeDPECX;
        }

        // Código legado -- retirada dos métodos relacionados à SCAN, em favor do SVCAN/SVCRS (Ticket #2875)
        //private static IspdNFeX BuildSCAN(cwkAmbiente _ambiente)
        //{
        //    IspdNFeX _spdNFeX = new spdNFeSCAN();
        //    switch (_ambiente)
        //    {
        //        case cwkAmbiente.Producao:
        //            _spdNFeX.Ambiente = Ambiente.akProducao;
        //            _spdNFeX.ArquivoServidoresProd = "nfeServidoresProSCAN.ini";
        //            break;
        //        case cwkAmbiente.Homologacao:
        //            _spdNFeX.Ambiente = Ambiente.akHomologacao;
        //            _spdNFeX.ArquivoServidoresHom = "nfeServidoresHomSCAN.ini";
        //            break;
        //    }
        //    return _spdNFeX;
        //}
        //
    }
}
