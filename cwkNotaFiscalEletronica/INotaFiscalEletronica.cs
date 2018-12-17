using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cwkNotaFiscalEletronica.Interfaces;
using System.IO;
using System.Xml.Linq;
using NFeX;
using NFeDataSetX;
using System.Reflection;
using System.Drawing.Printing;
using System.Collections.Specialized;
using System.Management;
using cwkNotaFiscalEletronica.Modelo;
using System.Net;
using System.Net.Http;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using DFe.Classes.Flags;
using System.Security.Cryptography.X509Certificates;
using DFe.Utils.Assinatura;
using cwkNotaFiscalEletronica.Zeus.Email;

namespace cwkNotaFiscalEletronica
{
    public abstract class INotaFiscalEletronica
    {
        public spdNFeX SpdNFeX { get; private set; }
        public spdNFeDataSetX SpdNFeDataSetX { get; protected set; }
        public Danfe Danfe { get; set; }
        public DanfeNFCe DanfeNFCe { get; set; }
        public IEmpresa Empresa { get; set; }
        public INota Nota { get; set; }
        public string Observacoes { get; set; }
        public TipoDoCertificado TipoDoCertificado { get; set; }
        public string DiretorioPadrao { get; private set; }
        public string DiretorioXML { get; private set; }
        protected cwkAmbiente CwkAmbiente { get; set; }
        protected TipoEmissao FormaEmissao { get; set; }
        //Configuração ZEUS
        public static ConfiguracaoApp _configuracoes;        

        public INotaFiscalEletronica(TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado, string _diretorioPadrao)
        {
            CwkAmbiente = _ambiente;
            SetDiretorioPadrao(_diretorioPadrao);           
            SetSpdNFeX(_tipoServidor, _tipoCertificado);
            _configuracoes = new ConfiguracaoApp();
            
        }

        #region Configuração NFeX Tecnospeed
        private void SetSpdNFeX(TipoEmissao _tipoServidor, TipoDoCertificado _tipoCertificado)
        {
            SpdNFeX = ((spdNFeX)FactoryIspdNFeX.Build(_tipoServidor, CwkAmbiente, _tipoCertificado, DiretorioPadrao));
        }
        #endregion

        #region Configuração Serviços ZEUS
        /// <summary>
        /// Monta a Configuração por ZEUS
        /// </summary>
        /// <returns></returns>
        public void MontaConfiguracoesZeus()
        {
            _configuracoes.CfgServico.Certificado = Funcoes.RetornaCertificado(TipoDoCertificado.ToString(), Nota.Empresa);
            _configuracoes.CfgServico.cUF = Funcoes.RetornaUF(Nota.Empresa.UF);
            _configuracoes.CfgServico.tpAmb = Funcoes.RetornaTipoAmbiente(((int)CwkAmbiente));
            _configuracoes.CfgServico.tpEmis = FormaEmissao;
            _configuracoes.CfgServico.ProtocoloDeSeguranca = SecurityProtocolType.Tls12; //Tls12 - Protocolo atual utilizado pela sefaz 4.00

            if (_configuracoes.CfgServico.tpAmb != TipoAmbiente.Producao)
            {
                _configuracoes.CfgServico.DiretorioSalvarXml = DiretorioPadrao + "\\LogHom";
                DiretorioXML = DiretorioPadrao + "\\XmlDestinatarioHom";
            }
            else
            {
                _configuracoes.CfgServico.DiretorioSalvarXml = DiretorioPadrao + "\\Log";
                DiretorioXML = DiretorioPadrao + "\\XmlDestinatario";
            }

            //Configuração CSC/Token
            _configuracoes.ConfiguracaoCsc.CIdToken = Nota.Empresa.CIdToken;
            _configuracoes.ConfiguracaoCsc.Csc = Nota.Empresa.Csc;

            //Configuração DanfeNFc-e
            _configuracoes.ConfiguracaoDanfeNfce.VersaoQrCode = Funcoes.RetornaVersaoQrCode(Nota.Empresa.QrCode);

        }
        #endregion

        #region Set Diretorio Padrão definido pelo Usuario na aplicação (Não está sendo Utilizado)
        private void SetDiretorioPadrao(string _diretorioPadrao)
        {
            if (String.IsNullOrEmpty(_diretorioPadrao) || !Directory.Exists(_diretorioPadrao))
                DiretorioPadrao = GetDiretorioSistema();
            else
                DiretorioPadrao = _diretorioPadrao;
        }
        #endregion

