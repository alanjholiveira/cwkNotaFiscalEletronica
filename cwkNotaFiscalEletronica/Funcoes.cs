using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Net;
using DFe.Utils;
using cwkNotaFiscalEletronica.Interfaces;
using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using NFe.Utils;

namespace cwkNotaFiscalEletronica
{
    public static class Funcoes
    {
        public static string LimpaStr(string cnpj)
        {
            if (cnpj != null)
            {
                return cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
            }
            else
                return "";
        }

        public static string Conv_TAG_CST(int aIndex)
        {
            switch (aIndex)
            {
                case 0: return "00";
                case 1: return "10";
                case 2: return "20";
                case 3: return "30";
                case 4: return "40";
                case 5: return "41";
                case 6: return "50";
                case 7: return "51";
                case 8: return "60";
                case 9: return "70";
                case 10: return "90";
                case 11: return "Part";
                case 12: return "ST";
                case 13: return "101";
                case 14: return "102";
                case 15: return "103";
                case 16: return "201";
                case 17: return "202";
                case 18: return "203";
                case 19: return "300";
                case 20: return "400";
                case 21: return "500";
                case 22: return "900";
            }
            return "";
        }

        public static string Conv_CST_IPI(int aIndex)
        {
            string ret = "";
            switch (aIndex)
            {
                case 0:
                    ret = "00";
                    break;
                case 1:
                    ret = "01";
                    break;
                case 2:
                    ret = "02";
                    break;
                case 3:
                    ret = "03";
                    break;
                case 4:
                    ret = "04";
                    break;
                case 5:
                    ret = "05";
                    break;
                case 6:
                    ret = "49";
                    break;
                case 7:
                    ret = "50";
                    break;
                case 8:
                    ret = "51";
                    break;
                case 9:
                    ret = "52";
                    break;
                case 10:
                    ret = "53";
                    break;
                case 11:
                    ret = "54";
                    break;
                case 12:
                    ret = "55";
                    break;
                case 13:
                    ret = "99";
                    break;
            }
            return ret;
        }

        public static string Conv_CST_Pis(int aIndex)
        {
            string ret = "";
            switch (aIndex)
            {
                case 0:
                    ret = "01";
                    break;
                case 1:
                    ret = "02";
                    break;
                case 2:
                    ret = "03";
                    break;
                case 3:
                    ret = "04";
                    break;
                case 4:
                    ret = "06";
                    break;
                case 5:
                    ret = "07";
                    break;
                case 6:
                    ret = "08";
                    break;
                case 7:
                    ret = "09";
                    break;
                case 8:
                    ret = "99";
                    break;
            }
            return ret;
        }

        public static IEnumerable<XElement> WhereLocalName<T>(this IEnumerable<T> source, string localName) where T : XContainer
        {
            return source.Elements().Where(e => e.Name.LocalName == localName);
        }

        public static IDictionary<string, string> DesmembrarXml(this XDocument documentoXml)
        {
            return DesmembrarXml(documentoXml.Root);
        }

