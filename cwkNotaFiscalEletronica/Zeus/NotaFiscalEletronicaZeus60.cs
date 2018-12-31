using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using cwkNotaFiscalEletronica.Interfaces;
using System.IO;
using System.Xml.Linq;
using cwkNotaFiscalEletronica.Erros;
using System.Xml;
using System.Text.RegularExpressions;

using NFe.Classes.Servicos.Tipos;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;
using NFe.Utils.Tributacao.Federal;
using NFe.Servicos;
using NFe.Utils.NFe;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using DFe.Utils;
using DFe.Classes.Entidades;
using DFe.Classes.Flags;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Cobranca;
using TipoEmissaoZeus = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao;
using FinalidadeNFeZeus = NFe.Classes.Informacoes.Identificacao.Tipos.FinalidadeNFe;
using System.Windows.Forms;

namespace cwkNotaFiscalEletronica
{
    class NotaFiscalEletronicaZeus60 : INotaFiscalEletronica
    {
        private string obs;
        private Int16 IndFinal  { get; set; }
        private IndPres IndPres { get; set; }
        private bool BDevolucao { get; set; }
        public string NotaStatusAnterior { get; set; }

        private NFe.Classes.NFe _nfe;


        public NotaFiscalEletronicaZeus60(TipoEmissaoZeus _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string _diretorioPadrao,
                                       Int16 indFinal, IndPres indPres, bool bDevolucao) : base(_tipoServidor, _ambiente, _tipoCertificado, _diretorioPadrao) 
        { 
            IndFinal = indFinal; 
            IndPres = indPres;
            BDevolucao = bDevolucao;
        }

        #region Iniciar
        public override void Iniciar()
        {
            obs = "";

            MontaConfiguracoesZeus();
            _configuracoes.CfgServico.ModeloDocumento = ModeloDocumento.NFe;

            switch (FormaEmissao)
            {
                case TipoEmissaoZeus.teNormal:
                    _configuracoes.CfgServico.tpEmis = TipoEmissaoZeus.teNormal;                    
                    break;
                case TipoEmissaoZeus.teSCAN:
                    _configuracoes.CfgServico.tpEmis = TipoEmissaoZeus.teSCAN;                    
                    break;
                case TipoEmissaoZeus.teSVCRS:
                    _configuracoes.CfgServico.tpEmis = TipoEmissaoZeus.teSVCRS;
                    break;
            }          
                        
        }
        #endregion

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

        #region VerificaCodigo
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
        #endregion

        #region TrataRetorno
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
        #endregion

        #region DadosNFe
        private void DadosNFe(FinalidadeNFeZeus finalidadeNFe)
        {            
            _nfe = new NFe.Classes.NFe();

            var infNFe = new infNFe();
            infNFe.Id = "NFe"; //Calcula Automático. Essa linha é desnecessária 
            infNFe.versao = "4.00"; //Versão do Layout que está utilizando

            infNFe.ide = new ide();
            infNFe.ide.cUF = (Estado) Convert.ToInt64(Nota.Empresa.UFIBGE); //Codigo da UF para o estado de SP (Emitente da NFe)
            infNFe.ide.cNF = Nota.Id.ToString().PadLeft(9, '0'); //Código Interno do Sistema que está integrando com a NFe
            infNFe.ide.natOp = Nota.NotaItems.First().CFOPDescricao; //Descrição da(s) CFOP(s) envolvidas nessa NFe
            infNFe.ide.mod = (ModeloDocumento) Nota.ModeloDocto; //Código do Modelo de Documento Fiscal
            infNFe.ide.serie = Convert.ToInt32(Nota.Serie); //Série do Documento
            infNFe.ide.nNF = Nota.Numero; //Número da Nota Fiscal
            infNFe.ide.dhEmi = Nota.DtEmissao; //Data e hora de Emissão da Nota Fiscal
            infNFe.ide.dhSaiEnt = Nota.DtSaida; //Data e hora de Saída ou Entrada da Nota Fiscal
            infNFe.ide.tpNF = (TipoNFe) (Nota.TipoNota == TipoNotaEntSaida.Entrada ? 0 : 1); //Tipo de Documento Fiscal (0-Entrada, 1-Saída)
            infNFe.ide.cMunFG = Convert.ToInt64(Nota.Empresa.CidadeIBGE); //Código do Município, conforme Tabela do IBGE
            infNFe.ide.tpImp = (TipoImpressao) ((int)Danfe); //Tipo de Impressão da Danfe (1- Retrato , 2-Paisagem)

            if (NotaStatusAnterior != "8")
            {
                infNFe.ide.tpEmis = (TipoEmissaoZeus) ((int)FormaEmissao); //Forma de Emissão da NFe (1 - Normal, 2 - FS, 3 - SCAN, 4 - DPEC, 5 - FS-DA, 6 - SVCAN, 7 - SVCRS);
            }
            else
            {
               infNFe.ide.tpEmis = TipoEmissaoZeus.teEPEC; //Forma de Emissão da NFe (1 - Normal, 2 - FS, 3 - SCAN, 4 - DPEC, 5 - FS-DA, 6 - SVCAN, 7 - SVCRS);
            }

            infNFe.ide.cDV = 0; //Calcula Automatico - Linha desnecessária já que o componente calcula o Dígito Verificador automaticamente e coloca no devido campo
            //infNFe.ide.tpAmb = ((int)CwkAmbiente); //Identificação do Ambiente (1- Producao, 2-Homologação)
            //infNFe.ide.tpAmb = TipoAmbiente.Homologacao; //Identificação do Ambiente (1- Producao, 2-Homologação)
            infNFe.ide.tpAmb = (TipoAmbiente)CwkAmbiente; //Identificação do Ambiente (1- Producao, 2-Homologação)
            infNFe.ide.finNFe = (FinalidadeNFeZeus)((int)finalidadeNFe); //Finalidade da NFe (1-Normal, 2-Complementar, 3-de Ajuste, 4-Devolução)
            infNFe.ide.procEmi = 0; //Identificador do Processo de emissão (0-Emissão da Nfe com Aplicativo do Contribuinte). Ver outras opções no manual da Receita.
            infNFe.ide.verProc = "1.00"; //Versão do Aplicativo Emissor

            if (FormaEmissao == TipoEmissaoZeus.teEPEC || NotaStatusAnterior == "8")
            {
                infNFe.ide.dhCont = Nota.Empresa.ContingenciaDataHora; //Data e Hora ("AAAA-MM-DDTHH:MM:SSzzz") da entrada em contingencia
                infNFe.ide.xJust = Nota.Empresa.ContingenciaJustificativa; //Motivo da entrada em contingencia
            }

            infNFe.ide.idDest = (DestinoOperacao) Nota.idDest;
            infNFe.ide.indFinal = (ConsumidorFinal) IndFinal;
            infNFe.ide.indPres = (PresencaComprador) (int)IndPres;

            //SpdNFeDataSetX.SetCampo(("UFSaidaPais_ZA02=" + Nota.ZA02_UFEmbarq)); //nome ad tag alterado de UFEmbarq para UFSaidaPais
            //SpdNFeDataSetX.SetCampo(("xLocExporta_ZA03=" + Nota.ZA03_xLocEmbarq)); //nome ad tag alterado de xLocEmbarq para xLocExporta


            if (finalidadeNFe == FinalidadeNFeZeus.fnDevolucao)
                GerarNotaReferenciada(Nota.NotaReferenciada);

            _nfe.infNFe = infNFe;
        }
        #endregion

