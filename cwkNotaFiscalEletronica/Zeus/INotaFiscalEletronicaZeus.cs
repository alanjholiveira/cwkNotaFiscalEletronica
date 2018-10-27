using System;
using System.Collections.Generic;
using System.Text;
using cwkNotaFiscalEletronica.Interfaces;
using System.IO;
using System.Reflection;
using System.Management;

using NFe.Utils.Email;
using DFe.Utils.Assinatura;
using System.Security.Cryptography.X509Certificates;
using DFe.Utils;
using DFe.Classes.Flags;
using TipoEmissaoZeus = NFe.Classes.Informacoes.Identificacao.Tipos.TipoEmissao;

namespace cwkNotaFiscalEletronica
{
    public abstract class INotaFiscalEletronicaZeus
    {
        public Danfe Danfe { get; set; }
        public DanfeNFCe DanfeNFCe { get; set; }
        public IEmpresa Empresa { get; set; }
        public INota Nota { get; set; }
        public string Observacoes { get; set; }
        public TipoCertificado TipoDoCertificado { get; set; }
        public string DiretorioPadrao { get; private set; }
        public string DiretorioXML { get; private set; }
        protected cwkAmbiente CwkAmbiente { get; set; }
        protected TipoAmbiente Ambiente { get; set; }
        protected TipoEmissaoZeus FormaEmissao { get; set; }
        //Configuração ZEUS
        public static ConfiguracaoApp _configuracoes;
        public string MesAno = DateTime.Now.ToString("yyyy-MM");


        public INotaFiscalEletronicaZeus(TipoEmissaoZeus _tipoServidor, cwkAmbiente _ambiente, TipoCertificado _tipoCertificado, string _diretorioPadrao)
        {
            CwkAmbiente = _ambiente;            
            SetDiretorioPadrao(_diretorioPadrao);         
            _configuracoes = new ConfiguracaoApp();                 
        }

