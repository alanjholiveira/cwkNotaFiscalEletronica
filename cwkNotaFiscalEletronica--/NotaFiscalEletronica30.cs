using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFeX;
using NFeDataSetX;
using cwkNotaFiscalEletronica.Interfaces;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using cwkNotaFiscalEletronica.Erros;

namespace cwkNotaFiscalEletronica
{
    internal class NotaFiscalEletronica30 : INotaFiscalEletronica
    {
        private string obs;

        public NotaFiscalEletronica30(TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string _diretorioPadrao)
            : base(_tipoServidor, _ambiente, _tipoCertificado, _diretorioPadrao) { }

        public override void Iniciar()
        {
            obs = "";
            SpdNFeX.CNPJ = Funcoes.LimpaStr(Empresa.Cnpj);
            SpdNFeX.UF = Empresa.UF; ;
            SpdNFeX.NomeCertificado = Empresa.Certificado;
            SpdNFeX.VersaoManual = "3.0";
            SpdNFeX.FraseContingencia = "DANFE em Contingencia";
            SpdNFeX.FraseHomologacao = "SEM VALOR FISCAL";

            string diretorioAplicacao = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            SpdNFeX.ModeloRetrato = diretorioAplicacao + @"\Templates\vm30\Danfe\Retrato.rtm";
            SpdNFeX.ModeloPaisagem = diretorioAplicacao + @"\Templates\vm30\Danfe\Paisagem.rtm";
            
            SpdNFeX.LogotipoEmitente = @"Templates\Logo.bmp";


        }

        private bool StatusWebService(string aXml, List<string> aListaRetorno)
        {
            if (VerificaCodigo(aXml, "107"))
            {
                aListaRetorno = null;
                return true;
            }
            else
            {
                aListaRetorno = NotaFiscalEletronica30.TrataRetorno(aXml, "cStat", TipoRetorno.Motivo);
                return false;
            }
        }