        #region DadosEmitente
        private void DadosEmitente()
        {            
            var emitente = new emit();

            emitente.CNPJ = Funcoes.LimpaStr(Nota.Empresa.Cnpj); // CNPJ do Emitente
            emitente.xNome = Nota.Empresa.Nome; // Razao Social ou Nome do Emitente

            if(Nota.Empresa.Fantasia != "")
                emitente.xFant = Nota.Empresa.Fantasia; // Nome Fantasia do Emitente

            emitente.enderEmit = new enderEmit();
            emitente.enderEmit.xLgr = Nota.Empresa.Endereco; // Logradouro do Emitente
            emitente.enderEmit.nro = Nota.Empresa.Numero; // Numero do Logradouro do Emitente

            if (!String.IsNullOrEmpty(Nota.Empresa.Complemento))
                emitente.enderEmit.xCpl = Nota.Empresa.Complemento; //Complemento do emitente

            emitente.enderEmit.xBairro = Nota.Empresa.Bairro; // Bairro do Emitente
            emitente.enderEmit.cMun = Convert.ToInt64(Nota.Empresa.CidadeIBGE); // Código da Cidade do Emitente (Tabela do IBGE)
            emitente.enderEmit.xMun = Nota.Empresa.Cidade; // Nome da Cidade do Emitente
            emitente.enderEmit.UF = Nota.Empresa.UF; // SIGLA do Estado do Emitente (Tabela do IBGE)
            emitente.enderEmit.CEP = Funcoes.LimpaStr(Nota.Empresa.CEP); // Cep do Emitente
            emitente.enderEmit.cPais = 1058; // Código do País do Emitente (Tabela BACEN)
            emitente.enderEmit.xPais = "BRASIL"; // Nome do País do Emitente
            emitente.enderEmit.fone = Convert.ToInt64( Funcoes.LimpaStr(Nota.Empresa.Telefone) ); // Fone do Emitente
            emitente.IE = Funcoes.LimpaStr(Nota.Empresa.Inscricao); // Inscrição Estadual do Emitente

            if (Nota.PessoaCidadeIBGE != "9999999" && Nota.Empresa.TipoST == TipoST.Substituto && Nota.NotaItems.Sum(a => a.ValorRetidoICMS) > 0)
            {
                emitente.IEST = Funcoes.LimpaStr(Nota.PessoaInscRG); // Inscrição Estadual do Substituto Tributário Emitente
            }

            emitente.CRT = (CRT)(int)Nota.Empresa.TipoCRT; //Código de Regime Tributário do Emitente


            _nfe.infNFe.emit = emitente;

        }
        #endregion

        #region DadosDestinatario
        private void DadosDestinatario()
        {
            var dest = new dest(VersaoServico.ve400);
            dest.enderDest = new enderDest();

            string cnpjcpf = Funcoes.LimpaStr(Nota.PessoaCNPJCPF);
            if (Nota.PessoaCidadeIBGE == "9999999")
            {
                dest.CNPJ = ""; // CNPJ do Destinatário
                dest.indIEDest = indIEDest.NaoContribuinte; //Indicador da IE do Destinatário
                dest.IE = ""; // Inscrição Estadual do Destinatário

                dest.enderDest.xMun = "EXTERIOR"; // Nome da Cidade do Destinatário
                dest.enderDest.UF = "EX"; // Sigla do Estado do Destinatário
                dest.enderDest.cPais = Convert.ToInt32(Nota.Cliente.PaisIBGE); // Código do Pais do Destinatário (Tabela do BACEN)
                dest.enderDest.xPais = Nota.Cliente.Pais; // Nome do País do Destinatário                
            }
            else
            {
                dest.enderDest.xMun = Nota.PessoaCidade; // Nome da Cidade do Destinatário
                dest.enderDest.UF = Nota.PessoaUF; // Sigla do Estado do Destinatário
                dest.enderDest.cPais = 1058; // Código do Pais do Destinatário (Tabela do BACEN)
                dest.enderDest.xPais = "BRASIL"; // Nome do País do Destinatário

                if (cnpjcpf.Length > 11)
                {
                    dest.CNPJ = cnpjcpf; // CNPJ do Destinatário

                    if (Nota.Cliente.bContribuinte)
                    {
                        dest.indIEDest = indIEDest.ContribuinteICMS; //Indicador da IE do Destinatário
                        dest.IE = Funcoes.LimpaStr(Nota.PessoaInscRG); // Inscrição Estadual do Destinatário
                    }
                    else
                    {
                        dest.indIEDest = indIEDest.NaoContribuinte; //Indicador da IE do Destinatário
                        dest.IE = Funcoes.LimpaStr(Nota.PessoaInscRG); // Inscrição Estadual do Destinatário
                    }

                    if (Nota.PessoaSUFRAMA != null && Nota.PessoaSUFRAMA != "")
                    {
                        dest.ISUF = Funcoes.LimpaStr(Nota.PessoaSUFRAMA); //Inscrição Suframa do Destinatário
                    }
                }
                else
                {
                    dest.CPF = cnpjcpf; // CPF do Destinatário
                    dest.CNPJ = null; // CNPJ do Destinatário

                    if (Nota.Cliente.bContribuinte)
                    {
                        if (String.IsNullOrEmpty(Nota.PessoaInscRG))
                        {
                            dest.indIEDest = indIEDest.ContribuinteICMS;//Indicador da IE do Destinatário
                            dest.IE = null; // Inscrição Estadual do Destinatário
                        }
                        else
                        {
                            dest.indIEDest = indIEDest.Isento;//Indicador da IE do Destinatário
                            dest.IE = Nota.PessoaInscRG; // Inscrição Estadual do Destinatário
                        }
                    }
                    else
                    {
                        dest.indIEDest = indIEDest.NaoContribuinte;
                        dest.IE = (String.IsNullOrEmpty(Nota.PessoaInscRG) == true ? null : Nota.PessoaInscRG); // Inscrição Estadual do Destinatário
                    }                                                     
                }                                         
            }

            dest.xNome = Nota.PessoaNome; // Razao social ou Nome do Destinatário
            dest.enderDest.xLgr = Nota.PessoaEndereco; // Logradouro do Destinatario
            dest.enderDest.nro = Nota.PessoaNumero; // Numero do Logradouro do Destinatario
            if (!String.IsNullOrEmpty(Nota.Cliente.Complemento))
            {
                dest.enderDest.xCpl = Nota.Cliente.Complemento;
            }
            dest.enderDest.xBairro = Nota.PessoaBairro; // Bairro do Destinatario
            dest.enderDest.cMun = Convert.ToInt64(Nota.PessoaCidadeIBGE); // Código do Município do Destinatário (Tabela IBGE)
            dest.enderDest.CEP = Funcoes.LimpaStr(Nota.PessoaCEP); // Cep do Destinatário
            dest.enderDest.fone = Convert.ToInt64( Funcoes.LimpaStr(Nota.PessoaTelefone) ); // Fone do Destinatário

            if (Nota.PessoaEmail != "")
            {
                dest.email = Nota.PessoaEmail; // Email
            }
            else
            {
                dest.email = "naoinformado@naoinformado.com";
            }

            _nfe.infNFe.dest = dest;
        }
        #endregion

        #region DadosEntrega
        private void DadosEntrega()
        {            
            if (Nota.EntregaEndreco != null)
            {
                var entrega = new entrega();

                string cnpjcpf = Funcoes.LimpaStr(Nota.PessoaCNPJCPF);
                if (cnpjcpf.Length == 11)
                    entrega.CPF = cnpjcpf;
                else
                    entrega.CNPJ = cnpjcpf;

                entrega.xLgr = Nota.EntregaEndreco.Logradouro;
                entrega.nro = Nota.EntregaEndreco.Numero;
                entrega.xCpl = Nota.EntregaEndreco.Complemente;
                entrega.xBairro = Nota.EntregaEndreco.Bairro;
                entrega.cMun = Convert.ToInt64( Nota.EntregaEndreco.CidadeCodigoIBGE);
                entrega.xMun = Nota.EntregaEndreco.CidadeNome;
                entrega.UF = Nota.EntregaEndreco.UFSigla;

                _nfe.infNFe.entrega = entrega;
            }                        
        }
        #endregion

