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
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace cwkNotaFiscalEletronica
{
    class NotaFiscalEletronica60 : INotaFiscalEletronica
    {
        private string obs;

        private Int16 IndFinal  { get; set; }
        private IndPres IndPres { get; set; }
        private bool BDevolucao { get; set; }
        public string NotaStatusAnterior { get; set; }


        public NotaFiscalEletronica60(TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string _diretorioPadrao,
                                       Int16 indFinal, IndPres indPres, bool bDevolucao) : base(_tipoServidor, _ambiente, _tipoCertificado, _diretorioPadrao) 
        { 
            IndFinal = indFinal; 
            IndPres = indPres;
            BDevolucao = bDevolucao;
        }

        public override void Iniciar()
        {
            obs = "";
            SpdNFeX.CNPJ = Funcoes.LimpaStr(Empresa.Cnpj);
            SpdNFeX.UF = Empresa.UF;
            
            switch (FormaEmissao)
            {
                case TipoEmissao.Normal:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moNormal;
                    break;
                case TipoEmissao.SVCAN:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moSVCAN;
                    break;
                case TipoEmissao.SVCRS:
                    SpdNFeX.ModoOperacao = ModoOperacaoNFe.moSVCRS;
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

        private bool StatusWebService(string aXml, List<string> aListaRetorno)
        {
            if (VerificaCodigo(aXml, "107"))
            {
                aListaRetorno = null;
                return true;
            }
            else
            {
                aListaRetorno = TrataRetorno(aXml, "cStat", TipoRetorno.Motivo);
                return false;
            }
        }

        private bool VerificaCodigo(string aXml, string aCodigo)
        {
            int tem = 0;

            tem = (from r in TrataRetorno(aXml, "cStat", TipoRetorno.Codigo)
                   where r == aCodigo
                   select r).Count();

            if (tem > 0)
                return true;
            else
                return false;
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
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retorno;
        }

        private void DadosNFe(FinalidadeNFe finalidadeNFe)
        {
            SpdNFeDataSetX.SetCampo(("Id_A03=")); //Calcula Automático. Essa linha é desnecessária
            SpdNFeDataSetX.SetCampo(("versao_A02=4.00")); //Versão do Layout que está utilizando

            SpdNFeDataSetX.SetCampo(("cUF_B02=" + Nota.Empresa.UFIBGE)); //Codigo da UF para o estado de SP (Emitente da NFe)
            SpdNFeDataSetX.SetCampo(("cNF_B03=" + Nota.Id.ToString().PadLeft(9, '0'))); //Código Interno do Sistema que está integrando com a NFe
            SpdNFeDataSetX.SetCampo(("natOp_B04=" + Nota.NotaItems.First().CFOPDescricao)); //Descrição da(s) CFOP(s) envolvidas nessa NFe
            SpdNFeDataSetX.SetCampo(("mod_B06=" + Nota.ModeloDocto)); //Código do Modelo de Documento Fiscal
            SpdNFeDataSetX.SetCampo(("serie_B07=" + Nota.Serie)); //Série do Documento
            SpdNFeDataSetX.SetCampo(("nNF_B08=" + Nota.Numero.ToString()));// + txtNumNF.Text)); //Número da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("dhEmi_B09=" + Nota.DtEmissao.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz"))); //Data e hora de Emissão da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("dhSaiEnt_B10=" + Nota.DtSaida.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz"))); //Data e hora de Saída ou Entrada da Nota Fiscal
            SpdNFeDataSetX.SetCampo(("tpNF_B11=" + (Nota.TipoNota == TipoNotaEntSaida.Entrada ? 0 : 1).ToString())); //Tipo de Documento Fiscal (0-Entrada, 1-Saída)
            SpdNFeDataSetX.SetCampo(("cMunFG_B12=" + Nota.Empresa.CidadeIBGE)); //Código do Município, conforme Tabela do IBGE
            SpdNFeDataSetX.SetCampo(("tpImp_B21=" + ((int)Danfe).ToString())); //Tipo de Impressão da Danfe (1- Retrato , 2-Paisagem)

            if (NotaStatusAnterior != "8")
            {
                SpdNFeDataSetX.SetCampo(("tpEmis_B22=" + ((int)FormaEmissao).ToString())); //Forma de Emissão da NFe (1 - Normal, 2 - FS, 3 - SCAN, 4 - DPEC, 5 - FS-DA, 6 - SVCAN, 7 - SVCRS);
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("tpEmis_B22=" + "4")); //Forma de Emissão da NFe (1 - Normal, 2 - FS, 3 - SCAN, 4 - DPEC, 5 - FS-DA, 6 - SVCAN, 7 - SVCRS);
            }
            
            SpdNFeDataSetX.SetCampo(("cDV_B23= ")); //Calcula Automatico - Linha desnecessária já que o componente calcula o Dígito Verificador automaticamente e coloca no devido campo
            SpdNFeDataSetX.SetCampo(("tpAmb_B24=" + ((int)CwkAmbiente).ToString())); //Identificação do Ambiente (1- Producao, 2-Homologação)
            SpdNFeDataSetX.SetCampo(("finNFe_B25=" + ((int)finalidadeNFe))); //Finalidade da NFe (1-Normal, 2-Complementar, 3-de Ajuste, 4-Devolução)
            SpdNFeDataSetX.SetCampo(("procEmi_B26=0")); //Identificador do Processo de emissão (0-Emissão da Nfe com Aplicativo do Contribuinte). Ver outras opções no manual da Receita.
            SpdNFeDataSetX.SetCampo(("verProc_B27=000")); //Versão do Aplicativo Emissor

            if (FormaEmissao == TipoEmissao.DPEC || NotaStatusAnterior == "8")
            {
                SpdNFeDataSetX.SetCampo(("dhCont_B28=" + Nota.Empresa.ContingenciaDataHora.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz"))); //Data e Hora ("AAAA-MM-DDTHH:MM:SSzzz") da entrada em contingencia
                SpdNFeDataSetX.SetCampo(("xJust_B29=" + Nota.Empresa.ContingenciaJustificativa)); //Motivo da entrada em contingencia
            }

            SpdNFeDataSetX.SetCampo(("idDest_B11a=" + Nota.idDest));
            SpdNFeDataSetX.SetCampo(("indFinal_B25a=" + IndFinal));
            SpdNFeDataSetX.SetCampo(("indPres_B25b=" + (int)IndPres));

            SpdNFeDataSetX.SetCampo(("UFSaidaPais_ZA02=" + Nota.ZA02_UFEmbarq)); //nome ad tag alterado de UFEmbarq para UFSaidaPais
            SpdNFeDataSetX.SetCampo(("xLocExporta_ZA03=" + Nota.ZA03_xLocEmbarq)); //nome ad tag alterado de xLocEmbarq para xLocExporta

            if (finalidadeNFe == FinalidadeNFe.DevolucaoRetorno)
                GerarNotaReferenciada(Nota.NotaReferenciada);
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

            SpdNFeDataSetX.SetCampo(("CRT_C21=" + (int)Nota.Empresa.TipoCRT)); //Código de Regime Tributário do Emitente
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

        private void DadosEntrega()
        {
            if (Nota.EntregaEndreco != null)
            {
                string cnpjcpf = Funcoes.LimpaStr(Nota.PessoaCNPJCPF);
                if (cnpjcpf.Length == 11)
                    SpdNFeDataSetX.SetCampo(("CPF_G02a=" + cnpjcpf));
                else
                    SpdNFeDataSetX.SetCampo(("CNPJ_G02=" + cnpjcpf));

                SpdNFeDataSetX.SetCampo(("xLgr_G03=" + Nota.EntregaEndreco.Logradouro));
                SpdNFeDataSetX.SetCampo(("nro_G04=" + Nota.EntregaEndreco.Numero));
                SpdNFeDataSetX.SetCampo(("xCpl_G05=" + Nota.EntregaEndreco.Complemente));
                SpdNFeDataSetX.SetCampo(("xBairro_G06=" + Nota.EntregaEndreco.Bairro));
                SpdNFeDataSetX.SetCampo(("cMun_G07=" + Nota.EntregaEndreco.CidadeCodigoIBGE));
                SpdNFeDataSetX.SetCampo(("xMun_G08=" + Nota.EntregaEndreco.CidadeNome));
                SpdNFeDataSetX.SetCampo(("UF_G09=" + Nota.EntregaEndreco.UFSigla));
            }
        }

        private void DadosItem(INotaItem aNotaItem)
        {
            //Importante: Respeitar a ordem sequencial do campo nItem_H02, quando gerar os itens
            SpdNFeDataSetX.SetCampo(("nItem_H02=" + aNotaItem.Sequencia.ToString())); // Número do Item da NFe (1 até 990)
            //Dados do Produto Vend Subido
            SpdNFeDataSetX.SetCampo(("cProd_I02=" + aNotaItem.ProdutoCodigo.ToString())); //Código do PRoduto ou Serviço


            //if ((aNotaItem.cEAN != string.Empty)&&(aNotaItem.cEAN.Length == 8))
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

            if (!String.IsNullOrEmpty(aNotaItem.Cest))
                SpdNFeDataSetX.SetCampo(("CEST_I05c=" + aNotaItem.Cest));

            if (aNotaItem.indEscala_I05d == 1)
            {
                SpdNFeDataSetX.SetCampo(("indEscala_I05d=" + "N")); //Indicador de Escala Relevante
                SpdNFeDataSetX.SetCampo(("CNPJFab_I05e=" + aNotaItem.CNPJFab_I05e)); // CNPJ do Fabricante da Mercadoria
            }
            //else
            //{
            //    SpdNFeDataSetX.SetCampo(("indEscala_I05d=" + "S")); //Indicador de Escala Relevante
            //}

            SpdNFeDataSetX.SetCampo(("cBenef_I05f=" + aNotaItem.cBenef_I05f)); // Código de Benefício Fiscal na UF aplicado ao item
            SpdNFeDataSetX.SetCampo(("CFOP_I08=" + aNotaItem.CFOPCodigo)); // CFOP incidente neste Item da NF
            SpdNFeDataSetX.SetCampo(("uCom_I09=" + aNotaItem.Unidade)); // Unidade de Medida do Item
            SpdNFeDataSetX.SetCampo(("qCom_I10=" + this.FormataTDEC_0804(aNotaItem.Quantidade))); // Quantidade Comercializada do Item
            SpdNFeDataSetX.SetCampo(("vUnCom_I10a=" + this.FormataTDEC_1204(aNotaItem.Valor))); // Valor Comercializado do Item
            SpdNFeDataSetX.SetCampo(("vProd_I11=" + this.FormataTDEC_1302(aNotaItem.Total))); // Valor Total Bruto do Item

            if ((aNotaItem.cEANTrib != null) && (aNotaItem.cEANTrib.Length > 8))
            {
                SpdNFeDataSetX.SetCampo(("cEANTrib_I12=" + aNotaItem.cEANTrib)); // EAN Tributável do Item
            }
            else
            {
                SpdNFeDataSetX.SetCampo(("cEANTrib_I12=" + "SEM GTIN")); // EAN Tributável do Item
            }

            //SpdNFeDataSetX.SetCampo(("cEANTrib_I12=" + aNotaItem.cEANTrib)); // EAN Tributável do Item
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

                        if (aNotaItem.pFCP_N17b > 0)
                        {
                            SpdNFeDataSetX.SetCampo(("pFCP_N17b=" + this.FormataTDEC_0302(aNotaItem.pFCP_N17b))); // Percentual do Fundo de Combate à Pobreza
                            SpdNFeDataSetX.SetCampo(("vFCP_N17c=" + this.FormataTDEC_1302(aNotaItem.vFCP_N17c))); // Valor do Fundo de Combate à Pobreza    
                        }

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
                        SpdNFeDataSetX.SetCampo(("vBCFCP_N17a=" + this.FormataTDEC_1302(aNotaItem.vBCFCP_N17a))); // Valor da Base de Cálculo do FCP
                        SpdNFeDataSetX.SetCampo(("pFCP_N17b=" + this.FormataTDEC_0302(aNotaItem.pFCP_N17b))); // Percentual do Fundo de Combate à Pobreza
                        SpdNFeDataSetX.SetCampo(("vFCP_N17c=" + this.FormataTDEC_1302(aNotaItem.vFCP_N17c))); // Valor do Fundo de Combate à Pobreza
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária

                        break;
                    case "20":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("modBC_N13=" + aNotaItem.modBC_N13.ToString())); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("pRedBC_N14=" + this.FormataTDEC_0302(aNotaItem.pRedBC_N14))); // Modalidade de determinação da Base de Cálculo - ver Manual
                        SpdNFeDataSetX.SetCampo(("vBC_N15=" + this.FormataTDEC_1302(aNotaItem.BaseICMS))); // Valor da Base de Cálculo do ICMS
                        SpdNFeDataSetX.SetCampo(("pICMS_N16=" + this.FormataTDEC_0302(aNotaItem.AliqICMSNormal))); // Alíquota do ICMS em Percentual
                        SpdNFeDataSetX.SetCampo(("vICMS_N17=" + this.FormataTDEC_1302(aNotaItem.ValorICMS))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("vBCFCP_N17a=" + this.FormataTDEC_1302(aNotaItem.vBCFCP_N17a))); // Valor da Base de Cálculo do FCP
                        SpdNFeDataSetX.SetCampo(("pFCP_N17b=" + this.FormataTDEC_0302(aNotaItem.pFCP_N17b))); // Percentual do Fundo de Combate à Pobreza
                        SpdNFeDataSetX.SetCampo(("vFCP_N17c=" + this.FormataTDEC_1302(aNotaItem.vFCP_N17c))); // Valor do Fundo de Combate à Pobreza

                        IncluirTagICMSDeson(aNotaItem);
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
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária

                        IncluirTagICMSDeson(aNotaItem);
                        break;
                    case "40":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual

                        IncluirTagICMSDeson(aNotaItem);
                        break;
                    case "41":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("vBCSTRet_N26=" + this.FormataTDEC_1302(aNotaItem.ValorIsentoICMS))); // Valor do BC do ICMS ST retido na UF remetente
                        SpdNFeDataSetX.SetCampo(("vICMSSTRet_N27=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS))); // Valor do ICMS ST retido na UF

                        IncluirTagICMSDeson(aNotaItem);
                        break;
                    case "50":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual

                        IncluirTagICMSDeson(aNotaItem);
                        break;
                    case "51":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("vBCFCP_N17a=" + this.FormataTDEC_1302(aNotaItem.vBCFCP_N17a))); // Valor da Base de Cálculo do FCP
                        SpdNFeDataSetX.SetCampo(("pFCP_N17b=" + this.FormataTDEC_0302(aNotaItem.pFCP_N17b))); // Percentual do Fundo de Combate à Pobreza
                        SpdNFeDataSetX.SetCampo(("vFCP_N17c=" + this.FormataTDEC_1302(aNotaItem.vFCP_N17c))); // Valor do Fundo de Combate à Pobreza
                        break;
                    case "60":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11.ToString())); // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                        SpdNFeDataSetX.SetCampo(("CST_N12=" + aNotaItem.TAG_CST)); // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        SpdNFeDataSetX.SetCampo(("vBCST_N21=" + this.FormataTDEC_1302(aNotaItem.BaseICMSSubst))); // Valor do ICMS em Reais
                        SpdNFeDataSetX.SetCampo(("vICMSST_N23=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS)));
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pST_N26a=" + "0.00")); // Alíquota suportada pelo Consumidor Final
                        SpdNFeDataSetX.SetCampo(("vFCPSTRet_N27d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vBCSTRet_N26=" + this.FormataTDEC_1302(aNotaItem.ValorIsentoICMS))); // Valor do BC do ICMS ST retido na UF remetente
                        SpdNFeDataSetX.SetCampo(("vICMSSTRet_N27=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS))); // Valor do ICMS ST retido na UF
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
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vBCFCP_N17a=" + this.FormataTDEC_1302(aNotaItem.vBCFCP_N17a))); // Valor da Base de Cálculo do FCP
                        SpdNFeDataSetX.SetCampo(("pFCP_N17b=" + this.FormataTDEC_0302(aNotaItem.pFCP_N17b))); // Percentual do Fundo de Combate à Pobreza
                        SpdNFeDataSetX.SetCampo(("vFCP_N17c=" + this.FormataTDEC_1302(aNotaItem.vFCP_N17c))); // Valor do Fundo de Combate à Pobreza
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
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vBCFCP_N17a=" + this.FormataTDEC_1302(aNotaItem.vBCFCP_N17a))); // Valor da Base de Cálculo do FCP
                        SpdNFeDataSetX.SetCampo(("pFCP_N17b=" + this.FormataTDEC_0302(aNotaItem.pFCP_N17b))); // Percentual do Fundo de Combate à Pobreza
                        SpdNFeDataSetX.SetCampo(("vFCP_N17c=" + this.FormataTDEC_1302(aNotaItem.vFCP_N17c))); // Valor do Fundo de Combate à Pobreza
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
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + "0.00")); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + "0.00")); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + "0.00")); // Valor do FCP retido por Substituição Tributária

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
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária

                        break;
                    case "300": goto case "400";
                    case "400":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        break;
                    case "500":
                        SpdNFeDataSetX.SetCampo(("orig_N11=" + aNotaItem.orig_N11));
                        SpdNFeDataSetX.SetCampo(("CSOSN_N12a=" + aNotaItem.TAG_CST));
                        SpdNFeDataSetX.SetCampo(("vBCSTRet_N26=" + this.FormataTDEC_1302(aNotaItem.ValorIsentoICMS))); // Valor do BC do ICMS ST retido na UF remetente
                        SpdNFeDataSetX.SetCampo(("vICMSSTRet_N27=" + this.FormataTDEC_1302(aNotaItem.ValorRetidoICMS))); // Valor do ICMS ST retido na UF
                        SpdNFeDataSetX.SetCampo(("pST_N26a=" + "0.00")); // Alíquota suportada pelo Consumidor Final
                        SpdNFeDataSetX.SetCampo(("vBCFCPSTRet_N27a=" + "0.00")); // Valor da Base de Cálculo do FCP retido anteriormente por ST
                        SpdNFeDataSetX.SetCampo(("pFCPSTRet_N27b=" + "0.00")); // Percentual do FCP retido anteriormente por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPSTRet_N27d=" + "0.00")); // Valor do FCP retido por Substituição Tributária
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
                        SpdNFeDataSetX.SetCampo(("vBCFCPST_N23a=" + this.FormataTDEC_1302(aNotaItem.vBCFCPST_N23a))); // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("pFCPST_N23b=" + this.FormataTDEC_0302(aNotaItem.pFCPST_N23b))); // Percentual do FCP retido por Substituição Tributária
                        SpdNFeDataSetX.SetCampo(("vFCPST_N23d=" + this.FormataTDEC_1302(aNotaItem.vFCPST_N23d))); // Valor do FCP retido por Substituição Tributária



                        break;
                }
            }

            if ((Empresa.UF != Nota.PessoaUF) && (Nota.PessoaUF != "EX") && Convert.ToBoolean(IndFinal))
            {
                SpdNFeDataSetX.SetCampo(("vBCUFDest_NA03=" + FormataTDEC_1302(aNotaItem.BaseICMS)));
                SpdNFeDataSetX.SetCampo(("vBCFCPUFDest_NA04=" + "0.00")); // Valor da BC FCP na UF de destino
                SpdNFeDataSetX.SetCampo(("pFCPUFDest_NA05=" + "0.00"));
                SpdNFeDataSetX.SetCampo(("pICMSUFDest_NA07=" + FormataTDEC_1302(aNotaItem.AliqInterna)));
                SpdNFeDataSetX.SetCampo(("pICMSInter_NA09=" + FormataTDEC_1302(aNotaItem.pICMSInter)));
                SpdNFeDataSetX.SetCampo(("pICMSInterPart_NA11=" + PegarPercentualPartilha()));
                SpdNFeDataSetX.SetCampo(("vFCPUFDest_NA13=" + "0.00"));
                SpdNFeDataSetX.SetCampo(("vICMSUFDest_NA15=" + FormataTDEC_1302(aNotaItem.vICMSUFDest_NA15)));
                SpdNFeDataSetX.SetCampo(("vICMSUFRemet_NA17=" + FormataTDEC_1302(aNotaItem.vICMSUFRemet_NA17)));
            }

            //IPI
            if (aNotaItem.CST_Ipi == "00" || aNotaItem.CST_Ipi == "49" || aNotaItem.CST_Ipi == "50" || aNotaItem.CST_Ipi == "99")
            {
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

            if (Nota.EnviaTagTotalImposto)
                SpdNFeDataSetX.SetCampo(("vTotTrib_M02=" + this.FormataTDEC_1302(aNotaItem.TotalImpostos)));

            DadosNotaItemExportacao(aNotaItem);
            DadosNotaItemImportacao(aNotaItem);

            //Informações Adicionais
            if (aNotaItem.InfAdicionais != null)
            {
                SpdNFeDataSetX.SetCampo(("infAdProd_V01=" + aNotaItem.InfAdicionais));
            }

            if (aNotaItem.TextoLei != null)
                obs = obs + " " + aNotaItem.TextoLei.Trim();
        }

        private void IncluirTagICMSDeson(INotaItem aNotaItem)
        {
            if (aNotaItem.motDesICMS != 0)
            {
                SpdNFeDataSetX.SetCampo(("vICMSDeson_N27a=" + this.FormataTDEC_1302(aNotaItem.vICMSDeson))); // Valor do ICMS desonerado
                SpdNFeDataSetX.SetCampo(("motDesICMS_N28=" + aNotaItem.motDesICMS)); // Motivo da desoneração do ICMS
            }
        }
        // Rastreabilidade de produto * Informar apenas quando se tratar de produto a ser rastreado posteriormente
        private void IncluirRastreabilidadeProduto()
        {
            SpdNFeDataSetX.IncluirParte("I80");
            SpdNFeDataSetX.SetCampo(("nlote_I81=")); // Número do Lote do produto
            SpdNFeDataSetX.SetCampo(("qLote_I82=")); // Quantidade de produto no Lote
            SpdNFeDataSetX.SetCampo(("dFab_I83=")); // Data de fabricação/ Produção
            SpdNFeDataSetX.SetCampo(("dVal_I84=")); // Data de validade
            SpdNFeDataSetX.SetCampo(("cAgreg_I85=")); // Código de Agregação
            SpdNFeDataSetX.SalvarParte("I80");

        }

        private void DadosNotaItemExportacao(INotaItem aNotaItem)
        {
            SpdNFeDataSetX.IncluirParte("DETEXPORT"); 
            SpdNFeDataSetX.SetCampo("nDraw_I51=" + aNotaItem.NumDrawBack);
            SpdNFeDataSetX.SalvarParte("DETEXPORT");
        }

        private void DadosNotaItemImportacao(INotaItem aNotaItem)
        {
            SpdNFeDataSetX.SetCampo("nDI_I19=" + aNotaItem.I19_nDI);
            if (aNotaItem.I20_dDI.HasValue)
                SpdNFeDataSetX.SetCampo("dDI_I20=" + aNotaItem.I20_dDI.Value.ToString("yyyy-MM-dd"));
            SpdNFeDataSetX.SetCampo("xLocDesemb_I21=" + aNotaItem.I21_xLocDesemb);
            SpdNFeDataSetX.SetCampo("UFDesemb_I22=" + aNotaItem.I22_UFDesemb);
            if (aNotaItem.I23_dDesemb.HasValue)
                SpdNFeDataSetX.SetCampo("dDesemb_I23=" + aNotaItem.I23_dDesemb.Value.ToString("yyyy-MM-dd"));
            SpdNFeDataSetX.SetCampo("cExportador_I24=" + aNotaItem.I24_cExportador);

            SpdNFeDataSetX.SetCampo("vBC_P02=" + FormataTDEC_1302(aNotaItem.P02_vBC));
            SpdNFeDataSetX.SetCampo("vDespAdu_P03=" + FormataTDEC_1302(aNotaItem.P03_vDespAdu));
            SpdNFeDataSetX.SetCampo("vII_P04=" + FormataTDEC_1302(aNotaItem.P04_vII));
            SpdNFeDataSetX.SetCampo("vIOF_P05=" + FormataTDEC_1302(aNotaItem.P05_vIOF));

            if (aNotaItem.ViaTransp != 0)
            {
                SpdNFeDataSetX.SetCampo("tpViaTransp_I23a=" + aNotaItem.ViaTransp);
                if (aNotaItem.ViaTransp == 1)
                {
                    SpdNFeDataSetX.SetCampo("vAFRMM_I23b=" + aNotaItem.ValorFreteRenovacaoMarinhaMercante);
                }
            }

            SpdNFeDataSetX.IncluirParte("adi");

            foreach (var adi in aNotaItem.AdicoesNotaItem)
            {
                SpdNFeDataSetX.SetCampo("nAdicao_I26=" + FormataINTEIRO(adi.I26_nAdicao));
                SpdNFeDataSetX.SetCampo("nSeqAdic_I27=" + FormataINTEIRO(adi.I27_nSeqAdic));
                SpdNFeDataSetX.SetCampo("cFabricante_I28=" + adi.I28_cFabricante);
                SpdNFeDataSetX.SetCampo("xPed_I30=" + adi.I30_xPed);
                SpdNFeDataSetX.SetCampo("nItemPed_I31=" + FormataINTEIRO(adi.I31_nItemPed));
                if (adi.I29_vDescDI > 0)
                    SpdNFeDataSetX.SetCampo("vDescDI_I29=" + FormataTDEC_1302(adi.I29_vDescDI));
            }

            SpdNFeDataSetX.SalvarParte("adi");
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
                    SpdNFeDataSetX.SetCampo(("vDesc_Y05=" + this.FormataTDEC_1302(0))); // Valor Original da Fatura
                    SpdNFeDataSetX.SetCampo(("vLiq_Y06=" + this.FormataTDEC_1302(parcela.FaturaValorLiquido))); // Valor Líquido da Fatura
                }
                
                if (parcela.FormaPagamento == "14")
                {
                    SpdNFeDataSetX.SetCampo(("nDup_Y08=" + parcela.Sequencia.ToString())); // Número da Duplicata
                    SpdNFeDataSetX.SetCampo(("dVenc_Y09=" + parcela.Vencimento.ToString("yyyy-MM-dd"))); // Data de Vencimento da Duplicata
                    SpdNFeDataSetX.SetCampo(("vDup_Y10=" + this.FormataTDEC_1302(parcela.Valor))); // Valor da Duplicata
                }
                
                SpdNFeDataSetX.SalvarCobranca(); // Grava a Duplicata em questão.
              }
            
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
            SpdNFeDataSetX.SetCampo(("vFCP_W04h=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vFCP_N17c)))); // Valor Total do FCP
            SpdNFeDataSetX.SetCampo(("vFCPST_W06a=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vFCPST_N23d)))); // Valor Total do FCP
            SpdNFeDataSetX.SetCampo(("vFCPSTRet_W06b=" + this.FormataTDEC_1302(notaItems.Sum(a => a.vFCPST_N23d)))); // Valor Total do FCP retido anteriormente por Substituição Tributária
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

        private string PegarPercentualPartilha()
        {
            DateTime dataAgora = DateTime.Now;
            int ano = dataAgora.Year;
            if (Nota.NotaReferenciada != null && Nota.NotaReferenciada.DtEmissao != null)
            {
                ano = Nota.NotaReferenciada.DtEmissao.Year;
            }
            if (ano < 2016)
            {
                ano = 2015;
            }
            switch (ano)
            {
                case 2015: return "0.00"; break;
                case 2016: return "40.00"; break;
                case 2017: return "60.00"; break;
                case 2018: return "80.00"; break;
                case 2019: return "100.00"; break;
                default: return "100.00"; break; 
            }
        }

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
                // Dados do Veículo de Transporte
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
        // Informações de Pagamento
        private void InformacoesPagamento()
        {
            foreach (INotaParcela formasPagamento in Nota.NotaParcelas)
            {
                SpdNFeDataSetX.IncluirParte("YA");
                SpdNFeDataSetX.SetCampo(("tPag_YA02=" + formasPagamento.FormaPagamento));
                SpdNFeDataSetX.SetCampo(("vPag_YA03=" + this.FormataTDEC_1302(formasPagamento.Valor)));
                SpdNFeDataSetX.SalvarParte("YA");   
            }

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
                return GerarNotaComplementar();
        }

        private IDictionary<string, string> GerarNotaNormal()
        {
            IDictionary<string, string> retorno = new Dictionary<string, string>();
            String aXmlNota = "";
            String aXmlNotaEPEC = "";
            String envioDaNota = "";
            NotaStatusAnterior = Nota.Status;
            Nota.Status = Nota.Status != "8" ? "-1" : "8";

            if ((retorno = ValidaDadosNFe()).Count > 0)
            {
                throw new Exception("Nota não é validada");
            }

            aXmlNota = SpdNFeX.StatusDoServico();
           if ((retorno = VerificaStatusServico(aXmlNota)) != null)
            {
                throw new ServidorOfflineException(null, "O servidor do serviço está offline.");
            }

            aXmlNota = GeraXmlNota().Trim();
            SalvarXmlArquivo(aXmlNota, "UltimoXmlGerado.xml");

            if (FormaEmissao == TipoEmissao.DPEC)
            {
                String dataEnvio = Nota.DtEmissao.ToString("yyyy-MM-dd\"T\"HH:mm:ss");
                String fusoEnvio = Nota.DtEmissao.ToString("zzz");

                Nota.XmlLogEnvNFe = aXmlNota;

                aXmlNotaEPEC = SpdNFeX.MontarEPEC(Nota.Id.ToString(), aXmlNota, dataEnvio, fusoEnvio);
                String dpecAssinado = SpdNFeX.AssinarEPEC(aXmlNotaEPEC);
                aXmlNota = SpdNFeX.EnviarEPEC(dpecAssinado);
                envioDaNota = aXmlNota;
               
                Int32 cStat = RetornaCStat(aXmlNota);
                if ((MontaCodigoErrosNotaEpec().Where(num => num == cStat).Count() == 0) && !(aXmlNota.Contains("Rejeicao")))
                {
                    var regex = new Regex(@"<nProt>(.*?)</nProt>");
                    var match = regex.Match(aXmlNota);
                    Nota.NumeroProtocolo = match.Groups[1].Value;
                    Nota.Status = "8";
                }    
                else
                    Nota.Status = NotaStatusAnterior != "8" ? "0" : "8";

                Nota.UltimoXmlRecebido = aXmlNota;
            }
            else
            {
                aXmlNota = SpdNFeX.EnviarNF(Nota.Numero.ToString(), aXmlNota, false);
                envioDaNota = aXmlNota;

                Nota.NumeroRecibo = TrataRetornoEnvioNumeroRecibo(aXmlNota);
                Nota.LogEnvio = FormataUltimoLog(1, SpdNFeX.UltimoLogEnvio);
                Nota.XmlLogEnvNFe = SpdNFeX.LerLog(SpdNFeX.DiretorioLog + Nota.LogEnvio);
            }

            return envioDaNota.DesmembrarXml();
        }

        public Int32 RetornaCStat(string aXmlNota)
        {
            Int32 cStat = -1;
            int indiceEventoInfo = aXmlNota.IndexOf("<infEvento>");
            if (indiceEventoInfo != -1)
            {
                var regex = new Regex(@"<cStat>(.*?)</cStat>");
                var match = regex.Match(aXmlNota.Substring(indiceEventoInfo));
                String cStatString = match.Groups[1].Value;
                cStatString = new String(cStatString.Where(Char.IsDigit).ToArray());

                Int32.TryParse(cStatString, out cStat);
            }

            return cStat;
        }

        public IList<Int32> MontaCodigoErrosNotaEpec()
        {
            IList<Int32> erros = new List<Int32>();
            erros.Add(142);
            erros.Add(203);
            erros.Add(208);
            erros.Add(209);
            erros.Add(210);
            erros.Add(212);
            erros.Add(228);
            erros.Add(229);
            erros.Add(230);
            erros.Add(231);
            erros.Add(233);
            erros.Add(234);
            erros.Add(236);
            erros.Add(237);
            erros.Add(250);
            erros.Add(252);
            erros.Add(302);
            erros.Add(408);
            erros.Add(417);
            erros.Add(418);
            erros.Add(455);
            erros.Add(466);
            erros.Add(467);
            erros.Add(468);
            erros.Add(484);
            erros.Add(485);
            erros.Add(489);
            erros.Add(490);
            erros.Add(491);
            erros.Add(492);
            erros.Add(493);
            erros.Add(572);
            erros.Add(573);
            erros.Add(574);
            erros.Add(576);
            erros.Add(577);
            erros.Add(578);
            erros.Add(594);
            erros.Add(614);
            erros.Add(615);
            erros.Add(616);
            erros.Add(617);
            erros.Add(618);
            erros.Add(619);
            erros.Add(628);
            erros.Add(661);
            erros.Add(662);
            erros.Add(720);
            erros.Add(721);
            erros.Add(792);

            return erros;
        }

        private void SalvarXmlArquivo(string xmlNota, string nomeArquivo)
        {
            StreamWriter stream = new StreamWriter(nomeArquivo);
            stream.Write(xmlNota);
            stream.Close();
        }

        public Dictionary<string, string> GerarNotaComplementar()
        {
            IniciarDataSet();

            this.SpdNFeDataSetX.Incluir();
            this.DadosNFe(FinalidadeNFe.Complementar);
            if (Nota.NotaComplementada.Status != "2")
                throw new Exception("Só é possível gerar nota complementar de notas autorizadas.");

            SpdNFeDataSetX.IncluirParte("NRef"); 
            SpdNFeDataSetX.SetCampo("refNFe_BA02=" + Nota.NotaComplementada.ChaveNota);
            SpdNFeDataSetX.SalvarParte("NRef");

            this.DadosEmitente();
            this.DadosDestinatario();
            this.InformacoesPagamento();

            int seq = 0;
            foreach (INotaItem objNotaItem in Nota.NotaItems)
            {
                seq += 1;
                this.SpdNFeDataSetX.IncluirItem();
                this.DadosItem(objNotaItem);
                this.SpdNFeDataSetX.SalvarItem();
            }

            DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
            SpdNFeDataSetX.SetCampo(("modFrete_X02=0")); // Modalidade de Frete = 0 por ser complementar

            SpdNFeDataSetX.Salvar();

            string xmlNota = SpdNFeDataSetX.LoteNFe;

            xmlNota = SpdNFeX.AssinarNota(xmlNota);
            xmlNota = SpdNFeX.EnviarNF(Nota.NotaComplementada.Numero.ToString(), xmlNota, false);

            Nota.NumeroRecibo = TrataRetornoEnvioNumeroRecibo(xmlNota);
            Nota.LogEnvio = FormataUltimoLog(1, SpdNFeX.UltimoLogEnvio);
            Nota.XmlLogEnvNFe = SpdNFeX.LerLog(SpdNFeX.DiretorioLog + Nota.LogEnvio);

            return new Dictionary<string, string>();
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
                    this.DadosNFe(FinalidadeNFe.DevolucaoRetorno);
                }
                else
                    this.DadosNFe(FinalidadeNFe.Normal);

                this.DadosEmitente();
                this.DadosDestinatario();
                this.DadosEntrega();
                int seq = 0;
                foreach (INotaItem objNotaItem in Nota.NotaItems)
                {
                    seq += 1;
                    this.SpdNFeDataSetX.IncluirItem();
                    this.DadosItem(objNotaItem);
                    this.SpdNFeDataSetX.SalvarItem();
                }

                DadosCobranca();
                InformacoesPagamento();
                DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
                DadosTransporte();

                SpdNFeDataSetX.Salvar();

                aXmlNota = SpdNFeDataSetX.LoteNFe;

                Nota.ChaveNota = aXmlNota.Substring(aXmlNota.IndexOf("infNFe Id") + 14, 44);

                aXmlNota = SpdNFeX.AssinarNota(aXmlNota);

                return aXmlNota;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public void GerarNotaReferenciada(INota notaReferenciada)
        {
            if (notaReferenciada == null)
                throw new Exception("Não foi possível encontrar a nota referenciada. Por favor verifique.");

            if (String.IsNullOrEmpty(notaReferenciada.ChaveNota))
                throw new Exception("Só é possível gerar nota referenciadas de notas que possuem chave.");

            SpdNFeDataSetX.IncluirParte("NREF");
            SpdNFeDataSetX.SetCampo("refNFe_BA02=" + notaReferenciada.ChaveNota);

            SpdNFeDataSetX.SetCampo("cUF_BA04=" + notaReferenciada.Empresa.UFIBGE);

            string anoEmissao = notaReferenciada.DtEmissao.Year.ToString();
            string mesEmissao = notaReferenciada.DtEmissao.Month.ToString();
            string AAMM = anoEmissao.Substring(2) + mesEmissao;

            SpdNFeDataSetX.SetCampo("AAMM_BA05=" + AAMM);
            SpdNFeDataSetX.SetCampo("CNPJ_BA06=" + Funcoes.LimpaStr(notaReferenciada.Empresa.Cnpj));
            SpdNFeDataSetX.SetCampo("mod_BA07=" + notaReferenciada.ModeloDocto);
            SpdNFeDataSetX.SetCampo("serie_BA08=" + notaReferenciada.Serie);
            SpdNFeDataSetX.SetCampo("nNF_BA09=" + notaReferenciada.Numero.ToString());

            SpdNFeDataSetX.SalvarParte("NREF");
           
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
                    this.DadosNFe(FinalidadeNFe.DevolucaoRetorno);
                }
                else
                    this.DadosNFe(FinalidadeNFe.Normal);


                this.DadosEmitente();
                this.DadosDestinatario();
                this.DadosEntrega();
                int seq = 0;
                foreach (INotaItem objNotaItem in Nota.NotaItems)
                {
                    seq += 1;
                    this.SpdNFeDataSetX.IncluirItem();
                    this.DadosItem(objNotaItem);
                    this.SpdNFeDataSetX.SalvarItem();
                }

                DadosCobranca();
                InformacoesPagamento();
                DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
                DadosTransporte();


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
                string xMotivo = (from noh in documentoXml.Root.Elements() where noh.Name.LocalName == "xMotivo" select noh.Value).Single<string>();
                Nota.StatusMotivo = "Campo inconsistente: " + CapturaCampoErrado(xMotivo);

                throw new XmlMalFormatadoException(retorno, "Ocorreram erros no envio da nota.");
            }
            else
            {
                IDictionary<string, string> retorno = new Dictionary<string, string>();
                Nota.Status = "1";
                return documentoXml.Root.Elements().Where(x => x.Name.LocalName == "infRec").Elements()
                                                .Where(x => x.Name.LocalName == "nRec")
                                                    .Single().Value;
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
                        Nota.Status = NotaStatusAnterior != "8" ? "0" : NotaStatusAnterior;
                        throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
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
            //var noh = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "retConsStatServ" select c).Single<XElement>();

            if (noh.Value == "107")
            {
                /* string Mensagem = noh.Value;
                 string Valor = "Servico em Operacao";
                 if (Mensagem.Contains(Valor))
                 {*/
                return null;
            }
            else
            {
                IDictionary<string, string> retorno = new Dictionary<string, string>();
                retorno.Add("Serviço offline", "Serviço offline");
                return retorno;
            }
        }

        public override IDictionary<string, string> CancelarNFe(string _motivo, string _usuario)
        {
            SpdNFeX.DiretorioLogErro = AppDomain.CurrentDomain.BaseDirectory + @"\DiretorioLogErro";
            DateTime agora = DateTime.Now;
            String DataHoraFormatada = String.Format("{0:yyyy-MM-dd'T'HH:mm:ss}", agora);

            String utcNota = "-03:00";
            String DataHoraFormatadaComUTC = agora.ToString("yyyy-MM-dd\"T\"HH:mm:sszzz");
            int indice = DataHoraFormatadaComUTC.LastIndexOf('-');
            if (indice != -1)
                utcNota = DataHoraFormatadaComUTC.Substring(indice, 6);

            string aXmlNota = SpdNFeX.CancelarNFeEvento(Nota.ChaveNota, Nota.NumeroProtocolo, _motivo, DataHoraFormatada, 1, utcNota);
            
            if (aXmlNota == null || aXmlNota == "")
            {
                throw new SemRespostaDoServidorException(null);
            } 

            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlNota))));

            //XElement nohInfCanc = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "retEvento" select c).Single();
            //XElement noh = (from c in nohInfCanc.Elements() where c.Name.LocalName == "infEvento" select c).Single();
            //string valorCStat = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlNota))));

            XmlNodeList elemList = doc.GetElementsByTagName("infEvento");
            string valorCStat = elemList[0].ChildNodes[3].InnerText;

            //Método Alterado dia 26/07/2018 para cancelamento da Nfe - André - por Tecnospeed//


            if (valorCStat == "135")
            {
                Nota.CancDt = DateTime.Today;
                Nota.CancMotivo = _motivo;
                Nota.CancUsuario = _usuario;

                Nota.Status = "3";
            }

            Nota.UltimoXmlRecebido = aXmlNota;

            return documentoXml.DesmembrarXml();

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
                throw new SemRespostaDoServidorException(null, "Não houve resposta do servidor no recebimento do recibo.");

            Nota.UltimoXmlRecebido = aXmlNota;

            Nota.NumeroProtocolo = String.Empty;

            if (String.IsNullOrEmpty(Nota.NumeroProtocolo))
                return AtribuiRetornoRecibo(aXmlNota);
            else
                return aXmlNota.DesmembrarXml();
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