        private bool VerificaCodigo(string aXml, string aCodigo)
        {
            int tem = 0;

            tem = (from r in NotaFiscalEletronica30.TrataRetorno(aXml, "cStat", TipoRetorno.Codigo)
                   where r == aCodigo
                   select r).Count();

            if (tem > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<string> TrataRetorno(string aXml, string aTag, TipoRetorno aTipoRetorno)
        {
            List<string> retorno = new List<string>();

            XmlDocument _xml = new XmlDocument();
            _xml.LoadXml(aXml);
            XPathNavigator nav = _xml.CreateNavigator();

            XPathExpression expr;
            expr = nav.Compile("//*");
            XPathNodeIterator iterator = nav.Select(expr);

            try
            {
                while (iterator.MoveNext())
                {
                    XPathNavigator nav2 = iterator.Current.Clone();
                    if (nav2.Name == aTag)
                    {
                        if (aTipoRetorno == TipoRetorno.Codigo)
                        {
                            retorno.Add(nav2.Value);
                        }
                        else
                        {
                            //retorno.Add(new Erro(int.Parse(nav2.Value)).Instrucao);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retorno;
        }

        private void DadosNFe()
        {
            SpdNFeDataSetX.SetCampo(("Id_A03=0")); //Calcula Automático. Essa linha é desnecessária
            SpdNFeDataSetX.SetCampo(("versao_A02=1.10")); //Versão do Layout que está utilizando

            SpdNFeDataSetX.SetCampo(("cUF_B02=" + Nota.Empresa.UFIBGE)); //Codigo da UF para o estado de SP (Emitente da NFe)
            SpdNFeDataSetX.SetCampo(("cNF_B03=" + Nota.Id.ToString().PadLeft(9, '0'))); //Código Interno do Sistema que está integrando com a NFe
            SpdNFeDataSetX.SetCampo(("natOp_B04=" + Nota.NotaItems.First().CFOPDescricao)); //Descrição da(s) CFOP(s) envolvidas nessa NFe
            SpdNFeDataSetX.SetCampo(("indPag_B05=0")); //Indicador da Forma de Pgto (0- a Vista, 1 a Prazo)
            SpdNFeDataSetX.SetCampo(("mod_B06=55")); //Código do Modelo de Documento Fiscal
            SpdNFeDataSetX.SetCampo(("serie_B07=" + Nota.Serie)); //S érie do Documento
            SpdNFeDataSetX.SetCampo(("nNF_B08=" + Nota.Numero.ToString()));// + txtNumNF.Text)); //Número da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("dEmi_B09=" + Nota.DtSaida.ToString("yyyy-MM-dd"))); //Data de Emissão da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("dSaiEnt_B10=" + Nota.DtSaida.ToString("yyyy-MM-dd"))); //Data de Saída ou Entrada da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("tpNF_B11=" + (Nota.TipoNota == TipoNotaEntSaida.Entrada ? 0 : 1).ToString())); //Tipo de Documento Fiscal (0-Entrada, 1-Saída)
            SpdNFeDataSetX.SetCampo(("cMunFG_B12=" + Nota.Empresa.CidadeIBGE)); //Código do Município, conforme Tabela do IBGE
            SpdNFeDataSetX.SetCampo(("tpImp_B21=" + ((int)Danfe).ToString())); //Tipo de Impressão da Danfe (1- Retrato , 2-Paisagem)
            SpdNFeDataSetX.SetCampo(("tpEmis_B22=" + (FormaEmissao == TipoEmissao.Normal ? 1 : 2 ).ToString())); //Forma de Emissão da NFe (1-Normal, 2-Contigencia)
            SpdNFeDataSetX.SetCampo(("cDV_B23= ")); //Calcula Automatico - Linha desnecessária já que o componente calcula o Dígito Verificador automaticamente e coloca no devido campo
            SpdNFeDataSetX.SetCampo(("tpAmb_B24=" + ((int)CwkAmbiente).ToString())); //Identificação do Ambiente (1- Producao, 2-Homologação)
            SpdNFeDataSetX.SetCampo(("finNFe_B25=1")); //Finalidade da NFe (1-Normal, 2-Complementar, 3-de Ajuste)
            SpdNFeDataSetX.SetCampo(("procEmi_B26=0")); //Identificador do Processo de emissão (0-Emissão da Nfe com Aplicativo do Contribuinte). Ver outras opções no manual da Receita.
            SpdNFeDataSetX.SetCampo(("verProc_B27=000")); //Versão do Aplicativo Emissor
        }

        private void DadosEmitente()
        {
            SpdNFeDataSetX.SetCampo(("CNPJ_C02=" + Funcoes.LimpaStr(Nota.Empresa.Cnpj))); // CNPJ do Emitente
            SpdNFeDataSetX.SetCampo(("xNome_C03=" + Nota.Empresa.Nome)); // Razao Social ou Nome do Emitente
            SpdNFeDataSetX.SetCampo(("xFant_C04=" + Nota.Empresa.Fantasia)); // Nome Fantasia do Emitente
            SpdNFeDataSetX.SetCampo(("xLgr_C06=" + Nota.Empresa.Endereco)); // Logradouro do Emitente
            SpdNFeDataSetX.SetCampo(("nro_C07=" + Nota.Empresa.Numero)); // Numero do Logradouro do Emitente
            
            if (!String.IsNullOrEmpty(Nota.Empresa.Complemento))
                SpdNFeDataSetX.SetCampo(("xCpl_C08=" + Nota.Empresa.Complemento)); //Complemento do emitente
            
            SpdNFeDataSetX.SetCampo(("xBairro_C09=" + Nota.Empresa.Bairro)); // Bairro do Emitente
            SpdNFeDataSetX.SetCampo(("cMun_C10=" + Nota.Empresa.CidadeIBGE)); // Código da Cidade do Emitente (Tabela do IBGE)
            SpdNFeDataSetX.SetCampo(("xMun_C11=" + Nota.Empresa.Cidade)); // Nome da Cidade do Emitente
            SpdNFeDataSetX.SetCampo(("UF_C12=" + Nota.Empresa.UF)); // SIGLA do Estado do Emitente (Tabela do IBGE)
            SpdNFeDataSetX.SetCampo(("CEP_C13=" + Funcoes.LimpaStr(Nota.Empresa.CEP))); // Cep do Emitente
            SpdNFeDataSetX.SetCampo(("cPais_C14=1058")); // Código do País do Emitente (Tabela BACEN)
            SpdNFeDataSetX.SetCampo(("xPais_C15=BRASIL")); // Nome do País do Emitente
            SpdNFeDataSetX.SetCampo(("fone_C16=" + Funcoes.LimpaStr(Nota.Empresa.Telefone))); // Fone do Emitente
            SpdNFeDataSetX.SetCampo(("IE_C17=" + Funcoes.LimpaStr(Nota.Empresa.Inscricao))); // Inscrição Estadual do Emitente
            if (Nota.PessoaCidadeIBGE != "9999999" && Nota.Empresa.TipoST == TipoST.Substituto)
            {
                SpdNFeDataSetX.SetCampo(("IEST_C18=" + Funcoes.LimpaStr(Nota.Empresa.Inscricao))); // Inscrição Estadual do Substituto Tributário Emitente
            }
        }

        private void DadosDestinatario()
        {
            string cnpjcpf = Funcoes.LimpaStr(Nota.PessoaCNPJCPF);
            if (Nota.PessoaCidadeIBGE == "9999999")
            {
                SpdNFeDataSetX.SetCampo(("CNPJ_E02=")); // CNPJ do Destinatário
                SpdNFeDataSetX.SetCampo(("IE_E17=")); // Inscrição Estadual do Destinatário
                SpdNFeDataSetX.SetCampo(("xMun_E11=EXTERIOR")); // Nome da Cidade do Destinatário
                SpdNFeDataSetX.SetCampo(("UF_E12=EX")); // Sigla do Estado do Destinatário
                SpdNFeDataSetX.SetCampo(("cPais_E14=" + Nota.Cliente.PaisIBGE));
                SpdNFeDataSetX.SetCampo(("xPais_E15=" + Nota.Cliente.Pais)); // Nome do País do Destinatário
                
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("xMun_E11=" + Nota.PessoaCidade)); // Nome da Cidade do Destinatário
                SpdNFeDataSetX.SetCampo(("UF_E12=" + Nota.PessoaUF)); // Sigla do Estado do Destinatário
                SpdNFeDataSetX.SetCampo(("cPais_E14=1058")); // Código do Pais do Destinatário (Tabela do BACEN)
                SpdNFeDataSetX.SetCampo(("xPais_E15=BRASIL")); // Nome do País do Destinatário

                if (cnpjcpf.Length > 11)
                {
                    SpdNFeDataSetX.SetCampo(("CNPJ_E02=" + cnpjcpf)); // CNPJ do Destinatário
                    SpdNFeDataSetX.SetCampo(("IE_E17=" + Funcoes.LimpaStr(Nota.PessoaInscRG))); // Inscrição Estadual do Destinatário
                }
                else
                {
                    SpdNFeDataSetX.SetCampo(("CPF_E03=" + cnpjcpf)); // CPF do Destinatário
                    SpdNFeDataSetX.SetCampo(("CNPJ_E02=null")); // CNPJ do Destinatário
                    SpdNFeDataSetX.SetCampo(("IE_E17=" + (String.IsNullOrEmpty(Nota.PessoaInscRG) == true ? null : Nota.PessoaInscRG))); // Inscrição Estadual do Destinatário
                }
            }
            SpdNFeDataSetX.SetCampo(("xNome_E04=" + Nota.PessoaNome)); // Razao social ou Nome do Destinatário
            SpdNFeDataSetX.SetCampo(("xLgr_E06=" + Nota.PessoaEndereco)); // Logradouro do Destinatario
            SpdNFeDataSetX.SetCampo(("nro_E07=" + Nota.PessoaNumero)); // Numero do Logradouro do Destinatario
            if (!String.IsNullOrEmpty(Nota.Cliente.Complemento))
                {
                    SpdNFeDataSetX.SetCampo(("xCpl_E08=" + Nota.Cliente.Complemento));
                }
            SpdNFeDataSetX.SetCampo(("xBairro_E09=" + Nota.PessoaBairro)); // Bairro do Destinatario
            SpdNFeDataSetX.SetCampo(("cMun_E10=" + Nota.PessoaCidadeIBGE)); // Código do Município do Destinatário (Tabela IBGE)
            SpdNFeDataSetX.SetCampo(("CEP_E13=" + Funcoes.LimpaStr(Nota.PessoaCEP))); // Cep do Destinatário
            SpdNFeDataSetX.SetCampo(("fone_E16=" + Funcoes.LimpaStr(Nota.PessoaTelefone))); // Fone do Destinatário
        }

        private void DadosItem(INotaItem aNotaItem)
        {
            //Importante: Respeitar a ordem sequencial do campo nItem_H02, quando gerar os itens
            SpdNFeDataSetX.SetCampo(("nItem_H02=" + aNotaItem.Sequencia.ToString())); // Número do Item da NFe (1 até 990)
            //Dados do Produto Vend Subido
            SpdNFeDataSetX.SetCampo(("cProd_I02=" + aNotaItem.ProdutoCodigo.ToString())); //Código do PRoduto ou Serviço
            SpdNFeDataSetX.SetCampo(("cEAN_I03=")); // EAN do Produto
            SpdNFeDataSetX.SetCampo(("xProd_I04=" + aNotaItem.ProdutoNome)); // Descrição do PRoduto
            SpdNFeDataSetX.SetCampo(("NCM_I05=" + aNotaItem.ProdutoNCM)); // Código do NCM - informar de acordo com o Tabela oficial do NCM
            SpdNFeDataSetX.SetCampo(("CFOP_I08=" + aNotaItem.CFOPCodigo)); // CFOP incidente neste Item da NF
            SpdNFeDataSetX.SetCampo(("uCom_I09=" + aNotaItem.Unidade)); // Unidade de Medida do Item
            SpdNFeDataSetX.SetCampo(("qCom_I10=" + this.FormataTDEC_0804(aNotaItem.Quantidade))); // Quantidade Comercializada do Item
            SpdNFeDataSetX.SetCampo(("vUnCom_I10a=" + this.FormataTDEC_1204(aNotaItem.Valor))); // Valor Comercializado do Item
            SpdNFeDataSetX.SetCampo(("vProd_I11=" + this.FormataTDEC_1302(aNotaItem.Total))); // Valor Total Bruto do Item
            SpdNFeDataSetX.SetCampo(("cEANTrib_I12=")); // EAN Tributável do Item
            SpdNFeDataSetX.SetCampo(("uTrib_I13=" + aNotaItem.Unidade)); // Unidade de Medida Tributável do Item
            SpdNFeDataSetX.SetCampo(("qTrib_I14=" + this.FormataTDEC_0804(aNotaItem.Quantidade))); // Quantidade Tributável do Item
            SpdNFeDataSetX.SetCampo(("vUnTrib_I14a=" + this.FormataTDEC_1204(aNotaItem.Valor))); // Valor Tributável do Item

            // Aqui começam os Impostos Incidentes sobre o Item''''''''''''
            //Verificar Manual pois existe uma variação nos campos de acordo com Tipo de Tribucação ''

            //ICMS
            switch (aNotaItem.TAG_CST)
            {
                case "00":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                    SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                    SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                    break;
                case "10":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                    SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                    SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                    SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst)));
                    SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                    SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorIsentoICMS)));
                    break;
                case "20":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("pRedBC_N14=" + this.FormataTDEC_0302(aNotaItem.pRedBC_N14))); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                    SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                    SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                    break;
                case "30":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                    SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                    SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                    break;
                case "40":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    break;
                case "41":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    break;
                case "50":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    break;
                case "51":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    break;
                case "60":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                    break;
                case "70":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("pRedBC_N14=" + aNotaItem.pRedBC_N14.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                    SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                    SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                    SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                    SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                    break;
                case "90":
                    SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                    SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                    SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                    SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                    SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                    SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                    SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                    SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                    SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                    break;
            }