        #region Metodos abstract
        public abstract void Iniciar();
        public abstract IDictionary<string, string> GerarNFe();
        public abstract IDictionary<string, string> ConsultarNFe();
        public abstract IDictionary<string, string> ConsultarRecibo();
        public abstract string InutilizarNFe(string _ano, string _serie, string _numeroInicio, string _numeroFim, string _justificativa, string _cnpj = null);
        public abstract IDictionary<string, string> ResolveNfce();
        public abstract IDictionary<string, string> CancelarNFe(string _motivo, string _usuario);
        public abstract string GeraXmlNota();
        public abstract void GerarXmlPreDanfe();
        public abstract string AlterarFormaDeEmissao();
        #endregion

        public static INotaFiscalEletronica NotaFiscalEletronicaFactory(VersaoXML _versaoXml, TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado
                                                                        , string diretorioPadrao, Int16 indFinal, IndPres indPres, bool bDevolucao, int modeloDocumento = 55, int componenteDfe = 1)
        {             
            return NotaFiscalEletronicaFactory(_versaoXml, _tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao, modeloDocumento, componenteDfe, new ConfiguracaoManager());
        }

        public static string InutilizarNfce(ConfiguracaoManager _configManager, string _ano, string _serie, string _numeroInicio, string _numeroFim, string _justificativa)
        {
            string retorno = NotaFiscalEletronicaConsumidor50a.InutilizarNfce(_configManager, _ano, _serie, _numeroInicio, _numeroFim, _justificativa);
            return retorno;
        }

        #region Nota Fiscal Factory NFe/NFCe
        public static INotaFiscalEletronica NotaFiscalEletronicaFactory(VersaoXML _versaoXml, TipoEmissao _tipoServidor, cwkAmbiente _ambiente, TipoDoCertificado _tipoCertificado
                                                                        , string diretorioPadrao, Int16 indFinal, IndPres indPres, bool bDevolucao, int modeloDocumento, int componenteDfe, ConfiguracaoManager configManager = null)
        {
            INotaFiscalEletronica retorno;
            
            if (modeloDocumento == 55)
	        {            
                switch (_versaoXml)
                {
                    case VersaoXML.v3:
                        retorno = new NotaFiscalEletronica30(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao);
                        break;
                    case VersaoXML.v4:
                        retorno = new NotaFiscalEletronica40(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao);
                        break;
                    case VersaoXML.v5a:
                        retorno = new NotaFiscalEletronica50a(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao);
                        break;
                    case VersaoXML.v6:
                        if (componenteDfe == 0)
                        {
                            retorno = new NotaFiscalEletronica60(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao);
                        }
                        else
                        {
                            retorno = new NotaFiscalEletronicaZeus60(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao);
                        }

                        break;
                    default:
                        throw new Exception("Versão do XML " + _versaoXml + " não implementado para a NF-e");
                        //return null;
                }
	        }
            else
            {
                switch (_versaoXml)
                {
                    case VersaoXML.v5a:
                        retorno = new NotaFiscalEletronicaConsumidor50a(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao, configManager);
                        break;
                    case VersaoXML.v6:
                        if (componenteDfe == 0)
                        {
                            retorno = new NotaFiscalEletronicaConsumidor60(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao, configManager);
                        }
                        else
                        {
                            retorno = new NotaFiscalEletronicaConsumidorZeus60(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao);
                        }
                        break;
                    default:
                        throw new Exception("Versão do XML " + _versaoXml + "não implementado para a NFC-e" );
                }
            }
                
            retorno.FormaEmissao = _tipoServidor;
            retorno.TipoDoCertificado = _tipoCertificado;
            return retorno;
        }
        #endregion

        private static string GetDiretorioSistema()
        {
            string dir = Assembly.GetEntryAssembly().Location;
            return Path.Combine(Path.GetDirectoryName(dir), String.Empty);
        }

