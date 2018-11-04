using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFeX;
using NFeDataSetX;
using System.Xml.XPath;
using cwkNotaFiscalEletronica.Interfaces;
using System.Reflection;
using System.IO;
using System.Xml.Linq;
using cwkNotaFiscalEletronica.Erros;
using System.Xml;
using cwkNotaFiscalEletronica.Modelo;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace cwkNotaFiscalEletronica
{
    class NotaFiscalEletronicaConsumidor60 : INotaFiscalEletronica
    {
        private string obs;

        private Int16 IndFinal { get; set; }
        private IndPres IndPres { get; set; }
        private bool BDevolucao { get; set; }
        #region  Manager e-Doc
        private ConfiguracaoManager ConfigManager { get; set; }
        private string GrupoManager { get; set; }
        private string UsuarioManager { get; set; }
        private string SenhaManager { get; set; }
#pragma warning disable CS0108 // "NotaFiscalEletronicaConsumidor60.FormaEmissao" oculta o membro herdado "INotaFiscalEletronica.FormaEmissao". Use a nova palavra-chave se foi pretendido ocultar.
        private TipoEmissao FormaEmissao { get; set; }
#pragma warning restore CS0108 // "NotaFiscalEletronicaConsumidor60.FormaEmissao" oculta o membro herdado "INotaFiscalEletronica.FormaEmissao". Use a nova palavra-chave se foi pretendido ocultar.
        #endregion

        public NotaFiscalEletronicaConsumidor60(TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string _diretorioPadrao,
                                       Int16 indFinal, IndPres indPres, bool bDevolucao, ConfiguracaoManager configManager)
            : base(_tipoServidor, _ambiente, _tipoCertificado, _diretorioPadrao)
        {
            IndFinal = indFinal;
            IndPres = indPres;
            BDevolucao = bDevolucao;
            ValidaConfigManagerEDoc(configManager);
            ConfigManager = configManager;
            FormaEmissao = _tipoServidor;
        }

        private static void ValidaConfigManagerEDoc(ConfiguracaoManager configManager)
        {
            if (String.IsNullOrEmpty(configManager.host))
            {
                throw new Exception("Endereço do manager E-Doc não foi informado. Verifique!");
            }
            if (String.IsNullOrEmpty(configManager.grupo))
            {
                throw new Exception("Grupo do manager E-Doc não foi informado. Verifique!");
            }
            if (String.IsNullOrEmpty(configManager.cnpj))
            {
                throw new Exception("CNPJ do emissor E-Doc não foi informado. Verifique!");
            }
            if (String.IsNullOrEmpty(configManager.usuario))
            {
                throw new Exception("Usuário do manager E-Doc não foi informado. Verifique!");
            }
            if (String.IsNullOrEmpty(configManager.senha))
            {
                throw new Exception("Senha do manager E-Doc não foi informada. Verifique!");
            }
        }

        public override void Iniciar()
        {
            obs = "";
            SpdNFeX.CNPJ = Funcoes.LimpaStr(Empresa.Cnpj);
            SpdNFeX.UF = Empresa.UF;
            switch (FormaEmissao)
            {
                case TipoEmissao.teNormal:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moNormal;
                    break;
                case TipoEmissao.teSVCAN:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moSVCAN;
                    break;
                case TipoEmissao.teSVCRS:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moSVCRS;
                    break;
                default:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moEPEC;
                    break;
            }
            SpdNFeX.DiretorioEsquemas = @"Esquemas\";
            SpdNFeX.DiretorioTemplates = @"Templates\";
            SpdNFeX.ArquivoServidoresHom = "nfeServidoresHom.ini";
            SpdNFeX.ArquivoServidoresProd = "nfeServidoresProd.ini";
            SpdNFeX.NomeCertificado = Empresa.Certificado;
            SpdNFeX.ModeloRetrato = @"Templates\vm60\Danfe\Retrato.rtm";
            SpdNFeX.ModeloPaisagem = @"Templates\vm60\Danfe\Paisagem.rtm";
            SpdNFeX.VersaoManual = "6.0";
            SpdNFeX.FraseContingencia = "DANFE em Contingencia";
            SpdNFeX.FraseHomologacao = "SEM VALOR FISCAL";

            /*
             * 16/02/2012
             * Aguardando retorno da TecnoSpeed sobre PINCODE/SmartCard.
             */

            //SpdNFeX.PINCODE = Empresa.PinNfe;

            if (Empresa.GMailAutenticacao)
                SpdNFeX.EmailAutenticacao = Empresa.GMailAutenticacao;
        }

        private void DadosNFe(FinalidadeNFe finalidadeNFe)
        {
            SpdNFeDataSetX.SetCampo(("Id_A03=")); //Calcula Automático. Essa linha é desnecessária
            SpdNFeDataSetX.SetCampo(("versao_A02=4.00")); //Versão do Layout que está utilizando

            SpdNFeDataSetX.SetCampo(("cUF_B02=" + Nota.Empresa.UFIBGE)); //Codigo da UF para o estado de SP (Emitente da NFe)
            SpdNFeDataSetX.SetCampo(("cNF_B03=" + Nota.Id.ToString().PadLeft(9, '0'))); //Código Interno do Sistema que está integrando com a NFe
            SpdNFeDataSetX.SetCampo(("natOp_B04=" + Nota.NotaItems.First().CFOPDescricao)); //Descrição da(s) CFOP(s) envolvidas nessa NFe
            //Forma de pagamento sempre será à vista.
            //SpdNFeDataSetX.SetCampo(("indPag_B05=0")); //Indicador da Forma de Pgto (0- a Vista, 1 a Prazo)    
            SpdNFeDataSetX.SetCampo(("mod_B06=" + Nota.ModeloDocto)); //Código do Modelo de Documento Fiscal
            SpdNFeDataSetX.SetCampo(("serie_B07=" + Nota.Serie)); //S érie do Documento
            SpdNFeDataSetX.SetCampo(("nNF_B08=" + Nota.Numero.ToString()));// + txtNumNF.Text)); //Número da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("dhEmi_B09=" + Nota.DtEmissao.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz"))); //Data e hora de Emissão da Nota Fiscal
            //SpdNFeDataSetX.SetCampo(("dhSaiEnt_B10=" + Nota.DtSaida.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz"))); //Data e hora de Saída ou Entrada da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("tpNF_B11=" + (Nota.TipoNota == TipoNotaEntSaida.Entrada ? 0 : 1).ToString())); //Tipo de Documento Fiscal (0-Entrada, 1-Saída)
            SpdNFeDataSetX.SetCampo(("cMunFG_B12=" + Nota.Empresa.CidadeIBGE)); //Código do Município, conforme Tabela do IBGE
            SpdNFeDataSetX.SetCampo(("tpImp_B21=" + ((int)Danfe).ToString())); //Tipo de Impressão da Danfe (1- Retrato , 2-Paisagem)
            SpdNFeDataSetX.SetCampo(("tpEmis_B22=" + ((int)FormaEmissao).ToString())); //Forma de Emissão da NFe (1 - Normal, 2 - FS, 3 - SCAN, 4 - DPEC, 5 - FS-DA, 6 - SVCAN, 7 - SVCRS);
            SpdNFeDataSetX.SetCampo(("cDV_B23= ")); //Calcula Automatico - Linha desnecessária já que o componente calcula o Dígito Verificador automaticamente e coloca no devido campo
            SpdNFeDataSetX.SetCampo(("tpAmb_B24=" + ((int)CwkAmbiente).ToString())); //Identificação do Ambiente (1- Producao, 2-Homologação)
            SpdNFeDataSetX.SetCampo(("finNFe_B25=" + ((int)finalidadeNFe))); //Finalidade da NFe (1-Normal, 2-Complementar, 3-de Ajuste, 4-Devolução)
            SpdNFeDataSetX.SetCampo(("procEmi_B26=0")); //Identificador do Processo de emissão (0-Emissão da Nfe com Aplicativo do Contribuinte). Ver outras opções no manual da Receita.
            SpdNFeDataSetX.SetCampo(("verProc_B27=000")); //Versão do Aplicativo Emissor
            if (SpdNFeX.ModoOperacao == ModoOperacaoNFe.moSVCRS)
            {
                //            if (SpdNFeX.ModoOperacao.Contains("SVC"))

                SpdNFeDataSetX.SetCampo(("dhCont_B28=" + Nota.Empresa.ContingenciaDataHora.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz"))); //Data e Hora ("AAAA-MM-DDTHH:MM:SSzzz") da entrada em contingencia
                SpdNFeDataSetX.SetCampo(("xJust_B29=" + Nota.Empresa.ContingenciaJustificativa)); //Motivo da entrada em contingencia
            }

            SpdNFeDataSetX.SetCampo(("idDest_B11a=" + Nota.idDest));
            SpdNFeDataSetX.SetCampo(("indFinal_B25a=" + IndFinal));
            SpdNFeDataSetX.SetCampo(("indPres_B25b=" + (int)IndPres));

            SpdNFeDataSetX.SetCampo(("UFSaidaPais_ZA02=" + Nota.ZA02_UFEmbarq)); //nome ad tag alterado de UFEmbarq para UFSaidaPais
            SpdNFeDataSetX.SetCampo(("xLocExporta_ZA03=" + Nota.ZA03_xLocEmbarq)); //nome ad tag alterado de xLocEmbarq para xLocExporta
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
            if (Nota.PessoaCidadeIBGE != "9999999" && Nota.Empresa.TipoST == TipoST.Substituto && Nota.NotaItems.Sum(a => a.ValorRetidoICMS) > 0)
            {
                //SpdNFeDataSetX.SetCampo(("IEST_C18=" + Funcoes.LimpaStr(Nota.Empresa.Inscricao))); // Inscrição Estadual do Substituto Tributário Emitente
                SpdNFeDataSetX.SetCampo(("IEST_C18=" + Funcoes.LimpaStr(Nota.PessoaInscRG))); // Inscrição Estadual do Substituto Tributário Emitente
            }
            SpdNFeDataSetX.SetCampo(("CRT_C21=" + (int)Nota.Empresa.TipoCRT)); // Código de Regime Tributário do Emitente
        }

        private void DadosDestinatario()
        {
            string cnpjcpf = Funcoes.LimpaStr(Nota.PessoaCNPJCPF);
            if (Nota.PessoaCidadeIBGE == "9999999")
            {
                SpdNFeDataSetX.SetCampo(("CNPJ_E02=")); // CNPJ do Destinatário
                SpdNFeDataSetX.SetCampo(("indIEDest_E16a=" + 9));//Indicador da IE do Destinatário
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

                    if (Nota.Cliente.bContribuinte)
                    {
                        SpdNFeDataSetX.SetCampo(("indIEDest_E16a=" + 1));//Indicador da IE do Destinatário
                        SpdNFeDataSetX.SetCampo(("IE_E17=" + Funcoes.LimpaStr(Nota.PessoaInscRG))); // Inscrição Estadual do Destinatário
                    }
                    else
                    {
                        SpdNFeDataSetX.SetCampo(("indIEDest_E16a=" + 9));
                        SpdNFeDataSetX.SetCampo(("IE_E17=" + Funcoes.LimpaStr(Nota.PessoaInscRG))); // Inscrição Estadual do Destinatário
                    }


                    if (Nota.PessoaSUFRAMA != null && Nota.PessoaSUFRAMA != "")
                    {
                        SpdNFeDataSetX.SetCampo(("ISUF_E18=" + Funcoes.LimpaStr(Nota.PessoaSUFRAMA))); //Inscrição Suframa do Destinatário
                    }
                }
                else
                {
                    SpdNFeDataSetX.SetCampo(("CPF_E03=" + cnpjcpf)); // CPF do Destinatário
                    SpdNFeDataSetX.SetCampo(("CNPJ_E02=null")); // CNPJ do Destinatário

                    if (Nota.Cliente.bContribuinte)
                    {
                        if (String.IsNullOrEmpty(Nota.PessoaInscRG))
                        {
                            SpdNFeDataSetX.SetCampo(("indIEDest_E16a=" + 1));//Indicador da IE do Destinatário
                            SpdNFeDataSetX.SetCampo(("IE_E17=" + null)); // Inscrição Estadual do Destinatário
                        }
                        else
                        {
                            SpdNFeDataSetX.SetCampo(("indIEDest_E16a=" + 2));//Indicador da IE do Destinatário
                            SpdNFeDataSetX.SetCampo(("IE_E17=" + Nota.PessoaInscRG)); // Inscrição Estadual do Destinatário
                        }
                    }
                    else
                    {
                        SpdNFeDataSetX.SetCampo(("indIEDest_E16a=" + 9));
                        SpdNFeDataSetX.SetCampo(("IE_E17=" + (String.IsNullOrEmpty(Nota.PessoaInscRG) == true ? null : Nota.PessoaInscRG))); // Inscrição Estadual do Destinatário
                    }
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
            SpdNFeDataSetX.SetCampo(("email_E19=" + Nota.PessoaEmail)); // Email

        }

        private void DadosItem(INotaItem aNotaItem)
        {
            //Importante: Respeitar a ordem sequencial do campo nItem_H02, quando gerar os itens
            SpdNFeDataSetX.SetCampo(("nItem_H02=" + aNotaItem.Sequencia.ToString())); // Número do Item da NFe (1 até 990)
            //Dados do Produto Vend Subido
            SpdNFeDataSetX.SetCampo(("cProd_I02=" + aNotaItem.ProdutoCodigo.ToString())); //Código do PRoduto ou Serviço
            //SpdNFeDataSetX.SetCampo(("cEAN_I03=" + aNotaItem.cEAN)); // EAN do Produto
            if ((aNotaItem.cEANTrib != null) && (aNotaItem.cEANTrib.Length > 8))
            {
                SpdNFeDataSetX.SetCampo(("cEAN_I03=" + aNotaItem.cEANTrib)); // EAN do Produto    
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("cEAN_I03=" + "SEM GTIN")); // EAN do Produto
            }
            SpdNFeDataSetX.SetCampo(("xProd_I04=" + aNotaItem.ProdutoNome)); // Descrição do PRoduto
            SpdNFeDataSetX.SetCampo(("NCM_I05=" + aNotaItem.ProdutoNCM)); // Código do NCM - informar de acordo com o Tabela oficial do NCM
            SpdNFeDataSetX.SetCampo(("CFOP_I08=" + aNotaItem.CFOPCodigo)); // CFOP incidente neste Item da NF
            SpdNFeDataSetX.SetCampo(("uCom_I09=" + aNotaItem.Unidade)); // Unidade de Medida do Item
            SpdNFeDataSetX.SetCampo(("qCom_I10=" + this.FormataTDEC_0804(aNotaItem.Quantidade))); // Quantidade Comercializada do Item
            SpdNFeDataSetX.SetCampo(("vUnCom_I10a=" + this.FormataTDEC_1204(aNotaItem.Valor))); // Valor Comercializado do Item
            SpdNFeDataSetX.SetCampo(("vProd_I11=" + this.FormataTDEC_1302(aNotaItem.Total))); // Valor Total Bruto do Item
            //SpdNFeDataSetX.SetCampo(("cEANTrib_I12=" + aNotaItem.cEANTrib)); // EAN Tributável do Item
            if ((aNotaItem.cEANTrib != null) && (aNotaItem.cEANTrib.Length > 8))
            {
                SpdNFeDataSetX.SetCampo(("cEANTrib_I12=" + aNotaItem.cEANTrib)); // EAN Tributável do Item
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("cEANTrib_I12=" + "SEM GTIN")); // EAN Tributável do Item
            }
            SpdNFeDataSetX.SetCampo(("uTrib_I13=" + aNotaItem.Unidade)); // Unidade de Medida Tributável do Item
            SpdNFeDataSetX.SetCampo(("qTrib_I14=" + this.FormataTDEC_0804(aNotaItem.Quantidade))); // Quantidade Tributável do Item
            SpdNFeDataSetX.SetCampo(("vUnTrib_I14a=" + this.FormataTDEC_1204(aNotaItem.Valor))); // Valor Tributável do Item
            SpdNFeDataSetX.SetCampo("indTot_I17b=1"); // Indica se valor do Item vProd entra no valor total da NF-e vProd
            SpdNFeDataSetX.SetCampo("nFCI_I70=" + aNotaItem.FCI); //FCI do produto

            if (aNotaItem.ValorFrete > 0)
                SpdNFeDataSetX.SetCampo(("vFrete_I15=" + this.FormataTDEC_1302(aNotaItem.ValorFrete)));

            if (aNotaItem.ValorSeguro > 0)
                SpdNFeDataSetX.SetCampo(("vSeg_I16=" + this.FormataTDEC_1302(aNotaItem.ValorSeguro)));

            if (aNotaItem.RAT_Desconto > 0)
                SpdNFeDataSetX.SetCampo(("vDesc_I17=" + this.FormataTDEC_1302(aNotaItem.RAT_Desconto)));

            if (aNotaItem.OutrasDespesas > 0)
                SpdNFeDataSetX.SetCampo(("vOutro_I17a=" + this.FormataTDEC_1302(aNotaItem.OutrasDespesas)));

            // Aqui começam os Impostos Incidentes sobre o Item''''''''''''
            //Verificar Manual pois existe uma variação nos campos de acordo com Tipo de Tribucação ''

            //ICMS
            if (Nota.Empresa.TipoCRT != EmpresaCRT.SimplesNacional)
            {
                switch (aNotaItem.TAG_CST)
                {
                    case "00":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                        SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMS))); // Alíquota do ICMS em Percentual
                        SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                        break;
                    case "10":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                        SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMS))); // Alíquota do ICMS em Percentual
                        SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst)));
                        SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        break;
                    case "20":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("pRedBC_N14=" + this.FormataTDEC_0302(aNotaItem.pRedBC_N14))); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                        SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual

                        SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("vICMSDeson_N27a=" + this.FormataTDEC_1302(aNotaItem.vICMSDeson))); // Valor do ICMS desonerado
                        SpdNFeDataSetX.SetCampo(("motDesICMS_N28=" + aNotaItem.motDesICMS)); // Motivo da desoneração do ICMS
                        break;
                    case "30":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));

                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        SpdNFeDataSetX.SetCampo(("vICMSDeson_N27a=" + this.FormataTDEC_1302(aNotaItem.vICMSDeson))); // Valor do ICMS desonerado
                        SpdNFeDataSetX.SetCampo(("motDesICMS_N28=" + aNotaItem.motDesICMS)); // Motivo da desoneração do ICMS
                        break;
                    case "40":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual

                        SpdNFeDataSetX.SetCampo(("vICMSDeson_N27a=" + this.FormataTDEC_1302(aNotaItem.vICMSDeson))); // Valor do ICMS desonerado
                        SpdNFeDataSetX.SetCampo(("motDesICMS_N28=" + aNotaItem.motDesICMS)); // Motivo da desoneração do ICMS
                        break;
                    case "41":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual

                        SpdNFeDataSetX.SetCampo(("vICMSDeson_N27a=" + this.FormataTDEC_1302(aNotaItem.vICMSDeson))); // Valor do ICMS desonerado
                        SpdNFeDataSetX.SetCampo(("motDesICMS_N28=" + aNotaItem.motDesICMS)); // Motivo da desoneração do ICMS
                        break;
                    case "50":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual

                        SpdNFeDataSetX.SetCampo(("vICMSDeson_N27a=" + this.FormataTDEC_1302(aNotaItem.vICMSDeson))); // Valor do ICMS desonerado
                        SpdNFeDataSetX.SetCampo(("motDesICMS_N28=" + aNotaItem.motDesICMS)); // Motivo da desoneração do ICMS
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
                        SpdNFeDataSetX.SetCampo(("pRedBC_N14=" + this.FormataTDEC_0302(aNotaItem.pRedBC_N14))); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                        SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                        SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
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
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        break;
                }
            }
            else //Empresa Simples
            {
                switch (aNotaItem.TAG_CST)
                {
                    case "101":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        SpdNFeDataSetX.SetCampo(("pCredSN_N29=" + this.FormataTDEC_0302(aNotaItem.pCredSN_N29)));
                        SpdNFeDataSetX.SetCampo(("vCredICMSSN_N30=" + this.FormataTDEC_1302(aNotaItem.vCredICMSSN_N30)));
                        break;
                    case "102": goto case "400";
                    case "103": goto case "400";
                    case "201":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18));
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst)));
                        SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        SpdNFeDataSetX.SetCampo(("pCredSN_N29=" + this.FormataTDEC_0302(aNotaItem.pCredSN_N29)));
                        SpdNFeDataSetX.SetCampo(("vCredICMSSN_N30=" + this.FormataTDEC_1302(aNotaItem.vCredICMSSN_N30)));
                        break;
                    case "203": goto case "202";
                    case "202":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18));
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst)));
                        SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        break;
                    case "300": goto case "400";
                    case "400":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        break;
                    case "500":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));

                        if (aNotaItem.ValorIsentoICMS > 0)
                        {
                            SpdNFeDataSetX.SetCampo(("vBCSTRet_N26=" + this.FormataTDEC_1302(aNotaItem.ValorIsentoICMS)));    
                        }
                        if (aNotaItem.ValorRetidoICMS > 0)
                        {
                            SpdNFeDataSetX.SetCampo(("vICMSSTRet_N27=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        }
                        
                        
                        break;
                    case "900":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString()));
                        SpdNFeDataSetX.SetCampo(("pRedBC_N14=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBC_N14)));
                        SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS)));
                        SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal)));
                        SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS)));
                        SpdNFeDataSetX.SetCampo(("modBCST_N18=" + aNotaItem.modBCST_N18.ToString()));
                        SpdNFeDataSetX.SetCampo(("pMVAST_N19=" + this.FormataTDEC_0302Opc(aNotaItem.pMVAST_N19)));
                        SpdNFeDataSetX.SetCampo(("pRedBCST_N20=" + this.FormataTDEC_0302Opc(aNotaItem.pRedBCST_N20)));
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst)));
                        SpdNFeDataSetX.SetCampo(("pICMSST_N22=" + this.FormataTDEC_0302(aNotaItem.pICMSST_N22)));
                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        SpdNFeDataSetX.SetCampo(("pCredSN_N29=" + this.FormataTDEC_0302(aNotaItem.pCredSN_N29)));
                        SpdNFeDataSetX.SetCampo(("vCredICMSSN_N30=" + this.FormataTDEC_1302(aNotaItem.vCredICMSSN_N30)));
                        break;

                }
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

            if (Nota.EnviaTagTotalImposto)
                SpdNFeDataSetX.SetCampo(("vTotTrib_M02=" + this.FormataTDEC_1302(aNotaItem.TotalImpostos)));

            //Informações Adicionais
            if (aNotaItem.InfAdicionais != null)
            {
                SpdNFeDataSetX.SetCampo(("infAdProd_V01=" + aNotaItem.InfAdicionais));
            }

            if (aNotaItem.TextoLei != null)
                obs = obs + " " + aNotaItem.TextoLei.Trim();
        }

        private void DadosTotalizadores(IList<INotaItem> notaItems, decimal totalProduto, decimal totalNota)
        {
            SpdNFeDataSetX.SetCampo(("vICMSDeson_W04a=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vICMSDeson))));
            //SpdNFeDataSetX.SetCampo(("vFCP_W04a=" + "0.00")); // Valor Total do FCP
            SpdNFeDataSetX.SetCampo(("vBC_W03=" + this.FormataTDEC_1302(notaItems.Sum(a => a.BaseICMS)))); // Base de Cálculo do ICMS
            SpdNFeDataSetX.SetCampo(("vICMS_W04=" + this.FormataTDEC_1302(notaItems.Sum(a => a.ValorICMS)))); // Valor Total do ICMS
            SpdNFeDataSetX.SetCampo(("vFCPUFDest_w04c=" + "0.00")); // Valor Total do ICMS
            SpdNFeDataSetX.SetCampo(("vICMSUFDest_W04e=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vICMSUFDest_NA15)))); // Valor Total do ICMS Destino
            SpdNFeDataSetX.SetCampo(("vICMSUFRemet_W04g=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vICMSUFRemet_NA17)))); // Valor Total do ICMS Origem
            SpdNFeDataSetX.SetCampo(("vFCP_W04h=" + "0.00")); // Valor Total do FCP
            SpdNFeDataSetX.SetCampo(("vFCPST_W06a=" + "0.00")); // Valor Total do FCP
            SpdNFeDataSetX.SetCampo(("vFCPSTRet_W06b=" + "0.00")); // Valor Total do FCP retido anteriormente por Substituição Tributária
            SpdNFeDataSetX.SetCampo(("vIPIDevol_W12a=" + "0.00")); // Valor Total do IPI devolvido

            SpdNFeDataSetX.SetCampo(("vBCST_W05=" + this.FormataTDEC_1302(notaItems.Sum(a => a.BaseICMSSubst)))); // Base de Cálculo do ICMS Subst. Tributária
            SpdNFeDataSetX.SetCampo(("vST_W06=" + this.FormataTDEC_1302(notaItems.Sum(a => a.ValorRetidoICMS)))); // Valor Total do ICMS Sibst. Tributária
            SpdNFeDataSetX.SetCampo(("vProd_W07=" + this.FormataTDEC_1302(totalProduto))); // Valor Total de Produtos

            SpdNFeDataSetX.SetCampo(("vFrete_W08=" + this.FormataTDEC_1302(Nota.ValorFrete))); // Valor Total do Frete

            SpdNFeDataSetX.SetCampo(("vSeg_W09=" + this.FormataTDEC_1302(Nota.ValorSeguro))); // Valor Total do Seguro

            SpdNFeDataSetX.SetCampo(("vDesc_W10=" + this.FormataTDEC_1302(Nota.ValorDesconto))); // Valor Total de Desconto

            SpdNFeDataSetX.SetCampo(("vII_W11=0.00")); // Valor Total do II
            SpdNFeDataSetX.SetCampo(("vIPI_W12=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vIPI_O14)))); // Valor Total do IPI
            SpdNFeDataSetX.SetCampo(("vPIS_W13=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vPIS_Q09)))); // Valor Toal do PIS
            SpdNFeDataSetX.SetCampo(("vCOFINS_W14=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vCOFINS_S11)))); // Valor Total do COFINS
            SpdNFeDataSetX.SetCampo(("vOutro_W15=" + this.FormataTDEC_1302(Nota.OutrasDespesas))); // OUtras Despesas Acessórias

            SpdNFeDataSetX.SetCampo(("vNF_W16=" + this.FormataTDEC_1302(totalNota))); // Valor Total da NFe - Versão Trial só aceita NF até R$ 1.00

            if (Nota.EnviaTagTotalImposto)
                SpdNFeDataSetX.SetCampo(("vTotTrib_W16a=" + this.FormataTDEC_1302(notaItems.Sum(a => a.TotalImpostos)))); //Valor total dos impostos de acordo com a NT 2013-003

            SpdNFeDataSetX.SetCampo(("vII_W11=" + FormataTDEC_1302(Nota.W11_vII))); //Valor Total Imposto de Importação

            //Verifica se possui observação
            String observacaoTotal = "";
            if (!String.IsNullOrEmpty(Nota.ObservacaoSistema))
                observacaoTotal += Nota.ObservacaoSistema.Trim();

            if (!String.IsNullOrEmpty(Nota.ObservacaoUsuario))
                observacaoTotal += " " + Nota.ObservacaoUsuario.Trim();

            SpdNFeDataSetX.SetCampo(("infCpl_Z03=" + observacaoTotal));
        }
        //{
        //    SpdNFeDataSetX.SetCampo(("vICMSDeson_W04a=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vICMSDeson))));
        //    SpdNFeDataSetX.SetCampo(("vBC_W03=" + this.FormataTDEC_1302(notaItems.Sum(a => a.BaseICMS)))); // Base de Cálculo do ICMS
        //    SpdNFeDataSetX.SetCampo(("vICMS_W04=" + this.FormataTDEC_1302(notaItems.Sum(a => a.ValorICMS)))); // Valor Total do ICMS
        //    SpdNFeDataSetX.SetCampo(("vBCST_W05=" + this.FormataTDEC_1302(notaItems.Sum(a => a.BaseICMSSubst)))); // Base de Cálculo do ICMS Subst. Tributária
        //    SpdNFeDataSetX.SetCampo(("vST_W06=" + this.FormataTDEC_1302(notaItems.Sum(a => a.ValorRetidoICMS)))); // Valor Total do ICMS Sibst. Tributária
        //    SpdNFeDataSetX.SetCampo(("vProd_W07=" + this.FormataTDEC_1302(totalProduto))); // Valor Total de Produtos

        //    SpdNFeDataSetX.SetCampo(("vFrete_W08=" + this.FormataTDEC_1302(Nota.ValorFrete))); // Valor Total do Frete

        //    SpdNFeDataSetX.SetCampo(("vSeg_W09=" + this.FormataTDEC_1302(Nota.ValorSeguro))); // Valor Total do Seguro

        //    SpdNFeDataSetX.SetCampo(("vDesc_W10=" + this.FormataTDEC_1302(Nota.ValorDesconto))); // Valor Total de Desconto

        //    SpdNFeDataSetX.SetCampo(("vII_W11=0.00")); // Valor Total do II
        //    SpdNFeDataSetX.SetCampo(("vIPI_W12=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vIPI_O14)))); // Valor Total do IPI
        //    SpdNFeDataSetX.SetCampo(("vPIS_W13=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vPIS_Q09)))); // Valor Toal do PIS
        //    SpdNFeDataSetX.SetCampo(("vCOFINS_W14=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vCOFINS_S11)))); // Valor Total do COFINS
        //    SpdNFeDataSetX.SetCampo(("vOutro_W15=" + this.FormataTDEC_1302(Nota.OutrasDespesas))); // OUtras Despesas Acessórias

        //    SpdNFeDataSetX.SetCampo(("vNF_W16=" + this.FormataTDEC_1302(totalNota))); // Valor Total da NFe - Versão Trial só aceita NF até R$ 1.00

        //    if (Nota.EnviaTagTotalImposto)
        //        SpdNFeDataSetX.SetCampo(("vTotTrib_W16a=" + this.FormataTDEC_1302(notaItems.Sum(a => a.TotalImpostos)))); //Valor total dos impostos de acordo com a NT 2013-003

        //    SpdNFeDataSetX.SetCampo(("vII_W11=" + FormataTDEC_1302(Nota.W11_vII))); //Valor Total Imposto de Importação

        //    //Verifica se possui observação
        //    String observacaoTotal = "";
        //    if (!String.IsNullOrEmpty(Nota.ObservacaoSistema))
        //        observacaoTotal += Nota.ObservacaoSistema.Trim();

        //    if (!String.IsNullOrEmpty(Nota.ObservacaoUsuario))
        //        observacaoTotal += " " + Nota.ObservacaoUsuario.Trim();

        //    SpdNFeDataSetX.SetCampo(("infCpl_Z03=" + observacaoTotal));
        //}

        private void DadosTransporte()
        {
            SpdNFeDataSetX.SetCampo(("modFrete_X02=" + Nota.TipoFrete)); // Modalidade de Frete
            if (Nota.TransNome != null && Nota.TransNome != String.Empty)
            {
                string auxCnpjCpf = Funcoes.LimpaStr(Nota.TransCNPJCPF);

                if (auxCnpjCpf.Length == 11)
                    SpdNFeDataSetX.SetCampo(("CPF_X05=" + auxCnpjCpf));
                else
                    SpdNFeDataSetX.SetCampo(("CNPJ_X04=" + auxCnpjCpf)); // CNPJ do Transportador

                SpdNFeDataSetX.SetCampo(("xNome_X06=" + Nota.TransNome)); // Nome do Transportador
                SpdNFeDataSetX.SetCampo(("IE_X07=" + Funcoes.LimpaStr(Nota.TransInscricao))); //  Inscrição estadual do Transportador
                SpdNFeDataSetX.SetCampo(("xEnder_X08=" + Nota.TransEndereco)); // End Subereço do Transportador
                SpdNFeDataSetX.SetCampo(("xMun_X09=" + Nota.TransCidade)); // Nome do Município do Transportador
                SpdNFeDataSetX.SetCampo(("UF_X10=" + Nota.TransUF)); // Sigla do Estado do Transportador
                // Dados do Veículo de Transporte '
                SpdNFeDataSetX.SetCampo(("placa_X19=" + Funcoes.LimpaStr(Nota.TransPlaca))); // Placa do Veículo
                SpdNFeDataSetX.SetCampo(("uf_X20=" + Nota.TransPlacaUF)); // Sigla do Estado da Placa do Veículo
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
            if (Nota.NotaComplementada == null)
                return GerarNotaNormal();
            else
                throw new Exception("NFC-e não implementada nota complementar");
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private IDictionary<string, string> GerarNotaNormal()
        {
            IDictionary<string, string> retorno = new Dictionary<string, string>();
            string aXmlNota = "";
            Nota.Status = "-1";

            if ((retorno = ValidaDadosNFe()).Count > 0)
            {
                throw new Exception("Nota não é validada");
            }

            aXmlNota = GeraXmlNota().Trim();

            aXmlNota = AlteraXMLParaNFCe(aXmlNota);

            SalvarXmlArquivo(aXmlNota, "UltimoXmlNFCeGerado.xml");

            ManagerEDoc ManagerEdocNFCe = new ManagerEDoc(ConfigManager);
            aXmlNota = FormataXMLEdocPadraoGestao(aXmlNota);
            Nota.XmlLogEnvNFe = aXmlNota;

            string strDadosRetornoEnvio = ManagerEdocNFCe.EnviarNFCe(aXmlNota);

            string envioDaNota = TrataRetornoNFCe(retorno, ManagerEdocNFCe, strDadosRetornoEnvio);
            return envioDaNota.DesmembrarXml();
        }

        private string TrataRetornoNFCe(IDictionary<string, string> retorno, ManagerEDoc NFCe, string strDadosRetornoEnvio)
        {
            string[] dadosRetornoEnvio = strDadosRetornoEnvio.Split(',');
            List<string> dadosNFCe = new List<string>();
            string xmlRetornoEnvioLoteNFCe = String.Empty;
            string envioDaNota = String.Empty;

            if (FormaEmissao == TipoEmissao.teNormal)
            {
                Dictionary<string, string> parm = new Dictionary<string, string>();
                parm["Campos"] = "nrecibo, nprotenvio, impresso, xmldestinatario, xml, xmotivo, xmlcancdestinatario, dtcancelamento";
                parm["Filtro"] = "chave=" + Nota.ChaveNota;
                parm["Visao"] = "TspdNFCe";

                string strgDadosNFCe = NFCe.Consultar(parm);
                dadosNFCe = strgDadosNFCe.Split(',').ToList();

                if (dadosNFCe.Count > 1)
                    xmlRetornoEnvioLoteNFCe = CriaXMLRetornoEnvioLoteNFCe(dadosNFCe[0], dadosNFCe[1], dadosRetornoEnvio[2], dadosNFCe[5]);
                else
                    xmlRetornoEnvioLoteNFCe = CriaXMLRetornoEnvioLoteNFCe("", "", dadosRetornoEnvio[1].Contains("EspdManNFCe") == true ? dadosRetornoEnvio[1] : dadosRetornoEnvio[2],
                        dadosRetornoEnvio[1].Contains("EspdManNFCe") == true ? dadosRetornoEnvio[2] : dadosRetornoEnvio[3]);

                envioDaNota = xmlRetornoEnvioLoteNFCe;
                Nota.XmlLogRecNFe = xmlRetornoEnvioLoteNFCe;
                Nota.UltimoXmlRecebido = Nota.XmlLogRecNFe;
                if (!dadosRetornoEnvio[0].Contains("EXCEPTION"))
                {
                    string cStat = dadosRetornoEnvio[2];
                    string dStat = dadosRetornoEnvio[3];
                    Nota.NumeroRecibo = dadosNFCe[0];
                    if ((cStat != "704")&&(dadosNFCe.Count >= 5))
                    {
                        Nota.StatusMotivo = dadosNFCe[5];
                    }
                    if (cStat == "100")
                    {
                        Nota.NumeroProtocolo = dadosNFCe[1];
                        Nota.Status = "2";
                        string xmlDestinatarioNFe = dadosNFCe[3];

                        xmlDestinatarioNFe = FormataXMLEdocPadraoGestao(xmlDestinatarioNFe);
                        Nota.XmlDestinatarioNFe = xmlDestinatarioNFe;
                        if (dadosNFCe[2].Contains("S"))
                        {
                            Nota.bImpressa = true;
                        }
                        else
                        {
                            Nota.bImpressa = false;
                        }

                    }
                    else
                    {
                        if (cStat == "204")
                        {
                            Nota.Status = "4";
                        }
                        else if (dStat.Contains("NF-e está denegada") || dStat.Contains("Uso Denegado"))
                        {
                            Nota.Status = "7";
                        }
                        else if (dStat.Contains("Cancelamento"))
                        {
                            string motivo = String.Empty;
                            int inicio = dadosNFCe[6].IndexOf("<xJust>");
                            int fim = dadosNFCe[6].IndexOf("</xJust>");
                            if (inicio >= 0 && fim >= 0)
                            {
                                motivo = dadosNFCe[6].Substring(inicio + 7, fim - inicio - 7);
                            }

                            Nota.CancDt = Convert.ToDateTime(dadosNFCe[7]).Date;
                            Nota.CancMotivo = motivo;

                            Nota.Status = "3";
                        }
                        else
                        {
                            Nota.Status = "0";
                            retorno.Add("Status: ", dadosRetornoEnvio[2]);
                            retorno.Add("Mensagem: ", dadosRetornoEnvio[3]);
                            Nota.StatusMotivo = dadosRetornoEnvio[3];
                            throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
                        }
                    }
                }
                else
                {
                    retorno.Add("Status: ", dadosRetornoEnvio[1]);
                    retorno.Add("Mensagem: ", dadosRetornoEnvio[2]);
                    Nota.StatusMotivo = dadosRetornoEnvio[2];
                    throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
                }
            }
            else
            {
                xmlRetornoEnvioLoteNFCe = CriaXMLRetornoEnvioLoteNFCe("", "", dadosRetornoEnvio[1], dadosRetornoEnvio[2]);

                envioDaNota = xmlRetornoEnvioLoteNFCe;
                Nota.XmlLogRecNFe = xmlRetornoEnvioLoteNFCe;
                Nota.UltimoXmlRecebido = Nota.XmlLogRecNFe;

                if (!dadosRetornoEnvio[0].Contains("EXCEPTION"))
                {
                    string cStat = dadosRetornoEnvio[1];
                    string dStat = dadosRetornoEnvio[2];
                    Nota.StatusMotivo = dadosRetornoEnvio[2];
                    if (cStat == "999")
                    {
                        Nota.Status = "8";

                    }
                }
                else
                {
                    retorno.Add("Status: ", dadosRetornoEnvio[1]);
                    retorno.Add("Mensagem: ", dadosRetornoEnvio[2]);
                    Nota.StatusMotivo = dadosRetornoEnvio[2];
                    throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
                }
            }
            return envioDaNota;
        }

        private static string FormataXMLEdocPadraoGestao(string xmlForaPadrao)
        {
            string valor = "";
            if (xmlForaPadrao.Length > 9)
            {
                valor = xmlForaPadrao.Substring(0, 9);
            }
            //xmlForaPadrao = Regex.Replace(xmlForaPadrao, @"<!\[CDATA\[>?", "");
            //xmlForaPadrao = Regex.Replace(xmlForaPadrao, @"\]+>+", "");
            //xmlForaPadrao = xmlForaPadrao.Replace(@"\\linedelimiter", "");
            if (valor.Contains("<![CDATA["))
            {
                xmlForaPadrao = xmlForaPadrao.Remove(0, 9);
                xmlForaPadrao = xmlForaPadrao.Remove(xmlForaPadrao.Length - 18, 18);
            }
            

            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xmlForaPadrao))));
            string xmlFormatado = documentoXml.ToString();
            return xmlFormatado;
        }

        private string CriaXMLRetornoEnvioLoteNFCe(string recibo, string protocolo, string status, string xMotivo)
        {
            #region Cria XML de Retorno do lote enviado da NFC-e (Manager e-doc não retorna esse xml, apenas uma string com alguns dos dados)

            TRetConsReciNFe retXml = new TRetConsReciNFe();

            retXml.tpAmb = Empresa.Ambiente;
            retXml.verAplic = "PR-v3_4_3";
            retXml.nRec = recibo;
            retXml.cStat = status;
            retXml.xMotivo = "Lote processado";
            retXml.cUF = TCodUfIBGE.Item41;
            DateTime dhRecbto = DateTime.Now;
            DateTimeOffset dateOffset = new DateTimeOffset(dhRecbto,
                                        TimeZoneInfo.Local.GetUtcOffset(dhRecbto));
            retXml.dhRecbto = dhRecbto.ToString("o");
            TProtNFe protNfe = new TProtNFe();
            protNfe.versao = "4.00";
            protNfe.infProt = new TProtNFeInfProt();
            protNfe.infProt.tpAmb = Empresa.Ambiente;
            protNfe.infProt.verAplic = "PR-v3_4_3";
            protNfe.infProt.chNFe = Nota.ChaveNota;
            protNfe.infProt.dhRecbto = dhRecbto.ToString("o");
            protNfe.infProt.cStat = status;
            protNfe.infProt.nProt = protocolo;
            protNfe.infProt.xMotivo = xMotivo;

            retXml.protNFe = new List<TProtNFe>();
            retXml.protNFe.Add(protNfe);

            XmlSerializer xsSubmit = new XmlSerializer(typeof(TRetConsReciNFe));

            string xml = String.Empty;
            using (StringWriter sww = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(sww))
            {
                xsSubmit.Serialize(writer, retXml);
                xml = sww.ToString();
            }
            #endregion
            return xml;
        }

        private string AlteraXMLParaNFCe(string aXmlNota)
        {
            //Altera o tipo de impressão
            var aStringBuilder = new StringBuilder(aXmlNota);
            int posicaoTagTpImp = aXmlNota.IndexOf("<tpImp>") + 7;
            aStringBuilder.Remove(posicaoTagTpImp, 1);
            aStringBuilder.Insert(posicaoTagTpImp, ((int)DanfeNFCe));
            aXmlNota = aStringBuilder.ToString();

            //Adiciona a forma de pagamento <pag>
            aStringBuilder = new StringBuilder(aXmlNota);
            int posicaoTagPag = aXmlNota.IndexOf("</transp>") + 9;
            //StringBuilder sb;
            //MontaDadosNotaParcela(out sb);
            //aStringBuilder.Insert(posicaoTagPag, sb.ToString());
           // aXmlNota = aStringBuilder.ToString();

            //Retira dados do destinatário.
            if (String.IsNullOrEmpty(Nota.PessoaCNPJCPF) || Convert.ToInt64(Nota.PessoaCNPJCPF.Replace(".", "").Replace("/", "").Replace("-", "")) == 0)
            {
                aXmlNota = removeTag(aXmlNota, "<dest>", "</dest>");
            }
            else
            {
                if (String.IsNullOrEmpty(Nota.PessoaEndereco) || Nota.PessoaEndereco.Substring(0, 1) == ".")
                {
                    aXmlNota = aXmlNota = removeTag(aXmlNota, "<enderDest>", "</enderDest>");
                }
            }


            return aXmlNota;
        }

        private string removeTag(string aXmlNota, string tag, string tagFechamento)
        {
            StringBuilder aStringBuilder = new StringBuilder(aXmlNota);
            int posicaoTagDestIni = aXmlNota.IndexOf(tag);
            int posicaoTagDestFin = aXmlNota.IndexOf(tagFechamento) + tagFechamento.Length;
            aStringBuilder.Remove(posicaoTagDestIni, posicaoTagDestFin - posicaoTagDestIni);
            return aStringBuilder.ToString();
        }

        private void MontaDadosNotaParcela(out StringBuilder sb)
        {
            sb = new StringBuilder();
            foreach (var parcela in Nota.NotaParcelas)
            {
                sb.Append("<pag>");
                sb.Append("<tPag>");
                sb.Append(this.FormataTDEC_1302(parcela.FormaPagamento));
                sb.Append("</tPag>");
                sb.Append("<vPag>");
                sb.Append(this.FormataTDEC_1302(parcela.Valor));
                sb.Append("</vPag>");
                sb.Append("</pag>");
            }
        }
        private void InformacoesPagamento()
        {
            if (Nota.NotaParcelas.Count == 0)
            {
                SpdNFeDataSetX.IncluirParte("YA");
                SpdNFeDataSetX.SetCampo(("tPag_YA02=" + "90"));
                SpdNFeDataSetX.SetCampo(("vPag_YA03=" + this.FormataTDEC_1302("0.00")));
                SpdNFeDataSetX.SalvarParte("YA");
            }
            else
            {

                foreach (INotaParcela formasPagamento in Nota.NotaParcelas)
                {
                    SpdNFeDataSetX.IncluirParte("YA");
                    SpdNFeDataSetX.SetCampo(("tPag_YA02=" + formasPagamento.FormaPagamento));
                    SpdNFeDataSetX.SetCampo(("vPag_YA03=" + this.FormataTDEC_1302(formasPagamento.Valor)));
                    SpdNFeDataSetX.SalvarParte("YA");
                }
            }
        }

        private void SalvarXmlArquivo(string xmlNota, string nomeArquivo)
        {
            StreamWriter stream = new StreamWriter(nomeArquivo);
            stream.Write(xmlNota);
            stream.Close();
        }

        public override string GeraXmlNota()
        {
            string aXmlNota;
            IniciarDataSet();

            try
            {
                this.SpdNFeDataSetX.Incluir();

                if (BDevolucao)
                {
                    this.DadosNFe(FinalidadeNFe.fnDevolucao);
                }
                else
                    this.DadosNFe(FinalidadeNFe.fnNormal);

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
                InformacoesPagamento();
                DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
                DadosTransporte();

                SpdNFeDataSetX.Salvar();

                aXmlNota = SpdNFeDataSetX.LoteNFe;

                if (FormaEmissao != TipoEmissao.teNormal)
                {
                    string xmlPrimeiraParte = aXmlNota.Substring(0, 49);
                    string xmlSegundaParte = aXmlNota.Substring(50);
                    aXmlNota = xmlPrimeiraParte + "9" + xmlSegundaParte;
                }

                Nota.ChaveNota = aXmlNota.Substring(aXmlNota.IndexOf("infNFe Id") + 14, 44);

                return aXmlNota;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public override void GerarXmlPreDanfe()
        {
            string aXmlNota;

            try
            {
                IniciarDataSet();

                this.SpdNFeDataSetX.Incluir();

                if (BDevolucao)
                {
                    this.DadosNFe(FinalidadeNFe.fnDevolucao);
                }
                else
                    this.DadosNFe(FinalidadeNFe.fnNormal);


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

                DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);

                SpdNFeDataSetX.Salvar();

                aXmlNota = SpdNFeDataSetX.LoteNFe;

                Nota.ChaveNota = aXmlNota.Substring(aXmlNota.IndexOf("infNFe Id") + 14, 44);

                SpdNFeX.PreverDanfe(aXmlNota, "");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private IDictionary<string, string> AtribuiRetornoRecibo(string xml)
        {
            Nota.UltimoXmlRecebido = xml;
            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(xml))));

            IDictionary<string, string> retorno = documentoXml.DesmembrarXml();

            try
            {
                var noh = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "protNFe" select c).Single<XElement>();
                noh = (from c in noh.Elements() where c.Name.LocalName == "infProt" select c).Single<XElement>();
                string cStat = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();
                string dStat = (from c in noh.Elements() where c.Name.LocalName == "xMotivo" select c.Value).Single();
                Console.WriteLine("retorno do recibo: " + cStat);

                if (cStat == "100")
                {
                    Nota.NumeroProtocolo = (from c in noh.Elements() where c.Name.LocalName == "nProt" select c.Value).Single<string>();
                    Nota.ChaveNota = (from c in noh.Elements() where c.Name.LocalName == "chNFe" select c.Value).Single<string>();
                    Nota.NumeroProtocolo = Nota.NumeroProtocolo;
                    Nota.ModeloDocto = 55;
                    Nota.Status = "2";
                    Nota.LogRecibo = FormataUltimoLog(2, SpdNFeX.UltimoLogConsRecibo);
                    Nota.XmlLogRecNFe = SpdNFeX.LerLog(SpdNFeX.DiretorioLog + Nota.LogRecibo);
                    Nota.XmlDestinatarioNFe = Funcoes.AbrirArquivo(SpdNFeX.DiretorioXmlDestinatario + Nota.ChaveNota + "-nfe.xml").Replace("UTF-8", "UTF-16");
                    Nota.bImpressa = true;
                }
                else
                {
                    if (cStat == "204")
                    {
                        Nota.Status = "4";
                    }
                    else if (dStat.Contains("NF-e está denegada") || dStat.Contains("Uso Denegado"))
                    {
                        Nota.Status = "7";
                    }
                    else
                    {
                        Nota.Status = "0";
                        throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
                    }
                }
            }
#pragma warning disable CS0168 // A variável "e" está declarada, mas nunca é usada
            catch (Exception e)
#pragma warning restore CS0168 // A variável "e" está declarada, mas nunca é usada
            {
                throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
            }

            return retorno;

        }

        public override IDictionary<string, string> CancelarNFe(String motivo, String usuario)
        {
            string aXmlNota = String.Empty;

            ManagerEDoc manager = new ManagerEDoc(ConfigManager);
            string retorno = manager.CancelarNfce(Nota.ChaveNota, motivo);
            string[] dadosNfce = retorno.Split(',');


            if (String.IsNullOrEmpty(retorno))
            {
                throw new SemRespostaDoServidorException(null);
            }

            aXmlNota = CriaXMLRetornoEnvioLoteNFCe(Nota.NumeroRecibo, dadosNfce[0], dadosNfce[1], dadosNfce[2]);

            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlNota))));

            if (dadosNfce[1] == "135")
            {
                Nota.CancDt = DateTime.Today;
                Nota.CancMotivo = motivo;
                Nota.CancUsuario = usuario;

                Nota.Status = "3";
            }

            Nota.UltimoXmlRecebido = aXmlNota;

            return documentoXml.DesmembrarXml();

        }

        public override IDictionary<string, string> ResolveNfce()
        {
            Dictionary<string, string> retorno = new Dictionary<string, string>();

            ManagerEDoc manager = new ManagerEDoc(ConfigManager);
            string retornoResolve = manager.ResolverNfce(Nota.ChaveNota);

            string resolve = TrataRetornoNFCe(retorno, manager, retornoResolve);

            return resolve.DesmembrarXml();
        }

        public int MontarObservacao(int pTipoNota, List<INotaItem> pListaNotaItem, out string pObservacaoSistema)
        {
            string obs = "";

            //Verificar se a configuração geral possui observação
            string obsCfg = Observacoes;

            if (!String.IsNullOrEmpty(obsCfg))
                obs = JuntarObservacao(obs, obsCfg);

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

            Nota.UltimoXmlRecebido = aXmlNota;

            if (String.IsNullOrEmpty(Nota.NumeroProtocolo))
                return AtribuiRetornoRecibo(aXmlNota);
            else
                return aXmlNota.DesmembrarXml();
        }

        public override string InutilizarNFe(string _ano, string _serie, string _numeroInicio, string _numeroFim, string _justificativa)
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS0108 // "NotaFiscalEletronicaConsumidor60.InutilizarNfce(ConfiguracaoManager, string, string, string, string, string)" oculta o membro herdado "INotaFiscalEletronica.InutilizarNfce(ConfiguracaoManager, string, string, string, string, string)". Use a nova palavra-chave se foi pretendido ocultar.
        public static string InutilizarNfce(ConfiguracaoManager configManager, string _ano, string _serie, string _numeroInicio, string _numeroFim, string _justificativa)
#pragma warning restore CS0108 // "NotaFiscalEletronicaConsumidor60.InutilizarNfce(ConfiguracaoManager, string, string, string, string, string)" oculta o membro herdado "INotaFiscalEletronica.InutilizarNfce(ConfiguracaoManager, string, string, string, string, string)". Use a nova palavra-chave se foi pretendido ocultar.
        {
            ManagerEDoc manager = new ManagerEDoc(configManager);
            string retornoInutilizar = manager.InutilizarNfce(_ano, _serie, _numeroInicio, _numeroFim, _justificativa);
            string[] dadosRetorno = retornoInutilizar.Split(',');

            if (dadosRetorno[1] == "102")
            {
                return dadosRetorno[1]; //cStat
            }
            else
            {
                return dadosRetorno[2]; //mensagem
            }
        }

        public override string AlterarFormaDeEmissao()
        {
            ManagerEDoc manager = new ManagerEDoc(ConfigManager);
            string retorno = manager.AlterarFormaDeEmissao(FormaEmissao.ToString());
            return retorno;
        }

        private void IniciarDataSet()
        {
            this.SpdNFeDataSetX = new spdNFeDataSetX();
            this.SpdNFeDataSetX.VersaoEsquema = Empresa.VersaoEsquema;
            if (Nota.PessoaCidadeIBGE == "9999999")
            {
                this.SpdNFeDataSetX.DicionarioXML = @"Templates\vm60\Conversor\NFeDataSets_Exportacao.xml";
            }
            else
            {
                this.SpdNFeDataSetX.DicionarioXML = @"Templates\vm60\Conversor\NFeDataSets.xml";
            }

        }

        private void EnviarCCe(string _chaveNFe, string _textoCce, string _dataHoraEvento, string _aOrgao, string _aIDLote, int aSequenciaEvento, string aFusoHorario)
        {
            this.SpdNFeX.EnviarCCe(_chaveNFe, _textoCce, _dataHoraEvento, _aOrgao, _aIDLote, aSequenciaEvento, aFusoHorario);
        }
    }
}
