using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Net;

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

    }
}