        #region Retornar Certificado Tecnospeed
        public static string[] RetornaListaCertificados(TipoDoCertificado tipoCertificado)
        {
            string listaCertificados;
            IspdNFeX _spdNFeX2 = new spdNFeX();
            _spdNFeX2.ConfigurarSoftwareHouse("09813496000197", "");


            switch (tipoCertificado)
            {
                case TipoDoCertificado.ckFile:
                    _spdNFeX2.TipoCertificado = TipoCertificado.ckFile;
                    break;
                case TipoDoCertificado.ckSmartCard:
                    _spdNFeX2.TipoCertificado = TipoCertificado.ckSmartCard;
                    break;
                case TipoDoCertificado.ckLocalMachine:
                    _spdNFeX2.TipoCertificado = TipoCertificado.ckLocalMashine;
                    break;
                case TipoDoCertificado.ckActiveDirectory:
                    _spdNFeX2.TipoCertificado = TipoCertificado.ckActiveDirectory;
                    break;
                case TipoDoCertificado.ckMemory:
                    _spdNFeX2.TipoCertificado = TipoCertificado.ckMemory;
                    break;
                default:
                    break;
            }


            listaCertificados = _spdNFeX2.ListarCertificados("|");
            if (String.IsNullOrEmpty(listaCertificados))
                return new String[1] { "NÃO FOI POSSÍVEL CAPTURAR NENHUM CERTIFICADO" };
            else
                return listaCertificados.Split('|');
        }

        public static string[] RetornaListaCertificados()
        {
            string listaCertificados;
            IspdNFeX _spdNFeX2 = new spdNFeX();
            _spdNFeX2.ConfigurarSoftwareHouse("09813496000197", "");
            listaCertificados = _spdNFeX2.ListarCertificados("|");
            if (String.IsNullOrEmpty(listaCertificados))
                return null;
            else
                return listaCertificados.Split('|');
        }
        #endregion

        #region Lista Certificado Instalado Zeus
        public static X509Certificate2 ListaCertificado()
        {
            return CertificadoDigital.ListareObterDoRepositorio();
        }
        #endregion

        #region VisualizarDANFE
        public void VisualizarDANFE()
        {
            if(Nota.Empresa.ComponenteDfe == 0)
            {
                string aXmlNota = Funcoes.AbrirArquivo(SpdNFeX.DiretorioXmlDestinatario + Nota.ChaveNota + "-nfe.xml");
                SpdNFeX.VisualizarDanfe("0000", aXmlNota, "Templates\\vm50a\\Danfe\\Retrato.rtm");
                ExportarDanfe(aXmlNota, Nota.ChaveNota);
            }
            else
            {
                NFeFacade facade = new NFeFacade();

                if (Nota.ModeloDocto == 65)
                {
                    string aXmlNota = Funcoes.AbrirArquivo(DiretorioXML + "\\" + Nota.ChaveNota + "-nfe.xml");
                    facade.VisualizarDanfeNFCe(aXmlNota, _configuracoes.ConfiguracaoDanfeNfce, _configuracoes.ConfiguracaoCsc.CIdToken, _configuracoes.ConfiguracaoCsc.Csc);
                }
                else
                {
                    string aXmlNota = Funcoes.AbrirArquivo(DiretorioXML + "\\" + Nota.ChaveNota + "-nfe.xml");
                    facade.VisualizarDanfe(aXmlNota);
                }
            }
            
        }
        #endregion

        //Falta Implementar ZEUS
        public void VisualizarEPEC()
        {
            string aXmlNota = Nota.XmlLogEnvNFe;
            string dataHoraEpec = PegaDataHoraEPEC();
            SpdNFeX.VisualizarEPEC(aXmlNota, Nota.NumeroProtocolo, dataHoraEpec, "Templates\\vm50a\\Danfe\\Retrato.rtm");
            ExportarEPEC();
        }

        #region ExportarDanfe
        public void ExportarDanfe(string pXmlNota, string pChaveNota)
        {
            if(Nota.Empresa.ComponenteDfe == 0)
            {
                SpdNFeX.ExportarDanfe("1", pXmlNota, "", 1, DiretorioPadrao + "\\pdf\\" + pChaveNota + ".pdf");
            }
            else
            {
                NFeFacade facade = new NFeFacade();
                facade.ExportarDanfe(pXmlNota, DiretorioPadrao + "\\pdf\\" + pChaveNota + ".pdf");
            }
            
        }
        #endregion