            //IPI
            if (aNotaItem.CST_Ipi == "00" || aNotaItem.CST_Ipi == "49" || aNotaItem.CST_Ipi == "50" || aNotaItem.CST_Ipi == "99")
            {
                
                //_spdNFeDataSetX.SetCampo(("clEnq_O02=00000"));
                //_spdNFeDataSetX.SetCampo(("CNPJProd_O03="+Funcoes.LimpaStr(objNota.PessoaCNPJCPF)));
                //_spdNFeDataSetX.SetCampo(("cSelo_O04=selo"));
                //_spdNFeDataSetX.SetCampo(("qSelo_O05=1"));
                
                SpdNFeDataSetX.SetCampo(("cEnq_O06=" + aNotaItem.cEnq_O06));
                SpdNFeDataSetX.SetCampo(("CST_O09=" + aNotaItem.CST_Ipi));
                SpdNFeDataSetX.SetCampo(("vBC_O10=" + this.FormataTDEC_1302(aNotaItem.vBC_O10)));
                SpdNFeDataSetX.SetCampo(("qUnid_O11="));
                SpdNFeDataSetX.SetCampo(("vUnid_O12="));
                SpdNFeDataSetX.SetCampo(("pIPI_O13=" + this.FormataTDEC_0302(aNotaItem.pIPI_O13)));
                SpdNFeDataSetX.SetCampo(("vIPI_O14=" + this.FormataTDEC_1302(aNotaItem.vIPI_O14)));
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("cEnq_O06=" + aNotaItem.cEnq_O06));
                SpdNFeDataSetX.SetCampo(("CST_O09=" + aNotaItem.CST_Ipi));
            }