        #region DadosItem
        protected virtual det DadosItem(INotaItem aNotaItem)
        {
            var det = new det();

            //Importante: Respeitar a ordem sequencial do campo nItem, quando gerar os itens
            det.nItem = aNotaItem.Sequencia; // Número do Item da NFe (1 até 990)
            //Dados do Produto Vend Subido
            det.prod = new prod();
            det.prod.cProd = aNotaItem.ProdutoCodigo.ToString(); //Código do PRoduto ou Serviço

            if (aNotaItem.cEAN != "")
            { 
                det.prod.cEAN = aNotaItem.cEAN; // EAN do Produto
            }
            else
            {
                det.prod.cEAN = "SEM GTIN"; // EAN do Produto
            }
                
            det.prod.xProd = aNotaItem.ProdutoNome; // Descrição do PRoduto
            det.prod.NCM = aNotaItem.ProdutoNCM; // Código do NCM - informar de acordo com o Tabela oficial do NCM
            
            if (!String.IsNullOrEmpty(aNotaItem.Cest))
            {
                det.prod.CEST = aNotaItem.Cest;                

                if (aNotaItem.indEscala_I05d == 1)
                {
                    det.prod.indEscala = indEscala.N; //Indicador de Escala Relevante
                    det.prod.CNPJFab = aNotaItem.CNPJFab_I05e; // CNPJ do Fabricante da Mercadoria
                }
                else
                {
                    det.prod.indEscala = indEscala.S; //Indicador de Escala Relevante
                }
            }

            det.prod.cBenef = aNotaItem.cBenef_I05f;// Código de Benefício Fiscal na UF aplicado ao item
            det.prod.CFOP = int.Parse(aNotaItem.CFOPCodigo); // CFOP incidente neste Item da NF
            det.prod.uCom = aNotaItem.Unidade; // Unidade de Medida do Item
            det.prod.qCom = aNotaItem.Quantidade;// Quantidade Comercializada do Item
            det.prod.vUnCom = aNotaItem.Valor;// Valor Comercializado do Item
            det.prod.vProd = aNotaItem.Total;// Valor Total Bruto do Item
            
            if (aNotaItem.cEAN != "")
            {
                det.prod.cEANTrib = aNotaItem.cEANTrib; // EAN Tributável do Item
            }
            else
            {
                det.prod.cEANTrib = "SEM GTIN"; // EAN do Produto
            }
                        
            det.prod.uTrib = aNotaItem.Unidade; // Unidade de Medida Tributável do Item
            det.prod.qTrib = aNotaItem.Quantidade; // Quantidade Tributável do Item
            det.prod.vUnTrib = aNotaItem.Valor; // Valor Tributável do Item
            det.prod.indTot = IndicadorTotal.ValorDoItemCompoeTotalNF; // Indica se valor do Item vProd entra no valor total da NF-e vProd
            det.prod.nFCI = aNotaItem.FCI; //FCI do produto
       
            if (aNotaItem.ValorFrete > 0)
                det.prod.vFrete = aNotaItem.ValorFrete;

            if (aNotaItem.ValorSeguro > 0)
                det.prod.vSeg = aNotaItem.ValorSeguro;

            if (aNotaItem.RAT_Desconto > 0)
                det.prod.vDesc = aNotaItem.RAT_Desconto;

            if (aNotaItem.OutrasDespesas > 0)
                det.prod.vOutro = aNotaItem.OutrasDespesas;

            // Aqui começam os Impostos Incidentes sobre o Item''''''''''''
            //Verificar Manual pois existe uma variação nos campos de acordo com Tipo de Tribucação ''
          
            det.imposto = new imposto();
            //ICMS
            //ICMS icms = det.imposto.ICMS;
            var icms = new ICMS();
            if (Nota.Empresa.TipoCRT != EmpresaCRT.SimplesNacional)
            {
                                
                switch (aNotaItem.TAG_CST)
                {
                    case "00":
                        icms.TipoICMS = new ICMS00
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            modBC = (DeterminacaoBaseIcms)aNotaItem.modBC_N13, // Modalidade de determinação da Base de Cálculo - ver Manual
                            vBC = aNotaItem.BaseICMS, // Valor da Base de Cálculo do ICMS
                            pICMS = aNotaItem.AliqICMS, // Alíquota do ICMS em Percentual
                            vICMS = aNotaItem.ValorICMS, // Valor do ICMS em Reais
                            pFCP = aNotaItem.pFCP_N17b, // Percentual do Fundo de Combate à Pobreza
                            vFCP = aNotaItem.vFCP_N17c // Valor do Fundo de Combate à Pobreza
                        };                                               
                        break;

                    case "10":
                        icms.TipoICMS = new ICMS10
                        {
                            orig = (OrigemMercadoria) aNotaItem.orig_N11,// Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms) Convert.ToInt32(aNotaItem.TAG_CST),// Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            modBC = (DeterminacaoBaseIcms)aNotaItem.modBC_N13,// Modalidade de determinação da Base de Cálculo - ver Manual
                            vBC = aNotaItem.BaseICMS,// Valor da Base de Cálculo do ICMS
                            pICMS = aNotaItem.AliqICMS,// Alíquota do ICMS em Percentual
                            vICMS = aNotaItem.ValorICMS,// Valor do ICMS em Reais
                            modBCST = (DeterminacaoBaseIcmsSt) aNotaItem.modBCST_N18,
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,
                            vBCST = aNotaItem.BaseICMSSubst,
                            pICMSST = aNotaItem.pICMSST_N22,
                            vICMSST = aNotaItem.ValorRetidoICMS,
                            vBCFCP = aNotaItem.vBCFCP_N17a, // Valor da Base de Cálculo do FCP
                            pFCP = aNotaItem.pFCP_N17b, // Percentual do Fundo de Combate à Pobreza
                            vFCP = aNotaItem.vFCP_N17c, // Valor do Fundo de Combate à Pobreza
                            vBCFCPST = aNotaItem.vBCFCPST_N23a, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPST = aNotaItem.pFCPST_N23b, // Percentual do FCP retido por Substituição Tributária
                            vFCPST = aNotaItem.vFCPST_N23d // Valor do FCP retido por Substituição Tributária
                        };
                        break;

                    case "20":
                        var icms20 = new ICMS20
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            modBC = (DeterminacaoBaseIcms)aNotaItem.modBC_N13, // Modalidade de determinação da Base de Cálculo - ver Manual
                            pRedBC = aNotaItem.pRedBC_N14,// Modalidade de determinação da Base de Cálculo - ver Manual
                            vBC = aNotaItem.BaseICMS, // Valor da Base de Cálculo do ICMS
                            pICMS = aNotaItem.AliqICMS, // Alíquota do ICMS em Percentual
                            vICMS = aNotaItem.ValorICMS, // Valor do ICMS em Reais
                            vBCFCP = aNotaItem.vBCFCP_N17a, // Valor da Base de Cálculo do FCP
                            pFCP = aNotaItem.pFCP_N17b, // Percentual do Fundo de Combate à Pobreza
                            vFCP = aNotaItem.vFCP_N17c, // Valor do Fundo de Combate à Pobreza
                        };

                        if (aNotaItem.motDesICMS != 0)
                        {
                            icms20.vICMSDeson = aNotaItem.vICMSDeson;
                            icms20.motDesICMS = (MotivoDesoneracaoIcms) aNotaItem.motDesICMS;
                        }                            

                        icms.TipoICMS = icms20;
                        break;

                    case "30":
                        var icms30 = new ICMS30
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            modBCST = (DeterminacaoBaseIcmsSt)aNotaItem.modBCST_N18, // Modalidade de determinação da Base de Cálculo - ver Manual
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,// Modalidade de determinação da Base de Cálculo - ver Manual
                            vBCST = aNotaItem.BaseICMSSubst, // Valor da Base de Cálculo do ICMS
                            pICMSST = aNotaItem.pICMSST_N22, // Alíquota do ICMS em Percentual
                            vICMSST = aNotaItem.ValorRetidoICMS, // Valor do ICMS em Reais
                            vBCFCPST = aNotaItem.vBCFCPST_N23a, // Valor da Base de Cálculo do FCP
                            pFCPST = aNotaItem.pFCPST_N23b, // Percentual do Fundo de Combate à Pobreza
                            vFCPST = aNotaItem.vFCPST_N23d, // Valor do Fundo de Combate à Pobreza
                        };

                        if (aNotaItem.motDesICMS != 0)
                        {
                            icms30.vICMSDeson = aNotaItem.vICMSDeson;
                            icms30.motDesICMS = (MotivoDesoneracaoIcms) aNotaItem.motDesICMS;
                        }
                        icms.TipoICMS = icms30;
                        break;

                    case "40":
                        var icms40 = new ICMS40
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST) // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                        };

                        if (aNotaItem.motDesICMS != 0)
                        {
                            icms40.vICMSDeson = aNotaItem.vICMSDeson;
                            icms40.motDesICMS = (MotivoDesoneracaoIcms) aNotaItem.motDesICMS;
                        }