        //Falta Implementar ZEUS
        public void ExportarEPEC()
        {
            string aXmlNota = Nota.XmlLogEnvNFe;
            string dataHoraEpec = PegaDataHoraEPEC();
            SpdNFeX.ExportarEPEC(aXmlNota, Nota.NumeroProtocolo, dataHoraEpec, "", 1, DiretorioPadrao + "\\pdf\\" + Nota.ChaveNota + ".pdf");
        }        
        //Falta Implementar Zeus
        public void ImprimirDANFE(Dictionary<string, string> pDictChaveXmlNotas)
        {
            foreach (string chave in pDictChaveXmlNotas.Keys)
            {
                SpdNFeX.ImprimirDanfe("1", pDictChaveXmlNotas[chave], "", GetDefaultPrinter());
            }
        }
        //Falta Implementar Zeus
        public string ImprimirEpec()
        {
            string aXmlNota = Nota.XmlLogEnvNFe;
            string dataHoraEpec = PegaDataHoraEPEC();

            return SpdNFeX.ImprimirEPEC(aXmlNota, Nota.NumeroProtocolo, dataHoraEpec, "Templates\\vm50a\\Danfe\\Retrato.rtm", GetDefaultPrinter());
        }

        #region Pega Data Hora EPEC
        private string PegaDataHoraEPEC()
        {
            string dataHoraEpec = String.Empty;
            int inicio = Nota.UltimoXmlRecebido.IndexOf("<dhRegEvento>");
            int fim = Nota.UltimoXmlRecebido.IndexOf("</dhRegEvento>");
            return dataHoraEpec = Nota.UltimoXmlRecebido.Substring(inicio + 13, fim - inicio - 13);
        }
        #endregion

        #region Define a impressora padrão
        private string GetDefaultPrinter()
        {
            ObjectQuery query = new ObjectQuery(
                                    "Select * From Win32_Printer " +
                                    "Where Default = True");

            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(query);
            ManagementObjectCollection moc = searcher.Get();
            string printerPadrao = "";
            foreach (ManagementObject mo in moc)
            {
                Console.WriteLine(mo["Name"] + "\n");
                if (Convert.ToBoolean(mo.GetPropertyValue("Default")) == true)
                {
                    printerPadrao = mo["Name"].ToString();
                    break;
                }
            }
            return printerPadrao;
        }
        #endregion

        #region Editar Danfe
        public void EditarDanfe()
        {
            var arq_ret = new StringBuilder();
            var objReader = new StreamReader(SpdNFeX.DiretorioXmlDestinatario + Nota.ChaveNota + "-nfe.xml");
            var sLine = String.Empty;
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    arq_ret.Append(sLine);
            }
            objReader.Close();

            if(Nota.Empresa.ComponenteDfe == 0)
            {
                SpdNFeX.EditarModeloDanfe("0000001", arq_ret.ToString(), "");
            }
            else
            {
                NFeFacade facade = new NFeFacade();
                facade.EditarDanfe(arq_ret.ToString());
            }
            
        }
        #endregion

        #region Enviar Danfe Email
        public void EnviarDanfeEmail(string _emailDestinatario, string _assunto, string _mensagem)
        {
            if (Nota.Empresa.ComponenteDfe == 0)
            {
                string arquivo = "";

                SpdNFeX.EmailServidor = Empresa.ServidorSMTP;
                SpdNFeX.EmailPorta = Empresa.PortaSMTP;
                SpdNFeX.EmailRemetente = Empresa.EmailUsuario;
                SpdNFeX.EmailUsuario = Empresa.EmailUsuario;
                SpdNFeX.EmailSenha = Empresa.EmailSenha;
                SpdNFeX.EmailDestinatario = _emailDestinatario;
                SpdNFeX.EmailAssunto = _assunto;
                SpdNFeX.EmailMensagem = _mensagem;

                if (Empresa.GMailAutenticacao)
                    SpdNFeX.EmailAutenticacao = Empresa.GMailAutenticacao;

                try
                {
                    arquivo = SpdNFeX.EnviarNotaDestinatario(Nota.ChaveNota, DiretorioPadrao + "\\LOG\\" + Nota.LogEnvio, DiretorioPadrao + "\\LOG\\" + Nota.LogRecibo);
                }
                catch
                {
                    arquivo = SpdNFeX.EnviarNotaDestinatario(Nota.ChaveNota, DiretorioPadrao + "\\LogHom\\" + Nota.LogEnvio, DiretorioPadrao + "\\LogHom\\" + Nota.LogRecibo);
                }
            }
            else
            {
                NFe.Utils.Email.ConfiguracaoEmail ConfigEmail = new NFe.Utils.Email.ConfiguracaoEmail(
                Empresa.EmailUsuario,
                Empresa.EmailSenha,
                _assunto,
                _mensagem,
                Empresa.ServidorSMTP,
                Empresa.PortaSMTP,
                false,
                true);
                // Nome da Empresa
                ConfigEmail.Nome = Empresa.Fantasia;

                if (Empresa.GMailAutenticacao)
                    ConfigEmail.Ssl = true;

                var emailBuilder = new EmailBuilder(ConfigEmail)
                        .AdicionarDestinatario(_emailDestinatario)
                        .AdicionarAnexo(DiretorioXML + "\\" + Nota.ChaveNota + "-procNfe.xml")
                        .AdicionarAnexo(DiretorioPadrao + "\\pdf\\" + Nota.ChaveNota + "-nfe.pdf");

                //emailBuilder.ErroAoEnviarEmail += erro => MessageBox.Show(erro.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

                emailBuilder.Enviar();
            }
            
        }
        #endregion