            // PIS
            if (aNotaItem.CST_Pis == "04" || aNotaItem.CST_Pis == "06" || aNotaItem.CST_Pis == "07" || aNotaItem.CST_Pis == "08" || aNotaItem.CST_Pis == "09")
            {
                SpdNFeDataSetX.SetCampo(("CST_Q06=" + aNotaItem.CST_Pis));
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("CST_Q06=" + aNotaItem.CST_Pis)); // Codigo de Situacao Tributária - ver opções no Manual
                SpdNFeDataSetX.SetCampo(("vBC_Q07=" + this.FormataTDEC_1302(aNotaItem.vBC_Q07))); // Valor da Base de Cálculo do PIS
                SpdNFeDataSetX.SetCampo(("pPIS_Q08=" + this.FormataTDEC_0302(aNotaItem.pPIS_Q08))); // Alíquota em Percencual do PIS
                SpdNFeDataSetX.SetCampo(("vPIS_Q09=" + this.FormataTDEC_1302(aNotaItem.vPIS_Q09))); // Valor do PIS em Reais
            }

            // COFINS
            if (aNotaItem.CST_Pis == "04" || aNotaItem.CST_Pis == "06" || aNotaItem.CST_Pis == "07" || aNotaItem.CST_Pis == "08" || aNotaItem.CST_Pis == "09")
            {
                SpdNFeDataSetX.SetCampo(("CST_S06=" + aNotaItem.CST_Cofins));
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("CST_S06=" + aNotaItem.CST_Cofins)); // Código de Situacao Tributária - ver opções no Manual
                SpdNFeDataSetX.SetCampo(("vBC_S07=" + this.FormataTDEC_1302(aNotaItem.vBC_S07))); // Valor da Base de Cálculo do COFINS
                SpdNFeDataSetX.SetCampo(("pCOFINS_S08=" + this.FormataTDEC_0302(aNotaItem.pCOFINS_S08))); // Alíquota do COFINS em Percentual
                SpdNFeDataSetX.SetCampo(("vCOFINS_S11=" + this.FormataTDEC_1302(aNotaItem.vCOFINS_S11))); // Valor do COFINS em Reais
            }

            //Informações Adicionais
            if (aNotaItem.InfAdicionais != null)
            {
                SpdNFeDataSetX.SetCampo(("infAdProd_V01=" + aNotaItem.InfAdicionais));
            }

            if (aNotaItem.TextoLei != null)
                obs = obs + " " + aNotaItem.TextoLei.Trim();
        }

        private void DadosCobranca()
        {
            foreach (INotaParcela parcela in Nota.NotaParcelas)
            {
                SpdNFeDataSetX.IncluirCobranca();

                if (!String.IsNullOrEmpty(parcela.FaturaNumero))
                {
                    SpdNFeDataSetX.SetCampo(("nFat_Y03=" + parcela.FaturaNumero.ToString())); // Número da Farura
                    SpdNFeDataSetX.SetCampo(("vOrig_Y04=" + this.FormataTDEC_1302(parcela.FaturaValorOriginal))); // Valor Original da Fatura
                    SpdNFeDataSetX.SetCampo(("vLiq_Y06=" + this.FormataTDEC_1302(parcela.FaturaValorLiquido))); // Valor Líquido da Fatura
                }

                SpdNFeDataSetX.SetCampo(("nDup_Y08=" + parcela.Sequencia.ToString())); // Número da Duplicata
                SpdNFeDataSetX.SetCampo(("dVenc_Y09=" + parcela.Vencimento.ToString("yyyy-MM-dd"))); // Data de Vencimento da Duplicata
                SpdNFeDataSetX.SetCampo(("vDup_Y10=" + this.FormataTDEC_1302(parcela.Valor))); // Valor da Duplicata

                SpdNFeDataSetX.SalvarCobranca(); // Grava a Duplicata em questão.
            }
        }

        private void DadosTotalizadores()
        {
            
            SpdNFeDataSetX.SetCampo(("vBC_W03=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.BaseICMS)))); // Base de Cálculo do ICMS
            SpdNFeDataSetX.SetCampo(("vICMS_W04=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.ValorICMS)))); // Valor Total do ICMS
            SpdNFeDataSetX.SetCampo(("vBCST_W05=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.BaseICMSSubst)))); // Base de Cálculo do ICMS Subst. Tributária
            SpdNFeDataSetX.SetCampo(("vST_W06=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.ValorRetidoICMS)))); // Valor Total do ICMS Sibst. Tributária
            SpdNFeDataSetX.SetCampo(("vProd_W07=" + this.FormataTDEC_1302(Nota.TotalProduto))); // Valor Total de Produtos
            SpdNFeDataSetX.SetCampo(("vFrete_W08=" + this.FormataTDEC_1302(Nota.ValorFrete))); // Valor Total do Frete
            SpdNFeDataSetX.SetCampo(("vSeg_W09=0.00")); // Valor Total do Seguro
            SpdNFeDataSetX.SetCampo(("vDesc_W10=" + this.FormataTDEC_1302(Nota.ValorDesconto))); // Valor Total de Desconto
            SpdNFeDataSetX.SetCampo(("vII_W11=0.00")); // Valor Total do II
            SpdNFeDataSetX.SetCampo(("vIPI_W12=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.vIPI_O14)))); // Valor Total do IPI
            SpdNFeDataSetX.SetCampo(("vPIS_W13=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.vPIS_Q09)))); // Valor Toal do PIS
            SpdNFeDataSetX.SetCampo(("vCOFINS_W14=" + this.FormataTDEC_1302(Nota.NotaItems.Sum(a => a.vCOFINS_S11)))); // Valor Total do COFINS
            SpdNFeDataSetX.SetCampo(("vOutro_W15=0.00")); // OUtras Despesas Acessórias
            SpdNFeDataSetX.SetCampo(("vNF_W16=" + this.FormataTDEC_1302(Nota.TotalNota))); // Valor Total da NFe - Versão Trial só aceita NF até R$ 1.00

            //Verifica se possui observação
            String observacaoTotal = "";
            if (!String.IsNullOrEmpty(Nota.ObservacaoSistema))
                observacaoTotal += Nota.ObservacaoSistema.Trim();

            if (!String.IsNullOrEmpty(Nota.ObservacaoUsuario))
                observacaoTotal += " " + Nota.ObservacaoUsuario.Trim();

            SpdNFeDataSetX.SetCampo(("infCpl_Z03=" + observacaoTotal));
        }

        private void DadosTransporte()
        {
            SpdNFeDataSetX.SetCampo(("modFrete_X02=" + Nota.TipoFrete)); // Modalidade de Frete
            if (Nota.TransNome != null && Nota.TransNome != String.Empty)
            {
                SpdNFeDataSetX.SetCampo(("CNPJ_X04=" + Funcoes.LimpaStr(Nota.TransCNPJCPF))); // CNPJ do Transportador
                SpdNFeDataSetX.SetCampo(("xNome_X06=" + Nota.TransNome)); // Nome do Transportador
                SpdNFeDataSetX.SetCampo(("IE_X07=" + Nota.TransInscricao)); //  Inscrição estadual do Transportador
                SpdNFeDataSetX.SetCampo(("xEnder_X08=" + Nota.TransEndereco)); // End Subereço do Transportador
                SpdNFeDataSetX.SetCampo(("xMun_X09=" + Nota.TransCidade)); // Nome do Município do Transportador
                SpdNFeDataSetX.SetCampo(("UF_X10=" + Nota.TransUF)); // Sigla do Estado do Transportador
                // Dados do Veículo de Transporte '
                SpdNFeDataSetX.SetCampo(("placa_X19=" + Funcoes.LimpaStr(Nota.TransPlaca))); // Placa do Veículo
                SpdNFeDataSetX.SetCampo(("uf_X20=" + Nota.TransPlacaUF)); // Sigla do Estado da Placa do Veículo
                //_spdNFeDataSetX.SetCampo(("rntc_X21=123456")); // Registro nacional de Trasportador de Cargas (ANTT)
                // Dados da Carga Transportada
            }

            try
            {
                SpdNFeDataSetX.SetCampo(("qVol_X27=" + this.FormataINTEIRO(Convert.ToDecimal(Nota.VolumeQuant)))); // Quantidade de Volumes transportados
            }
            catch { SpdNFeDataSetX.SetCampo(("qVol_X27=")); }

            SpdNFeDataSetX.SetCampo(("esp_X28=" + Nota.VolumeEspecie)); // Espécie de Carga Transportada
            SpdNFeDataSetX.SetCampo(("marca_X29=" + Nota.VolumeMarca)); // MArca da Carga Transportada
            SpdNFeDataSetX.SetCampo(("nVol_X30=" + Nota.VolumeNumero)); // Numeração dos Volumes transportados
            SpdNFeDataSetX.SetCampo(("pesoL_X31=" + this.FormataTDEC_1203(Nota.VolumePesoLiquido))); // Peso Líquido
            SpdNFeDataSetX.SetCampo(("pesoB_X32=" + this.FormataTDEC_1203(Nota.VolumePesoBruto))); // Peso Bruto
        }

        private IDictionary<string, string> ValidaDadosNFe()
        {
            IDictionary<string, string> retorno = new Dictionary<string, string>();

            if (Nota.PessoaEndereco == "")
            {
                retorno.Add("Pessoa", "Pessoa sem endereço principal na Nota Fiscal.");
            }
            if (String.IsNullOrEmpty(Nota.PessoaNumero))
            {
                retorno.Add("Pessoa", "Pessoa sem número no campo endereço.");
            }

            return retorno;
        }

        private string FormataUltimoLog(int pTipoLog, string pLog)
        {
            string[] log;

            if (pTipoLog == 1) //LogEnvio
                log = SpdNFeX.UltimoLogEnvio.Split('\\');
            else //LogRecibo
                log = SpdNFeX.UltimoLogConsRecibo.Split('\\');

            return log[(log.Count() - 1)];
        }

        public override IDictionary<string, string> GerarNFe()
        {
            IDictionary<string, string> retorno = new Dictionary<string, string>();
            string aXmlNota = "";
            Nota.Status = "-1";
            
            if ((retorno = ValidaDadosNFe()).Count > 0)
            {
                throw new Exception("Nota não é validada");
            }

            aXmlNota = SpdNFeX.StatusDoServico();
            if ((retorno = VerificaStatusServico(aXmlNota)) != null)
            {
                throw new ServidorOfflineException(null, "O servidor do serviço está offline.");
            }
            
            this.SpdNFeDataSetX.DicionarioXML = @"Templates\vm30\Conversor\NFeDataSets.xml";
            this.SpdNFeDataSetX.Incluir();

            this.DadosNFe();
            this.DadosEmitente();
            this.DadosDestinatario();

            int seq = 0;
            foreach (INotaItem objNotaItem in Nota.NotaItems)
            {
                seq += 1;
                this.SpdNFeDataSetX.IncluirItem();
                this.DadosItem(objNotaItem);
                this.SpdNFeDataSetX.SalvarItem();
            }

            DadosCobranca();
            DadosTotalizadores();
            DadosTransporte();

            SpdNFeDataSetX.Salvar();

            aXmlNota = SpdNFeDataSetX.LoteNFe;
            Nota.ChaveNota = aXmlNota.Substring(aXmlNota.IndexOf("infNFe Id") + 14, 44);

            aXmlNota = SpdNFeX.AssinarNota(aXmlNota);

            aXmlNota = SpdNFeX.EnviarNF(Nota.Numero.ToString(), aXmlNota, false);
            string envioDaNota = aXmlNota;
            Nota.NumeroRecibo = TrataRetornoEnvioNumeroRecibo(aXmlNota);
            Nota.LogEnvio = FormataUltimoLog(1, SpdNFeX.UltimoLogEnvio);

            return envioDaNota.DesmembrarXml();
        }

        private string TrataRetornoEnvioNumeroRecibo(string xml)
        {
            Nota.UltimoXmlRecebido = xml;
            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xml))));
            string statusRetorno = (from noh in documentoXml.Root.Elements() where noh.Name.LocalName == "cStat" select noh.Value).Single<string>();

            if (statusRetorno != "103")
            {
                IDictionary<string, string> retorno = new Dictionary<string, string>();
                foreach (var item in documentoXml.Root.Elements())
                {
                    retorno.Add(item.Name.LocalName, item.Value);
                }

                Nota.Status = "0";
                string xMotivo  = (from noh in documentoXml.Root.Elements() where noh.Name.LocalName == "xMotivo" select noh.Value).Single<string>();
                Nota.StatusMotivo = "Campo inconsistente: " + CapturaCampoErrado(xMotivo);

                throw new XmlMalFormatadoException(retorno, "Ocorreram erros no envio da nota.");
            }
            else
            {
                IDictionary<string, string> retorno = new Dictionary<string, string>();
                Nota.Status = "1";
                return documentoXml.Root.Descendants().Where(x => x.Name.LocalName == "infRec")
                                                .Descendants().Where(x => x.Name.LocalName == "nRec")
                                                    .Single().Value;
            }
        }

        private IDictionary<string, string> AtribuiRetornoRecibo(string xml)
        {
            Nota.UltimoXmlRecebido = xml;
            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xml))));
            var noh = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "protNFe" select c).Single<XElement>();
            noh = (from c in noh.Elements() where c.Name.LocalName == "infProt" select c).Single<XElement>();

            IDictionary<string, string> retorno = documentoXml.DesmembrarXml();

            string cStat = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();
            Console.WriteLine("retorno do recibo: " + cStat);

            if (cStat == "100")
            {
                Nota.NumeroProtocolo = (from c in noh.Elements() where c.Name.LocalName == "nProt" select c.Value).Single<string>();
                Nota.ChaveNota = (from c in noh.Elements() where c.Name.LocalName == "chNFe" select c.Value).Single<string>();
                Nota.NumeroProtocolo = Nota.NumeroProtocolo;
                Nota.ModeloDocto = 55;
                Nota.Status = "2";
                Nota.LogRecibo = FormataUltimoLog(2, SpdNFeX.UltimoLogConsRecibo);
            }
            else
            {
                if (cStat == "204")
                {
                    Nota.Status = "4";
                }
                else
                {
                    Nota.Status = "0";
                    throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
                }
            }

            return retorno;
            
        }

        private string CapturaCampoErrado(string linha)
        {
            int ultimaAspa = linha.LastIndexOf('\'');
            int penultimaAspa = linha.Substring(0, ultimaAspa - 1).LastIndexOf('\'');

            return linha.Substring(penultimaAspa + 1, ultimaAspa - penultimaAspa);
            
        }

        private IDictionary<string, string> VerificaStatusServico(string xml)
        {
            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xml))));
            var noh = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "cStat" select c).Single<XElement>();

            if (noh.Value == "107")
                return null;
            else
            {
                IDictionary<string, string> retorno = new Dictionary<string, string>();
                retorno.Add("Serviço offline", "Serviço offline");
                return retorno;
            }
        }

        public override IDictionary<string, string> CancelarNFe(string _motivo, string _usuario)
        {
            string aXmlNota = SpdNFeX.CancelarNF(Nota.ChaveNota, Nota.NumeroProtocolo, _motivo);

            if (aXmlNota == null || aXmlNota == "")
            {
                throw new SemRespostaDoServidorException(null);
            }

            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlNota))));

            XElement nohInfCanc  = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "infCanc" select c).Single();
            string valorCStat = (from c in nohInfCanc.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();

            if (valorCStat == "101" || valorCStat == "218")
            {
                Nota.CancDt = DateTime.Today;
                Nota.CancMotivo = _motivo;
                Nota.CancUsuario = _usuario;
                Nota.UltimoXmlRecebido = aXmlNota;

                Nota.Status = "3";
            }
            else
            {
                throw new NFeException(aXmlNota.DesmembrarXml(), "Houve erro no cancelamento da nota.");
            }

            return documentoXml.DesmembrarXml();

        }

        public override string GeraXmlNota()
        {
            return "";
        }

        public int MontarObservacao(int pTipoNota, List<INotaItem> pListaNotaItem, out string pObservacaoSistema)
        {
            string obs = "";

            //Verificar se a configuração geral possui observação
            string obsCfg = Observacoes;

            if (!String.IsNullOrEmpty(obsCfg))
                obs = JuntarObservacao(obs, obsCfg);

            //Verificar se o tipo de movimentação possui observação
            //string obsTno = (from tno in db.TipoNotas
            //                 where tno.ID == pTipoNota
            //                 select tno.Observacao).First();
            //if (!String.IsNullOrEmpty(obsTno))
            //    obs = JuntarObservacao(obs, obsTno);

            //Percorrer todos os produtos da nota encontrando texto lei
            foreach (INotaItem ntai in pListaNotaItem)
            {
                if (String.IsNullOrEmpty(ntai.TextoLei))
                    continue;

                obs = JuntarObservacao(obs, ntai.TextoLei);
            }

            //Calcula a quantidade de caracter liberado para a observação do usuário
            pObservacaoSistema = obs.TrimEnd();
            return (500 - obs.Length);
        }

        private string JuntarObservacao(string pObservacao, string pTexto)
        {
            if (!pObservacao.Contains(pTexto))
                pObservacao = pObservacao.TrimEnd() + " " + pTexto.TrimEnd();

            if (pObservacao.Length > 500)
                pObservacao = pObservacao.Substring(0, 500);

            return pObservacao;
        }

        public override IDictionary<string, string> ConsultarNFe()
        {
            List<string> retorno = new List<string>();
            string aXmlNota = "";
            
            aXmlNota = SpdNFeX.ConsultarNF(Nota.ChaveNota);

            if (aXmlNota == null || aXmlNota == "")
            {
                throw new SemRespostaDoServidorException(null, "Não houve resposta do servidor na requisição de consulta.");
            }
            Nota.UltimoXmlRecebido = aXmlNota;
            return aXmlNota.DesmembrarXml();
        }

        public override IDictionary<string, string> ConsultarRecibo()
        {
            List<string> retorno = new List<string>();
            string aXmlNota = "";

            aXmlNota = SpdNFeX.ConsultarRecibo(Nota.NumeroRecibo);

            if (aXmlNota == null || aXmlNota == "")
            {
                throw new SemRespostaDoServidorException(null, "Não houve resposta do servidor no recebimento do recibo.");
            }
            return AtribuiRetornoRecibo(aXmlNota);
        }

        public override string InutilizarNFe(string _ano, string _serie, string _numeroInicio, string _numeroFim, string _justificativa)
        {
            List<string> retorno = new List<string>();
            string aXmlNota = "";

            aXmlNota = SpdNFeX.InutilizarNF("", _ano, Funcoes.LimpaStr(Empresa.Cnpj), "55", _serie, _numeroInicio, _numeroFim, _justificativa);

            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlNota))));
            var noh = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "infInut" select c).Single<XElement>();

            string valorRetorno = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c).Single<XElement>().Value;
            if (valorRetorno == "102")
            {
                return "";
            }
            else
            {
                string motivoErro = (from c in noh.Elements() where c.Name.LocalName == "xMotivo" select c).Single<XElement>().Value;
                return motivoErro;
            }
        }

        public override void GerarXmlPreDanfe()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, string> ResolveNfce()
        {
            throw new NotImplementedException();
        }

        public override string AlterarFormaDeEmissao()
        {
            throw new NotImplementedException();
        }
    }
}