                        icms.TipoICMS = icms40;
                        break;

                    case "51":
                        var icms51 = new ICMS51
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            vBCFCP = aNotaItem.vBCFCP_N17a, // Valor da Base de Cálculo do FCP
                            pFCP = aNotaItem.pFCP_N17b, // Percentual do Fundo de Combate à Pobreza
                            vFCP = aNotaItem.vFCP_N17c // Valor do Fundo de Combate à Pobreza
                        };

                        icms.TipoICMS = icms51;
                        break;

                    case "60":
                        var icms60 = new ICMS60
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            //vBCSTRet = this.FormataTDEC_1302(aNotaItem.BaseICMSSubst).ToDecimal(), // Valor da Base de Cálculo do FCP
                            //vICMSSTRet =,
                            //vBCFCPSTRet =, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPSTRet = aNotaItem.pFCPST_N23b, // Percentual do FCP retido por Substituição Tributária
                            pST =  0.00m, // Alíquota suportada pelo Consumidor Final
                            vFCPSTRet = aNotaItem.vFCPST_N23d, // Valor do FCP retido por Substituição Tributária
                            vBCSTRet = aNotaItem.ValorIsentoICMS, // Valor do BC do ICMS ST retido na UF remetente
                            vICMSSTRet = aNotaItem.ValorRetidoICMS // Valor do ICMS ST retido na UF

                        };

                        icms.TipoICMS = icms60;             
                        break;
                    case "70":
                        var icms70 = new ICMS70
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            modBC = (DeterminacaoBaseIcms)aNotaItem.modBC_N13, // Modalidade de determinação da Base de Cálculo - ver Manual
                            pRedBC = aNotaItem.pRedBC_N14, // Modalidade de determinação da Base de Cálculo - ver Manual
                            vBC = aNotaItem.BaseICMS, // Valor da Base de Cálculo do ICMS
                            pICMS = aNotaItem.AliqICMSNormal, // Alíquota do ICMS em Percentual
                            vICMS = aNotaItem.ValorICMS, // Valor do ICMS em Reais
                            modBCST = (DeterminacaoBaseIcmsSt) aNotaItem.modBCST_N18,
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,
                            vBCST = aNotaItem.BaseICMSSubst, // Valor do ICMS em Reais
                            pICMSST = aNotaItem.pICMSST_N22,
                            vICMSST = aNotaItem.ValorRetidoICMS,
                            vBCFCPST = aNotaItem.vBCFCPST_N23a, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPST = aNotaItem.pFCPST_N23b, // Percentual do FCP retido por Substituição Tributária
                            vFCPST = aNotaItem.vFCPST_N23d, // Valor do FCP retido por Substituição Tributária
                            vBCFCP = aNotaItem.vBCFCP_N17a, // Valor da Base de Cálculo do FCP
                            pFCP = aNotaItem.pFCP_N17b, // Percentual do Fundo de Combate à Pobreza
                            vFCP = aNotaItem.vFCP_N17c // Valor do Fundo de Combate à Pobreza
                        };

                        icms.TipoICMS = icms70;
                        break;

                    case "90":
                        var icms90 = new ICMS90
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11, // Origemd da Mercadoria (0-Nacional, 1-Estrangeira, 2-Estrangeira adiquirida no Merc. Interno)
                            CST = (Csticms)Convert.ToInt32(aNotaItem.TAG_CST), // Tipo da Tributação do ICMS (00 - Integralmente) ver outras formas no Manual
                            modBC = (DeterminacaoBaseIcms)aNotaItem.modBC_N13, // Modalidade de determinação da Base de Cálculo - ver Manual
                            vBC = aNotaItem.BaseICMS, // Valor da Base de Cálculo do ICMS
                            pICMS = aNotaItem.AliqICMSNormal, // Alíquota do ICMS em Percentual
                            vICMS = aNotaItem.ValorICMS, // Valor do ICMS em Reais
                            modBCST =  (DeterminacaoBaseIcmsSt) aNotaItem.modBCST_N18,
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,
                            vBCST = aNotaItem.BaseICMSSubst, // Valor do ICMS em Reais
                            pICMSST = aNotaItem.pICMSST_N22,
                            vICMSST = aNotaItem.ValorRetidoICMS,
                            vBCFCPST = aNotaItem.vBCFCPST_N23a, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPST = aNotaItem.pFCPST_N23b, // Percentual do FCP retido por Substituição Tributária
                            vFCPST = aNotaItem.vFCPST_N23d, // Valor do FCP retido por Substituição Tributária
                            vBCFCP = aNotaItem.vBCFCP_N17a, // Valor da Base de Cálculo do FCP
                            pFCP = aNotaItem.pFCP_N17b, // Percentual do Fundo de Combate à Pobreza
                            vFCP = aNotaItem.vFCP_N17c // Valor do Fundo de Combate à Pobreza
                        };

                        icms.TipoICMS = icms90;
                        break;
                }
                
            }
            else //Empresa Simples
            {
                switch (aNotaItem.TAG_CST)
                {
                    case "101":
                        var icmssn101 = new ICMSSN101
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11,
                            CSOSN = (Csosnicms) Convert.ToInt32(aNotaItem.TAG_CST),
                            pCredSN = aNotaItem.pCredSN_N29,
                            vCredICMSSN = aNotaItem.vCredICMSSN_N30
                        };

                        icms.TipoICMS = icmssn101;
                        break;
                    case "102": goto case "400";
                    case "103": goto case "400";
                    case "201":
                        var icmssn201 = new ICMSSN201
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11,
                            CSOSN = (Csosnicms)Convert.ToInt32(aNotaItem.TAG_CST),
                            modBCST = (DeterminacaoBaseIcmsSt) aNotaItem.modBCST_N18,
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,
                            vBCST = aNotaItem.BaseICMSSubst,
                            pICMSST = aNotaItem.pICMSST_N22,
                            vICMSST = aNotaItem.ValorRetidoICMS,
                            pCredSN = aNotaItem.pCredSN_N29,
                            vCredICMSSN = aNotaItem.vCredICMSSN_N30,
                            vBCFCPST = 0.00m, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPST = 0.00m, // Percentual do FCP retido por Substituição Tributária
                            vFCPST = 0.00m // Valor do FCP retido por Substituição Tributária
                        };

                        icms.TipoICMS = icmssn201;
                        break;
                    case "203": goto case "202";
                    case "202":
                        var icmssn202 = new ICMSSN202
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11,
                            CSOSN = (Csosnicms)Convert.ToInt32(aNotaItem.TAG_CST),
                            modBCST = (DeterminacaoBaseIcmsSt) aNotaItem.modBCST_N18,
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,
                            vBCST = aNotaItem.BaseICMSSubst,
                            pICMSST = aNotaItem.pICMSST_N22,
                            vICMSST = aNotaItem.ValorRetidoICMS,                            
                            vBCFCPST = aNotaItem.vBCFCPST_N23a, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPST = aNotaItem.pFCPST_N23b, // Percentual do FCP retido por Substituição Tributária
                            vFCPST = aNotaItem.vFCPST_N23d // Valor do FCP retido por Substituição Tributária
                        };

                        icms.TipoICMS = icmssn202;                       
                        break;
                    case "300": goto case "400";
                    case "400":
                        var icmssn102 = new ICMSSN102
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11,
                            CSOSN = (Csosnicms) Convert.ToInt32(aNotaItem.TAG_CST)
                        };

                        icms.TipoICMS = icmssn102;
                        break;
                    case "500":
                        var icmssn500 = new ICMSSN500
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11,
                            CSOSN = (Csosnicms) Convert.ToInt32(aNotaItem.TAG_CST),
                            vBCSTRet = aNotaItem.ValorIsentoICMS,
                            vICMSSTRet = aNotaItem.ValorIsentoICMS,
                            pST = 0.00m, // Alíquota suportada pelo Consumidor Final
                            vBCFCPSTRet = 0.00m, // Valor da Base de Cálculo do FCP retido anteriormente por ST
                            pFCPSTRet = 0.00m, // Percentual do FCP retido anteriormente por Substituição Tributária
                            vFCPSTRet = 0.00m // Valor do FCP retido por Substituição Tributária
                        };

                        icms.TipoICMS = icmssn500;
                        break;
                    case "900":
                        var icmssn900 = new ICMSSN900
                        {
                            orig = (OrigemMercadoria)aNotaItem.orig_N11,
                            CSOSN = (Csosnicms) Convert.ToInt32(aNotaItem.TAG_CST),
                            modBC = (DeterminacaoBaseIcms) aNotaItem.modBC_N13,
                            pRedBC = aNotaItem.pRedBC_N14,
                            vBC = aNotaItem.BaseICMS,
                            pICMS = aNotaItem.AliqICMSNormal,
                            vICMS = aNotaItem.ValorICMS,
                            modBCST = (DeterminacaoBaseIcmsSt) aNotaItem.modBCST_N18,
                            pMVAST = aNotaItem.pMVAST_N19,
                            pRedBCST = aNotaItem.pRedBCST_N20,
                            vBCST = aNotaItem.BaseICMSSubst,
                            pICMSST = aNotaItem.pICMSST_N22,
                            vICMSST = aNotaItem.ValorRetidoICMS,
                            pCredSN = aNotaItem.pCredSN_N29,
                            vCredICMSSN = aNotaItem.vCredICMSSN_N30,
                            vBCFCPST = aNotaItem.vBCFCPST_N23a, // Valor da Base de Cálculo do FCP retido por Substituição Tributária
                            pFCPST = aNotaItem.pFCPST_N23b, // Percentual do FCP retido por Substituição Tributária
                            vFCPST = aNotaItem.vFCPST_N23d // Valor do FCP retido por Substituição Tributária
                        };

                        icms.TipoICMS = icmssn900;
                        break;
                }
                
            }
            det.imposto.ICMS = icms;
            
            if ((Empresa.UF != Nota.PessoaUF) && (Nota.PessoaUF != "EX") && Convert.ToBoolean(IndFinal))
            {
                var icmsufdest = new ICMSUFDest
                {
                    vBCUFDest = aNotaItem.BaseICMS,
                    vBCFCPUFDest = 0.00m, // Valor da BC FCP na UF de destino
                    pFCPUFDest = 0.00m,
                    pICMSUFDest = aNotaItem.AliqInterna,
                    pICMSInter = aNotaItem.pICMSInter,
                    pICMSInterPart = PegarPercentualPartilha(),
                    vFCPUFDest = 0.00m,
                    vICMSUFDest = aNotaItem.vICMSUFDest_NA15,
                    vICMSUFRemet = aNotaItem.vICMSUFRemet_NA17,
                };

                det.imposto.ICMSUFDest = icmsufdest;
            }
            
            //IPI
            var ipi = new IPI();
            var ipiGeral = new IPIGeral();            
            if (aNotaItem.CST_Ipi == "00" || aNotaItem.CST_Ipi == "49" || aNotaItem.CST_Ipi == "50" || aNotaItem.CST_Ipi == "99")
            {                
                ipi.cEnq = Convert.ToInt32(aNotaItem.cEnq_O06);

                ipiGeral.CST = Funcoes.RetornaCSTIPI(aNotaItem.CST_Ipi);
                ipiGeral.vBC = aNotaItem.vBC_O10;
                ipiGeral.qUnid = null;
                ipiGeral.vUnid = null;
                ipiGeral.pIPI = aNotaItem.pIPI_O13;
                ipiGeral.vIPI = aNotaItem.vIPI_O14;
            }
            else
            {
                ipi.cEnq = Convert.ToInt32(aNotaItem.cEnq_O06);
                
                ipiGeral.CST = Funcoes.RetornaCSTIPI(aNotaItem.CST_Ipi);
            }
            ipi.TipoIPI = ipiGeral.ObterIPIBasico();
            det.imposto.IPI = ipi;
            
            // PIS
            var pis = new PIS();
            var pisGeral = new PISGeral();
            if (aNotaItem.CST_Pis == "04" || aNotaItem.CST_Pis == "06" || aNotaItem.CST_Pis == "07" || aNotaItem.CST_Pis == "08" || aNotaItem.CST_Pis == "09")
            {  
                pisGeral.CST = Funcoes.RetornaCSTPIS(aNotaItem.CST_Pis);
            }
            else
            {
                pisGeral.CST = Funcoes.RetornaCSTPIS(aNotaItem.CST_Pis); // Codigo de Situacao Tributária - ver opções no Manual
                pisGeral.vBC = aNotaItem.vBC_Q07; // Valor da Base de Cálculo do PIS
                pisGeral.pPIS = aNotaItem.pPIS_Q08; // Alíquota em Percencual do PIS
                pisGeral.vPIS = aNotaItem.vPIS_Q09; // Valor do PIS em Reais
            }            
            pis.TipoPIS = pisGeral.ObterPISBasico();
            det.imposto.PIS = pis;
            
            // COFINS
            var confis = new COFINS();
            var cofinsGeral = new COFINSGeral();
            if (aNotaItem.CST_Pis == "04" || aNotaItem.CST_Pis == "06" || aNotaItem.CST_Pis == "07" || aNotaItem.CST_Pis == "08" || aNotaItem.CST_Pis == "09")
            {
                cofinsGeral.CST = Funcoes.RetornaCSTCOFINS(aNotaItem.CST_Cofins);
            }
            else
            {                
                cofinsGeral.CST = Funcoes.RetornaCSTCOFINS(aNotaItem.CST_Cofins); // Código de Situacao Tributária - ver opções no Manual
                cofinsGeral.vBC = aNotaItem.vBC_S07; // Valor da Base de Cálculo do COFINS
                cofinsGeral.pCOFINS = aNotaItem.pCOFINS_S08; // Alíquota do COFINS em Percentual
                cofinsGeral.vCOFINS = aNotaItem.vCOFINS_S11; // Valor do COFINS em Reais
            }

            confis.TipoCOFINS = cofinsGeral.ObterCOFINSBasico();
            det.imposto.COFINS = confis;

            if (Nota.EnviaTagTotalImposto)
                det.imposto.vTotTrib = aNotaItem.TotalImpostos;

            //DadosNotaItemExportacao(aNotaItem);
            //DadosNotaItemImportacao(aNotaItem);

            //Informações Adicionais
            //if (aNotaItem.InfAdicionais != null)
            if (aNotaItem.InfAdicionais != "")
            {
                det.infAdProd = aNotaItem.InfAdicionais;
            }

            if (aNotaItem.TextoLei != null)
                obs = obs + " " + aNotaItem.TextoLei.Trim();
           
            return det;
        }
        #endregion

        #region METODO A SER IMPLEMENTADO
        // Rastreabilidade de produto * Informar apenas quando se tratar de produto a ser rastreado posteriormente
        private void IncluirRastreabilidadeProduto()
        {
            

        }

        private void DadosNotaItemExportacao(INotaItem aNotaItem)
        {
           
        }
        private void DadosNotaItemImportacao(INotaItem aNotaItem)
        {
            //SpdNFeDataSetX.SetCampo("nDI_I19=" + aNotaItem.I19_nDI);
            //if (aNotaItem.I20_dDI.HasValue)
            //    SpdNFeDataSetX.SetCampo("dDI_I20=" + aNotaItem.I20_dDI.Value.ToString("yyyy-MM-dd"));
            //SpdNFeDataSetX.SetCampo("xLocDesemb_I21=" + aNotaItem.I21_xLocDesemb);
            //SpdNFeDataSetX.SetCampo("UFDesemb_I22=" + aNotaItem.I22_UFDesemb);
            //if (aNotaItem.I23_dDesemb.HasValue)
            //    SpdNFeDataSetX.SetCampo("dDesemb_I23=" + aNotaItem.I23_dDesemb.Value.ToString("yyyy-MM-dd"));
            //SpdNFeDataSetX.SetCampo("cExportador_I24=" + aNotaItem.I24_cExportador);

            //SpdNFeDataSetX.SetCampo("vBC_P02=" + FormataTDEC_1302(aNotaItem.P02_vBC));
            //SpdNFeDataSetX.SetCampo("vDespAdu_P03=" + FormataTDEC_1302(aNotaItem.P03_vDespAdu));
            //SpdNFeDataSetX.SetCampo("vII_P04=" + FormataTDEC_1302(aNotaItem.P04_vII));
            //SpdNFeDataSetX.SetCampo("vIOF_P05=" + FormataTDEC_1302(aNotaItem.P05_vIOF));

            //if (aNotaItem.ViaTransp != 0)
            //{
            //    SpdNFeDataSetX.SetCampo("tpViaTransp_I23a=" + aNotaItem.ViaTransp);
            //    if (aNotaItem.ViaTransp == 1)
            //    {
            //        SpdNFeDataSetX.SetCampo("vAFRMM_I23b=" + aNotaItem.ValorFreteRenovacaoMarinhaMercante);
            //    }
            //}

            //SpdNFeDataSetX.IncluirParte("adi");
        }
        #endregion

        #region DadosCobrança
        private void DadosCobranca()
        {
            var cobr = new cobr();
            cobr.dup = new List<dup>();
            foreach (INotaParcela parcela in Nota.NotaParcelas)
            {                          

                if (!String.IsNullOrEmpty(parcela.FaturaNumero))
                {
                    cobr.fat = new fat
                    {
                        nFat = parcela.FaturaNumero, // Número da Farura
                        vOrig = parcela.FaturaValorOriginal, // Valor Original da Fatura
                        vLiq = parcela.FaturaValorLiquido, // Valor Líquido da Fatura
                        //vDesc = Nota.ValorDesconto // Valor Desconto da Fatura
                        vDesc = VerificarPreechimentoDesconto(Nota.DtEmissao) // Valor Desconto da Fatura
                    };
                }

                if (parcela.FormaPagamento == "14" | parcela.FormaPagamento == "15")
                {
                    cobr.dup.Add(new dup
                    {
                        nDup = parcela.Sequencia.ToString(), // Número da Duplicata
                        dVenc = parcela.Vencimento, // Data de Vencimento da Duplicata
                        vDup = parcela.Valor // Valor da Duplicata
                    });                            
                }
                               
            }
            _nfe.infNFe.cobr = cobr;

        }
        #endregion

        #region DadosTotalizadores
        private void DadosTotalizadores(IList<INotaItem> notaItems, decimal totalProduto, decimal totalNota)
        {
            var icmsTot = new ICMSTot();

            icmsTot.vICMSDeson = notaItems.Sum(a => a.vICMSDeson);
            //icmsTot.vFCP = 0; // Valor Total do FCP
            icmsTot.vBC = notaItems.Sum(a => a.BaseICMS); // Base de Cálculo do ICMS
            icmsTot.vICMS = notaItems.Sum(a => a.ValorICMS); // Valor Total do ICMS
            icmsTot.vFCPUFDest = 0; // Valor Total do ICMS
            icmsTot.vICMSUFDest = notaItems.Sum(a => a.vICMSUFDest_NA15); // Valor Total do ICMS Destino
            icmsTot.vICMSUFRemet = notaItems.Sum(a => a.vICMSUFRemet_NA17); // Valor Total do ICMS Origem
            icmsTot.vFCP = notaItems.Sum(a => a.vFCP_N17c); // Valor Total do FCP
            icmsTot.vFCPST = notaItems.Sum(a => a.vFCPST_N23d); // Valor Total do FCP
            icmsTot.vFCPSTRet = notaItems.Sum(a => a.vFCPST_N23d);  // Valor Total do FCP retido anteriormente por Substituição Tributária
            icmsTot.vIPIDevol = 0; // Valor Total do IPI devolvido

            icmsTot.vBCST = notaItems.Sum(a => a.BaseICMSSubst); // Base de Cálculo do ICMS Subst. Tributária
            icmsTot.vST = notaItems.Sum(a => a.ValorRetidoICMS); // Valor Total do ICMS Sibst. Tributária
            icmsTot.vProd = totalProduto; // Valor Total de Produtos

            icmsTot.vFrete = Nota.ValorSeguro; // Valor Total do Seguro

            icmsTot.vSeg = Nota.ValorSeguro; // Valor Total do Seguro

            icmsTot.vDesc = Nota.ValorDesconto; // Valor Total de Desconto

            icmsTot.vII = 0; // Valor Total do II
            icmsTot.vIPI = notaItems.Sum(a => a.vIPI_O14); // Valor Total do IPI
            icmsTot.vPIS = notaItems.Sum(a => a.vPIS_Q09); // Valor Toal do PIS
            icmsTot.vCOFINS = notaItems.Sum(a => a.vCOFINS_S11); // Valor Total do COFINS
            icmsTot.vOutro = Nota.OutrasDespesas; // OUtras Despesas Acessórias

            icmsTot.vNF = totalNota; // Valor Total da NFe - Versão Trial só aceita NF até R$ 1.00

            if (Nota.EnviaTagTotalImposto)
                icmsTot.vTotTrib = Convert.ToDecimal(notaItems.Sum(a => a.TotalImpostos)); //Valor total dos impostos de acordo com a NT 2013-003

            icmsTot.vII = Nota.W11_vII ; //Valor Total Imposto de Importação

            //Verifica se possui observação
            String observacaoTotal = "";
            if (!String.IsNullOrEmpty(Nota.ObservacaoSistema))
                observacaoTotal += Nota.ObservacaoSistema.Trim();

            if (!String.IsNullOrEmpty(Nota.ObservacaoUsuario))
                observacaoTotal += " " + Nota.ObservacaoUsuario.Trim();

            //SpdNFeDataSetX.SetCampo(("infCpl_Z03=" + observacaoTotal));

            _nfe.infNFe.total = new total { ICMSTot = icmsTot };
        }
        #endregion

        #region PegaPercetualPartilha
        private decimal PegarPercentualPartilha()
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
#pragma warning disable CS0162 // Código inacessível detectado
                case 2015: return 0.00m; break;