        #region Envia Danfe, Exporta PDF e Imprimir Danfe, Usando Componente Tecnospeed Manager
        public void EnviarDanfeEmailManager(String emailCliente, String assuntoEmail, String corpoEmail, String cnpjCliente, ConfiguracaoManager configManager)
        {
            try
            {
                ManagerEDoc manager = new ManagerEDoc(configManager);
                manager.EnviarDanfeNFCe(Nota.ChaveNota, cnpjCliente, emailCliente, assuntoEmail, corpoEmail, configManager);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }       
        
        public String GetPDFManager(String caminhoArquivo, String nomeArquivo, Dictionary<string, string> parms, ConfiguracaoManager configManager)
        {
            String caminhoArquivoPDF = String.Empty;
            try
            {
                ManagerEDoc manager = new ManagerEDoc(configManager);
                using (HttpClient client = new HttpClient())
                {
                    manager.HttpClientGetFile(client, "imprime", parms, caminhoArquivo, nomeArquivo);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return caminhoArquivoPDF;
        }
        
        public Dictionary<string, string> ImprimirDanfeManager(ConfiguracaoManager configManager, List<string> ChavesAcesso)
        {
            ManagerEDoc manager = new ManagerEDoc(configManager);
            return manager.Imprimir(ChavesAcesso);
        }
        #endregion

        #region Enviar CCe (Carta de Correção Eletronica) ZEUS
        public string EnviarCCe(string _chaveNFe, string _textoCce, string _aIDLote, string _cnpj, int aSequenciaEvento)
        {
            string retorno = EnviarCCe(_chaveNFe, _textoCce, _aIDLote, _cnpj, aSequenciaEvento);

            return retorno;
        }
        #endregion

        #region Verifica Preechimento Desconto ZEUS
        /// <summary>
        /// Verifica Preechencimento Desconto
        /// </summary>
        /// <param name="dataEmissaoNFe"></param>
        /// <returns></returns>
        public decimal? VerificarPreechimentoDesconto(DateTime dataEmissaoNFe)
        {
            if (_configuracoes.CfgServico.tpAmb == TipoAmbiente.Homologacao || dataEmissaoNFe >= new DateTime(2018, 9, 3))
                return 0m;

            return null;
        }
        #endregion

        #region Formata String
        public string FormataTDEC_1204(object input)
        {
            return String.Format("{0:###########0.0000}", input).Replace(",", ".");
        }

        public string FormataTDEC_0302(object input)
        {
            return String.Format("{0:##0.00}", input).Replace(",", ".");
        }

        public string FormataTDEC_0302Opc(object input)
        {
            if (Convert.ToDecimal(input) == 0)
                return null;
            else
                return String.Format("{0:##0.00}", input).Replace(",", ".");
        }

        public string FormataTDEC_1302(object input)
        {
            return String.Format("{0:############0.00}", input).Replace(",", ".");
        }

        public string FormataTDEC_0804(object input)
        {
            return String.Format("{0:#######0.0000}", input).Replace(",", ".");
        }

        public string FormataTDEC_1203(object input)
        {
            return String.Format("{0:###########0.000}", input).Replace(",", ".");
        }

        public string FormataINTEIRO(object input)
        {
            return String.Format("{0:###############}", input);
        }
        #endregion

    }
}