        public static IDictionary<string, string> DesmembrarXml(this string xml)
        {
            return DesmembrarXml(XElement.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xml)))));
        }

        public static IDictionary<string, string> DesmembrarXml(XElement elemento)
        {
            IDictionary<string, string> retorno = new Dictionary<string, string>();
            Int32 contadorCHNFePend = 0;


            foreach (var item in elemento.Elements())
            {
                if (item.HasElements)
                {
                    foreach (var nosInternos in DesmembrarXml(item.ToString()))
                    {
                        try
                        {
                            retorno.Add(item.Parent.Name.LocalName + " " + nosInternos.Key, nosInternos.Value);
                        }
                        catch
                        {
                            retorno[item.Parent.Name.LocalName + " " + nosInternos.Key] = nosInternos.Value;
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (item.Name.LocalName.Trim().Equals("chNFePend"))
                        {
                            contadorCHNFePend++;
                            retorno.Add(contadorCHNFePend + "-" + item.Name.LocalName, item.Value);
                        }
                        else
                            retorno.Add(TagsXMLNFe.RetornoENVIO.Tags[item.Name.LocalName], item.Value);
                    }
                    catch
                    {
                        retorno.Add(item.Name.LocalName, item.Value);
                    }
                }
            }

            return retorno;
        }

        public static string AbrirArquivo(string arquivo)
        {
            StringBuilder arq_ret = new StringBuilder();
            StreamReader objReader = new StreamReader(arquivo);
            string sLine = "";

            while (sLine != null)
            {
                sLine = objReader.ReadLine();

                if (sLine != null)
                    arq_ret.Append(sLine);
            }
            objReader.Close();

            return arq_ret.ToString();
        }


        ///Zeus Automação
        ///
        /// <summary>
        /// Metodo resposavel por retorna UF da empresa por Zeus
        /// </summary>
        /// <param name="uf"></param>
        /// <returns></returns>
        public static Estado RetornaUF(string uf)
        {
            //instancia a variável para o padrão de ES
            Estado ufEmpresa = Estado.ES;

            switch (uf)
            {
                case "AC":
                    ufEmpresa = Estado.AC;
                    break;
                case "AL":
                    ufEmpresa = Estado.AL;
                    break;
                case "AM":
                    ufEmpresa = Estado.AM;
                    break;
                case "AN":
                    ufEmpresa = Estado.AN;
                    break;
                case "AP":
                    ufEmpresa = Estado.AP;
                    break;
                case "BA":
                    ufEmpresa = Estado.BA;
                    break;
                case "CE":
                    ufEmpresa = Estado.CE;
                    break;
                case "DF":
                    ufEmpresa = Estado.DF;
                    break;
                case "ES":
                    ufEmpresa = Estado.ES;
                    break;
                case "EX":
                    ufEmpresa = Estado.EX;
                    break;
                case "GO":
                    ufEmpresa = Estado.GO;
                    break;
                case "MA":
                    ufEmpresa = Estado.MA;
                    break;
                case "MG":
                    ufEmpresa = Estado.MG;
                    break;
                case "MS":
                    ufEmpresa = Estado.MS;
                    break;
                case "MT":
                    ufEmpresa = Estado.MT;
                    break;
                case "PA":
                    ufEmpresa = Estado.PA;
                    break;
                case "PB":
                    ufEmpresa = Estado.PB;
                    break;
                case "PE":
                    ufEmpresa = Estado.PE;
                    break;
                case "PI":
                    ufEmpresa = Estado.PI;
                    break;
                case "PR":
                    ufEmpresa = Estado.PR;
                    break;
                case "RJ":
                    ufEmpresa = Estado.RJ;
                    break;
                case "RN":
                    ufEmpresa = Estado.RN;
                    break;
                case "RO":
                    ufEmpresa = Estado.RO;
                    break;
                case "RR":
                    ufEmpresa = Estado.RR;
                    break;
                case "RS":
                    ufEmpresa = Estado.RS;
                    break;
                case "SC":
                    ufEmpresa = Estado.SC;
                    break;
                case "SE":
                    ufEmpresa = Estado.SE;
                    break;
                case "SP":
                    ufEmpresa = Estado.SP;
                    break;
                case "TO":
                    ufEmpresa = Estado.TO;
                    break;
                default:
                    ufEmpresa = Estado.ES;
                    break;
            }

            return ufEmpresa;
        }

        public static TipoAmbiente RetornaTipoAmbiente(int tpAmb)
        {
            TipoAmbiente ambiente = TipoAmbiente.Homologacao;

            switch (tpAmb)
            {
                case 1:
                    ambiente = TipoAmbiente.Producao;
                    break;
                case 2:
                    ambiente = TipoAmbiente.Homologacao;
                    break;
            }

            return ambiente;

        }

        public static NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao RetornarTipoEmissao(string tpEmis)
        {
            NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teNormal;

            switch (tpEmis)
            {
                case "1":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teNormal;
                    break;
                case "2":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teFSIA;
                    break;
                case "3":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teSCAN;
                    break;
                case "4":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teEPEC;
                    break;
                case "5":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teFSDA;
                    break;
                case "6":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teSVCAN;
                    break;
                case "7":
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teSVCRS;
                    break;
                default:
                    emissao = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao.teNormal;
                    break;
            }

            return emissao;
        }

        /// <summary>
        /// Retorna Certifiicado ZEUS
        /// </summary>
        /// <param name="TipoCert"></param>
        /// <param name="Empresa"></param>
        /// <returns></returns>
        public static ConfiguracaoCertificado RetornaCertificado(string TipoCert, IEmpresa Empresa)
        {
            ConfiguracaoCertificado ConfgCertificado;

            #region Seleciona Tipo de Certificado
            switch (TipoCert)
            {

                case "A1Arquivo":
                    ConfgCertificado = new ConfiguracaoCertificado
                    {
                        TipoCertificado = TipoCertificado.A1Arquivo,
                        Arquivo = Empresa.Certificado,
                        Senha = Empresa.PinNfe
                    };
                    break;
                case "A1Repositorio":
                    ConfgCertificado = new ConfiguracaoCertificado
                    {
                        TipoCertificado = TipoCertificado.A1Repositorio,
                        Serial = Empresa.Certificado
                    };
                    break;
                case "A3":
                    ConfgCertificado = new ConfiguracaoCertificado
                    {
                        TipoCertificado = TipoCertificado.A3,
                        Serial = Empresa.Certificado,
                        Senha = Empresa.PinNfe
                    };
                    break;
                default:
                    ConfgCertificado = new ConfiguracaoCertificado
                    {
                        TipoCertificado = TipoCertificado.A1Repositorio,
                        Serial = Empresa.Certificado,
                    };
                    break;
            }
            #endregion

            return ConfgCertificado;
        }

        public static VersaoQrCode RetornaVersaoQrCode(int versao)
        {
            switch (versao)
            {
                case 1:
                    return VersaoQrCode.QrCodeVersao1;
                case 2:
                    return VersaoQrCode.QrCodeVersao2;
                default:
                    return VersaoQrCode.QrCodeVersao1;

            }
        }



    }
}