        #region Configuração Serviços ZEUS
        /// <summary>
        /// Monta a Configuração por ZEUS
        /// </summary>
        /// <returns></returns>
        public void MontaConfiguracoesZeus()
        {
            //Configuração Serviço
            _configuracoes.CfgServico.Certificado = Funcoes.RetornaCertificado(TipoDoCertificado.ToString(), Nota.Empresa);
            _configuracoes.CfgServico.cUF = Funcoes.RetornaUF(Nota.Empresa.UF);
            _configuracoes.CfgServico.tpAmb = Funcoes.RetornaTipoAmbiente(((int)CwkAmbiente));
            _configuracoes.CfgServico.tpEmis = FormaEmissao;
            _configuracoes.CfgServico.ProtocoloDeSeguranca = System.Net.SecurityProtocolType.Tls12; //Tls12

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
            _configuracoes.ConfiguracaoCsc.CIdToken = Nota.Empresa.CIdToken.ToString();
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
        //VErificar ser a necessidade desse metodo
        public abstract IDictionary<string, string> ResolveNfce();
        public abstract IDictionary<string, string> CancelarNFe(string _motivo, string _usuario);
        public abstract string GeraXmlNota();
        public abstract void GerarXmlPreDanfe();
        public abstract string AlterarFormaDeEmissao();
        #endregion
                
        #region Nota Fiscal Factory NFe/NFCe
        public static INotaFiscalEletronicaZeus NotaFiscalEletronicaFactory(VersaoXML _versaoXml, TipoEmissaoZeus _tipoServidor, cwkAmbiente _ambiente, TipoCertificado _tipoCertificado
                                                                        , string diretorioPadrao, Int16 indFinal, IndPres indPres, bool bDevolucao, int modeloDocumento)
        {
            INotaFiscalEletronicaZeus retorno;

            if (modeloDocumento == 55)
	        {            
                switch (_versaoXml)
                {
                    case VersaoXML.v6:
                        retorno = new NotaFiscalEletronicaZeus60(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao);
                        break;
                    default:
                        return null;
                }
	        }
            else
            {
                switch (_versaoXml)
                {
                    case VersaoXML.v6:
                        retorno = new NotaFiscalEletronicaConsumidorZeus60(_tipoServidor, _ambiente, _tipoCertificado, diretorioPadrao, indFinal, indPres, bDevolucao);
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

        #region Get/Cria Diretorio caso não exista
        private static string GetDiretorioSistema()
        {
            string dir = Assembly.GetEntryAssembly().Location;
            return Path.Combine(Path.GetDirectoryName(dir), String.Empty);
        }
        #endregion

        #region Lista Certificado Instalado 
        public static X509Certificate2 ListaCertificado()
        {
            return CertificadoDigital.ListareObterDoRepositorio();            
        }
        #endregion
        
        #region VisualizarDANFE
        public void VisualizarDANFE()
        {           
            NFeFacade facade = new NFeFacade();

            if (Nota.ModeloDocto == 65)
            {
                string aXmlNota = Funcoes.AbrirArquivo(DiretorioXML + "\\" + Nota.ChaveNota + "-procNfe.xml");
                facade.VisualizarDanfeNFCe(aXmlNota, _configuracoes.ConfiguracaoDanfeNfce, _configuracoes.ConfiguracaoCsc.CIdToken, _configuracoes.ConfiguracaoCsc.Csc);
            }
            else
            {
                string aXmlNota = Funcoes.AbrirArquivo(DiretorioXML + "\\" + Nota.ChaveNota + "-procNfe.xml");
                facade.VisualizarDanfe(aXmlNota);
            }

            //ExportarDanfe(aXmlNota, Nota.ChaveNota);
        }
        #endregion

        #region Visualizar EPEC
        public void VisualizarEPEC()
        {
            string aXmlNota = Nota.XmlLogEnvNFe;
            string dataHoraEpec = PegaDataHoraEPEC();
            //SpdNFeX.VisualizarEPEC(aXmlNota, Nota.NumeroProtocolo, dataHoraEpec, "Templates\\vm50a\\Danfe\\Retrato.rtm");
            ExportarEPEC();
        }
        #endregion

        #region ExportarDanfe (USADO NFeControlle verificar)
        public void ExportarDanfe(string pXmlNota, string pChaveNota)
        {  
            NFeFacade facade = new NFeFacade();
            facade.ExportarDanfe(pXmlNota, DiretorioPadrao + "\\pdf\\" + pChaveNota + ".pdf");
        }
        #endregion

        #region Exportar EPEC
        public void ExportarEPEC()
        {
            string aXmlNota = Nota.XmlLogEnvNFe;
            string dataHoraEpec = PegaDataHoraEPEC();
            //SpdNFeX.ExportarEPEC(aXmlNota, Nota.NumeroProtocolo, dataHoraEpec, "", 1, DiretorioPadrao + "\\pdf\\" + Nota.ChaveNota + ".pdf");
        }
        #endregion

        #region Imprimir DANFE (Impementar NFe/NFCe)
        public void ImprimirDANFE(Dictionary<string, string> pDictChaveXmlNotas)
        {
            foreach (string chave in pDictChaveXmlNotas.Keys)
            {
                //SpdNFeX.ImprimirDanfe("1", pDictChaveXmlNotas[chave], "", GetDefaultPrinter());
            }
        }
        #endregion

        #region Imprimir Epec
        public string ImprimirEpec()
        {
            string aXmlNota = Nota.XmlLogEnvNFe;
            string dataHoraEpec = PegaDataHoraEPEC();

            //return SpdNFeX.ImprimirEPEC(aXmlNota, Nota.NumeroProtocolo, dataHoraEpec, "Templates\\vm50a\\Danfe\\Retrato.rtm", GetDefaultPrinter());
            return null;
        }
        #endregion

        #region Pega Data Hora EPEC
        private string PegaDataHoraEPEC()
        {
            string dataHoraEpec = String.Empty;
            int inicio = Nota.UltimoXmlRecebido.IndexOf("<dhRegEvento>");
            int fim = Nota.UltimoXmlRecebido.IndexOf("</dhRegEvento>");
            return dataHoraEpec = Nota.UltimoXmlRecebido.Substring(inicio + 13, fim - inicio - 13);
        }
        #endregion

        #region Define a impressora padrão (VERIFICAR SE A NECESSIDADE)
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
            var objReader = new StreamReader(DiretorioXML + "\\" + Nota.ChaveNota + "-procNfe.xml");
            var sLine = String.Empty;
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    arq_ret.Append(sLine);
            }
            objReader.Close(); 
            
            NFeFacade facade = new NFeFacade();
            facade.EditarDanfe(arq_ret.ToString());
        }
        #endregion

        #region Enviar Danfe Email
        public void EnviarDanfeEmail(string _emailDestinatario, string _assunto, string _mensagem)
        {
            ConfiguracaoEmail ConfigEmail = new ConfiguracaoEmail(
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
        #endregion

        #region Enviar CCe (Carta de Correção Eletronica)
        public string EnviarCCe(string _chaveNFe, string _textoCce, string _aIDLote, string _cnpj, int aSequenciaEvento)
        {
            string retorno = EnviarCCe(_chaveNFe, _textoCce, _aIDLote, _cnpj, aSequenciaEvento);

            return retorno;
        }
        #endregion

        #region Verifica Preechimento Desconto
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

        
        //Verificar Remoção
        public string FormataINTEIRO(object input)
        {
            return String.Format("{0:###############}", input);
        }

    }
}