#pragma warning restore CS0162 // Código inacessível detectado
#pragma warning disable CS0162 // Código inacessível detectado
                case 2016: return 40.00m; break;
#pragma warning restore CS0162 // Código inacessível detectado
#pragma warning disable CS0162 // Código inacessível detectado
                case 2017: return 60.00m; break;
#pragma warning restore CS0162 // Código inacessível detectado
#pragma warning disable CS0162 // Código inacessível detectado
                case 2018: return 80.00m; break;
#pragma warning restore CS0162 // Código inacessível detectado
#pragma warning disable CS0162 // Código inacessível detectado
                case 2019: return 100.00m; break;
#pragma warning restore CS0162 // Código inacessível detectado
#pragma warning disable CS0162 // Código inacessível detectado
                default: return 100.00m; break; 
#pragma warning restore CS0162 // Código inacessível detectado
            }
        }
        #endregion

        #region DadosTransporte
        private void DadosTransporte()
        {
            var transporte = new transp();
            
            transporte.modFrete = (ModalidadeFrete) Convert.ToInt32(Nota.TipoFrete); // Modalidade de Frete
            //transporte.modFrete = ModalidadeFrete.mfSemFrete; // Modalidade de Frete
            if (Nota.TransNome != null && Nota.TransNome != String.Empty)
            {
                transporte.transporta = new transporta();
                string auxCnpjCpf = Funcoes.LimpaStr(Nota.TransCNPJCPF);

                if (auxCnpjCpf.Length == 11)
                    transporte.transporta.CPF = auxCnpjCpf;
                else
                    transporte.transporta.CNPJ = auxCnpjCpf; // CNPJ do Transportador

                transporte.transporta.xNome = Nota.TransNome; // Nome do Transportador
                transporte.transporta.IE = Funcoes.LimpaStr(Nota.TransInscricao); //  Inscrição estadual do Transportador
                transporte.transporta.xEnder = Nota.TransEndereco; // End Subereço do Transportador
                transporte.transporta.xMun = Nota.TransCidade; // Nome do Município do Transportador
                transporte.transporta.UF = Nota.TransUF; // Sigla do Estado do Transportador                
                // Dados do Veículo de Transporte
                transporte.veicTransp = new veicTransp
                {
                    placa = Funcoes.LimpaStr(Nota.TransPlaca), // Placa do Veículo
                    UF = Nota.TransPlacaUF // Sigla do Estado da Placa do Veículo
                };               
                // Dados da Carga Transportada
            }
            
            transporte.vol = new List<vol>();
            var volume = new vol();
            transporte.vol.Add(volume); 

            if(Nota.VolumeQuant != "")
            {
                volume.qVol = Convert.ToInt32(Nota.VolumeQuant); // Quantidade de Volumes transportados
            }
            else
            {
                volume.qVol = 0; // Quantidade de Volumes transportados
                volume.esp = Nota.VolumeEspecie; // Espécie de Carga Transportada
                volume.marca = Nota.VolumeMarca; // MArca da Carga Transportada
                volume.nVol = Nota.VolumeNumero; // Numeração dos Volumes transportados
                volume.pesoL = Nota.VolumePesoLiquido; // Peso Líquido
                volume.pesoB = Nota.VolumePesoBruto; // Peso Bruto
            }

            _nfe.infNFe.transp = transporte;
        
        }
        #endregion

        #region Informações de Pagamento
        private void InformacoesPagamento()
        {
            var pagamento = new pag();
            pagamento.detPag = new List<detPag>();
            foreach (INotaParcela formasPagamento in Nota.NotaParcelas)
            {
                
                //pagamento.detPag.Add(new detPag
                //{
				//	indPag = 
                //    tPag = formasPagamento.FormaPagamento,
                //    vPag = formasPagamento.Valor
                //});
				var detpag = new detPag
				{					
                    tPag = (FormaPagamento) Convert.ToInt64(formasPagamento.FormaPagamento),
                    vPag = formasPagamento.Valor
				};
				
				
				//if (Nota.NotaParcelas.Count == 1 && Nota.NotaParcelas.First().Vencimento == Nota.DtEmissao)
				//{
				//	detpag.indPag = IndicadorPagamentoDetalhePagamento.ipDetPgVista; //Indicador da Forma de Pgto (0- a Vista, 1 a Prazo)    
				//}
				//else
				//{
				//	detpag.indPag = IndicadorPagamentoDetalhePagamento.ipDetPgPrazo; //Indicador da Forma de Pgto (0- a Vista, 1 a Prazo)
				//}
				
				pagamento.detPag.Add(detpag);					

                //pagamento.vTroco = 0;
            }

            _nfe.infNFe.pag = new List<pag> { pagamento };
        }
        #endregion

        #region ValidaDadosNFe
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
        #endregion

        #region GerarNFe
        public override IDictionary<string, string> GerarNFe()
        {       
            if (Nota.NotaComplementada == null)
                return GerarNotaNormal();
            else
                return GerarNotaComplementar();
        }
        #endregion

        #region GerarNotaNormal
        private IDictionary<string, string> GerarNotaNormal()
        {           
            IDictionary<string, string> retorno = new Dictionary<string, string>();
            String aXmlNota = "";           
            String envioDaNota = "";
            NotaStatusAnterior = Nota.Status;
            Nota.Status = Nota.Status != "8" ? "-1" : "8";

            if ((retorno = ValidaDadosNFe()).Count > 0)
            {
                throw new Exception("Nota não é validada");
            }  
          
            aXmlNota = new ServicosNFe(_configuracoes.CfgServico).NfeStatusServico().RetornoCompletoStr;          
            if ((retorno = VerificaStatusServico(aXmlNota)) != null)
            {
                throw new ServidorOfflineException(null, "O servidor do serviço está offline.");
            }

            aXmlNota = GeraXmlNota().Trim();
            SalvarXmlArquivo(aXmlNota, "UltimoXmlGerado.xml");

            if (FormaEmissao == TipoEmissaoZeus.teEPEC)
            {

                Nota.XmlLogEnvNFe = aXmlNota;

                NFeFacade facade = new NFeFacade();
                var nfe = new NFe.Classes.NFe().CarregarDeArquivoXml(aXmlNota);
                aXmlNota = facade.EnviarEPEC(Nota.Id, 1, nfe, "4.00", _configuracoes.CfgServico).RetornoCompletoStr;              
                
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
                NFeFacade facade = new NFeFacade();
                var retornoEnvio = facade.EnviarNFe(Nota.Numero, _nfe, _configuracoes.CfgServico);

                aXmlNota = retornoEnvio.RetornoCompletoStr;
                
                envioDaNota = aXmlNota;                

                Nota.NumeroRecibo = TrataRetornoEnvioNumeroRecibo(aXmlNota);
                Nota.LogEnvio = _nfe.infNFe.ide.nNF + "-env-lot.xml";
                Nota.XmlLogEnvNFe = retornoEnvio.EnvioStr;
                //Nota.XmlLogEnvNFe = Funcoes.AbrirArquivo(this._cfgServico().DiretorioSalvarXml + "\\" + Nota.LogEnvio).Replace("UTF-8", "UTF-16"); ;
            }

            return envioDaNota.DesmembrarXml();
        }
        #endregion

        #region RetornarCStat
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
        #endregion

        #region MontaCodigoErrosNotaEpec
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
        #endregion

        #region SalvaXmlArquivo
        private void SalvarXmlArquivo(string xmlNota, string nomeArquivo)
        {
            StreamWriter stream = new StreamWriter(nomeArquivo);
            stream.Write(xmlNota);
            stream.Close();
        }
        #endregion

        #region GerarNotaComplementar
        public Dictionary<string, string> GerarNotaComplementar()
        {           

            this.DadosNFe(FinalidadeNFeZeus.fnComplementar);
            if (Nota.NotaComplementada.Status != "2")
                throw new Exception("Só é possível gerar nota complementar de notas autorizadas.");

 
            _nfe.infNFe.ide.NFref = new List<NFref> { new NFref { refNFe = Nota.NotaComplementada.ChaveNota } };

            this.DadosEmitente();
            this.DadosDestinatario();
            this.InformacoesPagamento();

            int seq = 0;
            foreach (INotaItem objNotaItem in Nota.NotaItems)
            {
                seq += 1;
                _nfe.infNFe.det.Add(this.DadosItem(objNotaItem));
            }

            //DadosCobranca();
            //InformacoesPagamento();
            DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
            _nfe.infNFe.transp = new transp
            {
                modFrete = ModalidadeFrete.mfContaEmitenteOumfContaRemetente // Modalidade de Frete = 0 por ser complementar
            };

            _nfe.Assina(_configuracoes.CfgServico);

            string xmlNota = _nfe.ObterXmlString();

            NFeFacade facade = new NFeFacade();
            var nf = facade.EnviarNFe(Nota.NotaComplementada.Numero, _nfe, _configuracoes.CfgServico);

            xmlNota = nf.RetornoCompletoStr;

            Nota.NumeroRecibo = TrataRetornoEnvioNumeroRecibo(xmlNota);
            Nota.LogEnvio = nf.EnvioStr;
            Nota.XmlLogEnvNFe = nf.EnvioStr;
            //Nota.XmlLogEnvNFe = Funcoes.AbrirArquivo(this._cfgServico().DiretorioSalvarXml + "\\" + Nota.LogEnvio).Replace("UTF-8", "UTF-16"); ;

            return new Dictionary<string, string>();
        }
        #endregion

        #region GeraXmlNota
        public override string GeraXmlNota()
        {
            string aXmlNota;          

            try
            { 
                if (BDevolucao)
                {
                    this.DadosNFe(FinalidadeNFeZeus.fnDevolucao);
                }
                else
                    this.DadosNFe(FinalidadeNFeZeus.fnNormal);

                this.DadosEmitente();
                this.DadosDestinatario();
                this.DadosEntrega();
                int seq = 0;
                foreach (INotaItem objNotaItem in Nota.NotaItems)
                {
                    seq += 1;
                    _nfe.infNFe.det.Add(this.DadosItem(objNotaItem));
                }

                DadosCobranca();
                InformacoesPagamento();
                DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
                DadosTransporte();

                _nfe.Assina(_configuracoes.CfgServico);                               

                aXmlNota = _nfe.ObterXmlString();             
              
                Nota.ChaveNota = _nfe.infNFe.Id.Replace("NFe", "");


                return aXmlNota;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region GerarNotaReferenciada
        public void GerarNotaReferenciada(INota notaReferenciada)
        {
            //MessageBox.Show("Teste");
            //if (notaReferenciada == null)
            //    throw new Exception("Não foi possível encontrar a nota referenciada. Por favor verifique.");

            //if (String.IsNullOrEmpty(notaReferenciada.ChaveNota))
            //    throw new Exception("Só é possível gerar nota referenciadas de notas que possuem chave.");

            string anoEmissao = notaReferenciada.DtEmissao.Year.ToString();
            string mesEmissao = notaReferenciada.DtEmissao.Month.ToString();
            string AAMM = anoEmissao.Substring(2) + mesEmissao;

            _nfe.infNFe.ide.NFref.Add(new NFref
            {
                refNFe = notaReferenciada.ChaveNota,
                refNF = new refNF
                {
                    cUF = (Estado)Convert.ToInt32(notaReferenciada.Empresa.UFIBGE),
                    AAMM = AAMM,
                    CNPJ = Funcoes.LimpaStr(notaReferenciada.Empresa.Cnpj),
                    mod = (refMod)notaReferenciada.ModeloDocto,
                    serie = Convert.ToInt32(notaReferenciada.Serie),
                    nNF = notaReferenciada.Numero
                }
            });

        }
        #endregion

        #region GerarXmlPreDanfe
        public override void GerarXmlPreDanfe()
        {
            string aXmlNota;

            try
            {

                if (BDevolucao)
                {
                    this.DadosNFe(FinalidadeNFeZeus.fnDevolucao);
                }
                else
                    this.DadosNFe(FinalidadeNFeZeus.fnNormal);

                this.DadosEmitente();
                this.DadosDestinatario();
                this.DadosEntrega();
                int seq = 0;
                foreach (INotaItem objNotaItem in Nota.NotaItems)
                {
                    seq += 1;
                    _nfe.infNFe.det.Add(this.DadosItem(objNotaItem));
                }

                DadosCobranca();
                InformacoesPagamento();
                DadosTotalizadores(Nota.NotaItems, Nota.TotalProduto, Nota.TotalNota);
                DadosTransporte();
                

                _nfe.Assina(_configuracoes.CfgServico);                              

                aXmlNota = _nfe.ObterXmlString();                

                Nota.ChaveNota = _nfe.infNFe.Id.Replace("NFe", "");

                NFeFacade facade = new NFeFacade();

                facade.VisualizarDanfe(aXmlNota);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region TrataRetornoEnvioNumeroRecibo
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
                //Nota.StatusMotivo = "Campo inconsistente: " + CapturaCampoErrado(xMotivo);
                Nota.StatusMotivo = xMotivo;

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
        #endregion

        #region AtribuiRetornoRecibo
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
                    Nota.LogRecibo = Nota.Numero + "-pro-rec.xml";
                    //Nota.XmlLogRecNFe = Funcoes.AbrirArquivo(this._cfgServico().DiretorioSalvarXml + "\\" + Nota.LogRecibo).Replace("UTF-8", "UTF-16"); ;
                    Nota.XmlLogRecNFe = xml;                  
                    Nota.XmlDestinatarioNFe = Funcoes.AbrirArquivo(DiretorioXML + "\\" + Nota.ChaveNota + "-nfe.xml").Replace("UTF-8", "UTF-16");                    
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
            #pragma warning disable CS0168 // A variável "e" está declarada, mas nunca é usada
            catch (Exception e)
            #pragma warning restore CS0168 // A variável "e" está declarada, mas nunca é usada
            {
                
                throw new XmlMalFormatadoException(retorno, "Ocorreram erros no processamento da nota.");
            }

            return retorno;

        }
        #endregion

        private string CapturaCampoErrado(string linha)
        {
            int ultimaAspa = linha.LastIndexOf('\'');
            int penultimaAspa = linha.Substring(0, ultimaAspa - 1).LastIndexOf('\'');

            return linha.Substring(penultimaAspa + 1, ultimaAspa - penultimaAspa);

        }

        #region VerificaStatusServico
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
        #endregion

        #region CancelarNFe
        public override IDictionary<string, string> CancelarNFe(string _motivo, string _usuario)
        {

            NFeFacade facade = new NFeFacade();
            var cancelarNFe = facade.CancelarNFe(Funcoes.LimpaStr(Empresa.Cnpj), Nota.Numero, 1, Nota.ChaveNota, Nota.NumeroProtocolo, _motivo, _configuracoes.CfgServico);

            string aXmlNota = cancelarNFe.RetornoCompletoStr;
                       
            if (aXmlNota == null || aXmlNota == "")
            {
                throw new SemRespostaDoServidorException(null);
            } 

            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlNota))));

            XElement nohInfCanc = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "retEvento" select c).Single();
            XElement noh = (from c in nohInfCanc.Elements() where c.Name.LocalName == "infEvento" select c).Single();
            string valorCStat = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();

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
        #endregion

        #region MontarObservação
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
        #endregion

        #region JuntarObservacao
        private string JuntarObservacao(string pObservacao, string pTexto)
        {
            if (!pObservacao.Contains(pTexto))
                pObservacao = pObservacao.TrimEnd() + " " + pTexto.TrimEnd();

            if (pObservacao.Length > 500)
                pObservacao = pObservacao.Substring(0, 500);

            return pObservacao;
        }
        #endregion

        #region ConsultarNFe
        public override IDictionary<string, string> ConsultarNFe()
        {
            List<string> retorno = new List<string>();
            string aXmlNota = "";

            NFeFacade facade = new NFeFacade();
            aXmlNota = facade.ConsultarNfe(Nota.ChaveNota, _configuracoes.CfgServico).RetornoCompletoStr;
           
            if (aXmlNota == null || aXmlNota == "")
            {
                throw new SemRespostaDoServidorException(null, "Não houve resposta do servidor na requisição de consulta.");
            }
            Nota.UltimoXmlRecebido = aXmlNota;

            return aXmlNota.DesmembrarXml();
        }
        #endregion

        #region ConsultarRecibo
        public override IDictionary<string, string> ConsultarRecibo()
        {
            List<string> retorno = new List<string>();
            string aXmlNota = "";
 
            NFeFacade facade = new NFeFacade();
            aXmlNota = facade.ConsultarReciboDeEnvio(Nota.NumeroRecibo, _configuracoes.CfgServico).RetornoCompletoStr;
                        
            if (aXmlNota == null || aXmlNota == "")
                throw new SemRespostaDoServidorException(null, "Não houve resposta do servidor no recebimento do recibo.");
            
            Nota.UltimoXmlRecebido = aXmlNota;

            Nota.NumeroProtocolo = String.Empty;

            if (String.IsNullOrEmpty(Nota.NumeroProtocolo))
            //    //return AtribuiRetornoRecibo(aXmlNota);
                AtribuiRetornoRecibo(aXmlNota);
            //else
                return aXmlNota.DesmembrarXml();
        }
        #endregion

        #region InutilizarNFe
        public override string InutilizarNFe(string _ano, string _serie, string _numeroInicio, string _numeroFim, string _justificativa, string _cnpj = null)
        {
            List<string> retorno = new List<string>();
            string aXmlNota = "";

            if(_cnpj == null)
            {
                _cnpj = Funcoes.LimpaStr(Empresa.Cnpj);
            }
                        
            NFeFacade facade = new NFeFacade();
            //aXmlNota = facade.InutilizarNumeracao(Convert.ToInt32(_ano), Funcoes.LimpaStr(Empresa.Cnpj), _justificativa, Convert.ToInt32(_numeroInicio), Convert.ToInt32(_numeroFim), Convert.ToInt32(_serie), this.MontaConfiguracoesDFe()).RetornoCompletoStr;
            aXmlNota = facade.InutilizarNumeracao(Convert.ToInt32(_ano), _cnpj, _justificativa, Convert.ToInt32(_numeroInicio), Convert.ToInt32(_numeroFim), Convert.ToInt32(_serie), _configuracoes.CfgServico).RetornoCompletoStr;

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
        #endregion














        //private void EnviarCCe(string _chaveNFe, string _textoCce, string _dataHoraEvento, string _aOrgao, string _aIDLote, int aSequenciaEvento, string aFusoHorario)
        //{           
        //    NFeFacade facade = new NFeFacade();
        //    facade.CartaCorrecao(Convert.ToInt32(_aIDLote), aSequenciaEvento, _chaveNFe, _textoCce, Funcoes.LimpaStr(Empresa.Cnpj), _cfgServico());

        //}


        // Implementação NFCe
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
