using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DFe.Classes.Flags;
using NFe.Classes.Servicos.Tipos;
using NFe.Servicos;
using NFe.Servicos.Retorno;
using NFe.Utils;
using NFe.Utils.NFe;
using Classes = NFe.Classes;
using NFe.Classes;
using NFe.Danfe.Fast.NFe;
using NFe.Danfe.Base.NFe;
using NFe.Classes.Protocolo;
using NFe.Utils.Excecoes;
using NFe.Utils.Consulta;
using NFe.Danfe.Nativo.NFCe;
using NFe.Danfe.Base.NFCe;
using NFeZeus = NFe.Classes.NFe;
using NFe.Danfe.Fast.NFCe;
using System.Windows.Forms;

namespace cwkNotaFiscalEletronica
{
    public class NFeFacade
    {
        readonly string SoftHouse = "Cwork Sistemas";        

        #region Consultar Status Serviço
        /// <summary>
        /// Responsavel Consultar Status do Serviço da Sefaz
        /// </summary>
        /// <returns></returns>
        public RetornoNfeStatusServico ConsultarStatusServico(ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            var retornoStatus = servicoNFe.NfeStatusServico();

            return retornoStatus;
        }
        #endregion

        #region Enviar NFe
        /// <summary>
        /// Responsavel por Enviar NFe para Sefaz
        /// </summary>
        /// <param name="cFgServico"></param>
        /// <param name="numLote"></param>
        /// <param name="nfe"></param>
        /// <returns></returns>
        public RetornoNFeAutorizacao EnviarNFe(int numLote, Classes.NFe nfe, ConfiguracaoServico cFgServico)
        {
            //nfe.Assina(cFgServico); //não precisa validar aqui, pois o lote será validado em ServicosNFe.NFeAutorizacao
            var servicoNFe = new ServicosNFe(cFgServico);
            var retornoEnvio = servicoNFe.NFeAutorizacao(numLote, IndicadorSincronizacao.Assincrono, new List<Classes.NFe> { nfe }, true/*Envia a mensagem compactada para a SEFAZ*/);

            var recibo = servicoNFe.NFeRetAutorizacao(retornoEnvio.Retorno.infRec.nRec);
            var cStat = this.VerificarcStat(recibo.RetornoCompletoStr, "protNFe", "infProt");
            if (cStat == "100")
            {
                //Salva NFe+protNFe na tag nfeProc apos autorização da NFe
                this.AdicionarNFeProc(nfe.infNFe.Id.Substring(3), nfe, cFgServico);              
            }
            if(nfe.infNFe.ide.mod == ModeloDocumento.NFCe)
            { 
                this.SalvarXmlArquivo(nfe.infNFe.Id.Substring(3) + "-nfce", nfe.ObterXmlString(), cFgServico.tpAmb);
            }
            else
            {
                this.SalvarXmlArquivo(nfe.infNFe.Id.Substring(3) + "-nfe", nfe.ObterXmlString(), cFgServico.tpAmb);
            }
            return retornoEnvio;

        }
        #endregion

        #region Consultar Recibo de Envio
        /// <summary>
        /// Responsavel Consultar Recibo de Envio do Lote/NFe
        /// </summary>
        /// <param name="recibo"></param>
        /// <returns></returns>
        public RetornoNFeRetAutorizacao ConsultarReciboDeEnvio(string recibo, ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            return servicoNFe.NFeRetAutorizacao(recibo);
        }
        #endregion

        #region CancelarNFe
        /// <summary>
        /// Responsavel de Cancelar NFe Emitida
        /// </summary>
        /// <param name="cFgServico"></param>
        /// <param name="cnpjEmitente"></param>
        /// <param name="numeroLote"></param>
        /// <param name="sequenciaEvento"></param>
        /// <param name="chaveAcesso"></param>
        /// <param name="protocolo"></param>
        /// <param name="justificativa"></param>
        /// <returns></returns>
        public RetornoRecepcaoEvento CancelarNFe(string cnpjEmitente, int numeroLote, short sequenciaEvento, string chaveAcesso,
            string protocolo, string justificativa, ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            var retornoEnvio = servicoNFe.RecepcaoEventoCancelamento(numeroLote, sequenciaEvento, protocolo, chaveAcesso, justificativa, cnpjEmitente);
            
            foreach (var evento in retornoEnvio.ProcEventosNFe)
            {
                var proceventoXmlString = evento.ObterXmlString();

                if (evento.retEvento.infEvento.cStat == 135 | evento.retEvento.infEvento.cStat == 155)
                {
                    //Salva procEventoNFe
                    this.SalvarXmlArquivo(evento.retEvento.infEvento.chNFe + "-canc", proceventoXmlString, cFgServico.tpAmb);
                }
            }
            
            return retornoEnvio;
        }
        #endregion

        #region Inutilizar Numercao
        /// <summary>
        /// Responsavel Inutilizar NFe
        /// </summary>
        /// <param name="cFgServico"></param>
        /// <param name="ano"></param>
        /// <param name="cnpj"></param>
        /// <param name="justificativa"></param>
        /// <param name="numeroInicial"></param>
        /// <param name="numeroFinal"></param>
        /// <param name="serie"></param>
        /// <returns></returns>
        public RetornoNfeInutilizacao InutilizarNumeracao(int ano, string cnpj, string justificativa,
            int numeroInicial, int numeroFinal, int serie, ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            return servicoNFe.NfeInutilizacao(cnpj, Convert.ToInt32(ano.ToString().Substring(2, 2)), cFgServico.ModeloDocumento, serie, numeroInicial, numeroFinal, justificativa);
        }
        #endregion

        #region ConsultarNFe
        /// <summary>
        /// Responsavel Consultar NFe Já Emitida
        /// </summary>
        /// <param name="cFgServico"></param>
        /// <param name="chaveNota"></param>
        /// <returns></returns>
        public RetornoNfeConsultaProtocolo ConsultarNfe(string chaveNota, ConfiguracaoServico cFgServico)
        {           
            var servicoNFe = new ServicosNFe(cFgServico);
            return servicoNFe.NfeConsultaProtocolo(chaveNota);            
        }
        #endregion

        #region Carta de Correção
        /// <summary>
        /// Responsavel por enviar Carta de Correção para Sefaz
        /// </summary>
        /// <param name="idLote"></param>
        /// <param name="sequenciaEvento"></param>
        /// <param name="chave"></param>
        /// <param name="correcao"></param>
        /// <param name="cpfcnpj"></param>
        /// <param name="cFgServico"></param>
        /// <returns></returns>
        public RetornoRecepcaoEvento CartaCorrecao(int idLote, int sequenciaEvento, string chave, string correcao, string cpfcnpj, ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            var retornoEnvio = servicoNFe.RecepcaoEventoCartaCorrecao(idLote, sequenciaEvento, chave, correcao, cpfcnpj);

            foreach (var evento in retornoEnvio.ProcEventosNFe)
            {
                var proceventoXmlString = evento.ObterXmlString();

                if (evento.retEvento.infEvento.cStat == 135)
                {
                    //Salva procEventoNFe
                    this.SalvarXmlArquivo(evento.retEvento.infEvento.chNFe + "-procEventoNFe", proceventoXmlString, cFgServico.tpAmb);
                }
            }

            return retornoEnvio;
        }
        #endregion

        #region Enviar EPEC
        /// <summary>
        /// Envia EPEC para Sefaz
        /// </summary>
        /// <param name="idlote"></param>
        /// <param name="sequenciaEvento"></param>
        /// <param name="_nfe"></param>
        /// <param name="veraplic"></param>
        /// <param name="cFgServico"></param>
        /// <returns></returns>
        public RetornoRecepcaoEvento EnviarEPEC(int idlote, int sequenciaEvento, Classes.NFe _nfe, string veraplic, ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            var retornoEnvio = servicoNFe.RecepcaoEventoEpec(Convert.ToInt32(idlote), Convert.ToInt16(sequenciaEvento), _nfe, veraplic);

            foreach (var evento in retornoEnvio.ProcEventosNFe)
            {
                var proceventoXmlString = evento.ObterXmlString();

                if (evento.retEvento.infEvento.cStat == 135)
                {
                    //Salva procEventoNFe
                    this.SalvarXmlArquivo(evento.retEvento.infEvento.chNFe + "-epec", proceventoXmlString, cFgServico.tpAmb);
                }
            }

            return retornoEnvio;
        }
        #endregion

        #region ConsultarEpec
        public RetornoNfeConsultaProtocolo ConsultarEPEC(string chave, ConfiguracaoServico cFgServico)
        {
            var servicoNFe = new ServicosNFe(cFgServico);
            return servicoNFe.NfeConsultaProtocolo(chave);
        }
        #endregion
        
        #region Visualizar Danfe NFE
        /// <summary>
        /// Responsavel Exibir Danfe
        /// </summary>
        /// <param name="xmlNfe"></param>
        /// <returns></returns>
        public void VisualizarDanfe(string xmlNfe)
        {
            try
            {
                #region Carrega um XML com nfeProc para a variável

                var arquivoXml = xmlNfe;

                
                if (string.IsNullOrEmpty(arquivoXml))
                    return;

                nfeProc proc = null;

                try
                {
                    //proc = new nfeProc().CarregarDeArquivoXml(arquivoXml);
                    proc = new nfeProc().CarregarDeXmlString(arquivoXml);
                }
                catch //Carregar NFe ainda não transmitida à sefaz, como uma pré-visualização.
                {
                    //proc = new nfeProc() { NFe = new NFe().CarregarDeArquivoXml(arquivoXml), protNFe = new protNFe() };
                    proc = new nfeProc() { NFe = new Classes.NFe().CarregarDeXmlString(arquivoXml), protNFe = new protNFe() };
                }

                if (proc.NFe.infNFe.ide.mod != ModeloDocumento.NFe)
                    throw new Exception("O XML informado não é um NFe!");

                /*
                //Carregar atravez de um stream....                   
                var stream = new StreamReader(arquivoXml, Encoding.GetEncoding("ISO-8859-1"));
                var proc = new nfeProc().CarregardeStream(stream);               
                */
                #endregion

                #region Abre a visualização do relatório para impressão
                var danfe = new DanfeFrNfe(proc: proc,
                                    configuracaoDanfeNfe: new ConfiguracaoDanfeNfe(),
                                    desenvolvedor: SoftHouse,
                                    arquivoRelatorio: string.Empty);

                danfe.Visualizar();
                //danfe.Imprimir();
                //danfe.ExibirDesign();
                //danfe.ExportarPdf(@"d:\teste.pdf");

                #endregion

                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Editar Danfe NFE
        /// <summary>
        /// Responsavel Editar Danfe
        /// </summary>
        /// <param name="xmlNfe"></param>
        /// <returns></returns>
        public void EditarDanfe(string xmlNfe)
        {
            try
            {
                #region Carrega um XML com nfeProc para a variável

                var arquivoXml = xmlNfe;


                if (string.IsNullOrEmpty(arquivoXml))
                    return;

                nfeProc proc = null;

                try
                {
                    //proc = new nfeProc().CarregarDeArquivoXml(arquivoXml);
                    proc = new nfeProc().CarregarDeXmlString(arquivoXml);
                }
                catch //Carregar NFe ainda não transmitida à sefaz, como uma pré-visualização.
                {
                    //proc = new nfeProc() { NFe = new NFe().CarregarDeArquivoXml(arquivoXml), protNFe = new protNFe() };
                    proc = new nfeProc() { NFe = new Classes.NFe().CarregarDeXmlString(arquivoXml), protNFe = new protNFe() };
                }

                if (proc.NFe.infNFe.ide.mod != ModeloDocumento.NFe)
                    throw new Exception("O XML informado não é um NFe!");

                #endregion

                #region Abre a visualização do relatório para impressão
                var danfe = new DanfeFrNfe(proc: proc,
                                    configuracaoDanfeNfe: new ConfiguracaoDanfeNfe(),
                                    desenvolvedor: SoftHouse,
                                    arquivoRelatorio: string.Empty);

                danfe.ExibirDesign();
               
                #endregion


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Exportar Danfe NFE para PDF
        /// <summary>
        /// Responsavel Exportar Danfe
        /// </summary>
        /// <param name="xmlNfe"></param>
        /// <param name="nomeArquivo"></param>
        /// <returns></returns>
        public void ExportarDanfe(string xmlNfe, string nomeArquivo)
        {
            try
            {
                #region Carrega um XML com nfeProc para a variável

                var arquivoXml = xmlNfe;


                if (string.IsNullOrEmpty(arquivoXml))
                    return;

                nfeProc proc = null;

                try
                {
                    //proc = new nfeProc().CarregarDeArquivoXml(arquivoXml);
                    proc = new nfeProc().CarregarDeXmlString(arquivoXml);
                }
                catch //Carregar NFe ainda não transmitida à sefaz, como uma pré-visualização.
                {
                    //proc = new nfeProc() { NFe = new NFe().CarregarDeArquivoXml(arquivoXml), protNFe = new protNFe() };
                    proc = new nfeProc() { NFe = new Classes.NFe().CarregarDeXmlString(arquivoXml), protNFe = new protNFe() };
                }

                if (proc.NFe.infNFe.ide.mod != ModeloDocumento.NFe)
                    throw new Exception("O XML informado não é um NFe!");

                #endregion

                #region Abre a visualização do relatório para impressão
                var danfe = new DanfeFrNfe(proc: proc,
                                    configuracaoDanfeNfe: new ConfiguracaoDanfeNfe(),
                                    desenvolvedor: SoftHouse,
                                    arquivoRelatorio: string.Empty);


                danfe.ExportarPdf(nomeArquivo);

                #endregion


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Visualizar Danfe NFC-e
        public void VisualizarDanfeNFCe(string xmlNFce, ConfiguracaoDanfeNfce configuracaoDanfe, string cIdToken, string csc)
        {
            VisualizarNFCe(xmlNFce, configuracaoDanfe, cIdToken, csc);
            //MessageBox.Show(xmlNFce);
            //this.ImprimirNFCe(xmlNFce, configuracaoDanfe, cIdToken, csc);
            
        }
        #endregion

        #region Salvar Arquivo XML
        /// <summary>
        /// Salva o XML conforme ambiente "Homologação/Produção"
        /// </summary>
        /// <param name="arquivoNome"></param>
        /// <param name="conteudoXML"></param>
        /// <param name="pastaNFe"></param>
        /// <param name="tpAmb"></param>
        public void SalvarXmlArquivo(string arquivoNome, string conteudoXML, TipoAmbiente tpAmb)
        {
            //string caminho = System.Environment.CurrentDirectory;
            StreamWriter streamWriter;
            DirectoryInfo di;


            if (tpAmb == TipoAmbiente.Homologacao)
            {
                di = Directory.CreateDirectory(Path.Combine("XmlDestinatarioHom\\"));
            }
            else
            {
                di = Directory.CreateDirectory(Path.Combine("XmlDestinatario\\"));
            }

            streamWriter = File.CreateText(di.FullName + arquivoNome + ".xml");

            streamWriter.Write(conteudoXML.ToString());
            streamWriter.Close();
        }
        #endregion

        #region Salvar Arquivo em PDF
        public void SalvarPDFArquivo(string arquivoNome, string conteudoXML, TipoAmbiente tpAmb)
        {
            DirectoryInfo di;

            if (tpAmb == TipoAmbiente.Homologacao)
            {
                di = Directory.CreateDirectory(Path.Combine("pdfHom\\"));
            }
            else
            {
                di = Directory.CreateDirectory(Path.Combine("pdf\\"));
            }

            
            #region Carrega um XML com nfeProc para a variável
            nfeProc proc = null;

            try
            {
                proc = new nfeProc().CarregarDeXmlString(conteudoXML);
            }
            catch //Carregar NFe ainda não transmitida à sefaz, como uma pré-visualização.
            {                    
                proc = new nfeProc() { NFe = new Classes.NFe().CarregarDeXmlString(conteudoXML), protNFe = new protNFe() };
            }
            #endregion

            #region Abre a visualização do relatório para impressão
            var danfe = new DanfeFrNfe(proc: proc,
                                configuracaoDanfeNfe: new ConfiguracaoDanfeNfe(),
                                desenvolvedor: SoftHouse,
                                arquivoRelatorio: string.Empty);


            danfe.ExportarPdf(di.FullName + arquivoNome + ".pdf");
            #endregion

        }
        #endregion

        #region PRIVATE: Adicionar NFE a tag nfeProc
        /// <summary>
        /// Responsavel por Adicionar NFe a tag nfeProc e Salvar o XML
        /// </summary>
        /// <param name="ChaveNota"></param>
        /// <param name="nfe"></param>
        /// <param name="cFgServico"></param>
        /// <returns></returns>
        private void AdicionarNFeProc(string ChaveNota, Classes.NFe nfe, ConfiguracaoServico cFgServico)
        {
            try
            {
                var protocolo = this.ConsultarNfe(ChaveNota, cFgServico);

                var nfeproc = new nfeProc
                {
                    NFe = nfe,
                    protNFe = protocolo.Retorno.protNFe,
                    versao = protocolo.Retorno.versao
                };

                if (nfeproc.protNFe != null)
                {
                    if(nfe.infNFe.ide.mod == ModeloDocumento.NFCe)
                    {
                        //Função para salvar XML
                        SalvarXmlArquivo(nfeproc.protNFe.infProt.chNFe + "-nfce", nfeproc.ObterXmlString(), cFgServico.tpAmb);
                    }
                    else
                    { 
                        //Função para salvar XML
                        SalvarXmlArquivo(nfeproc.protNFe.infProt.chNFe + "-nfe", nfeproc.ObterXmlString(), cFgServico.tpAmb);
                        //Função para salvar PDF
                        SalvarPDFArquivo(nfeproc.protNFe.infProt.chNFe + "-nfe", nfeproc.ObterXmlString(), cFgServico.tpAmb);
                    }
                }
                
            }
            catch (ComunicacaoException ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ValidacaoSchemaException ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        #endregion

        #region PRIVATE: Verificar cStat e retorna valor
        /// <summary>
        /// Responsavel por Verificar Status cStat e retorna o valor
        /// </summary>
        /// <param name="aXmlRetorno"></param>
        /// <param name="NFeLocalName"></param>
        /// <param name="INFLocalName"></param>
        /// <returns></returns>
        private string VerificarcStat(string aXmlRetorno, string NFeLocalName, string INFLocalName)
        {
            XDocument documentoXml = XDocument.Load(new StreamReader(new MemoryStream(ASCIIEncoding.UTF8.GetBytes(aXmlRetorno))));

            //var noh = (from c in documentoXml.Root.Elements() where c.Name.LocalName == "protNFe" select c).Single<XElement>();
            //noh = (from c in noh.Elements() where c.Name.LocalName == "infProt" select c).Single<XElement>();
            //string cStat = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();
            //string dStat = (from c in noh.Elements() where c.Name.LocalName == "xMotivo" select c.Value).Single();
            //Console.WriteLine("retorno do recibo: " + cStat);

            XElement nohInf = (from c in documentoXml.Root.Elements() where c.Name.LocalName == NFeLocalName select c).Single();
            XElement noh = (from c in nohInf.Elements() where c.Name.LocalName == INFLocalName select c).Single();
            string valorCStat = (from c in noh.Elements() where c.Name.LocalName == "cStat" select c.Value).Single();

            return valorCStat;

        }
        #endregion

        #region PRIVATE: Imprimir NFc-e (impressora)
        /// <summary>
        /// Responsavel por imprimir NFCe apos autorização direto na impressora padrão ou definida na configuração
        /// </summary>
        private void ImprimirNFCe(string xmlNFce, ConfiguracaoDanfeNfce configuracaoDanfe, string cIdToken, string csc, string nomedaimpressora = null)
        {
            try
            {
                nfeProc proc = null;
                NFeZeus nfe = null;
                string arquivo = string.Empty;

                try
                {
                    proc = new nfeProc().CarregarDeXmlString(xmlNFce);
                    arquivo = proc.ObterXmlString();
                }
                catch (Exception)
                {
                    nfe = new NFe.Classes.NFe().CarregarDeArquivoXml(xmlNFce);
                    arquivo = nfe.ObterXmlString();
                }

                DanfeNativoNfce impr = new DanfeNativoNfce(arquivo,
                    configuracaoDanfe, cIdToken, csc,
                    0 /*troco*//*, "Arial Black"*/);

                impr.Imprimir();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Visualizar DANFE NFC-e
        /// <summary>
        /// Responsavel por visualizar NFCe apos autorização. E possivel imprimir apos visualização
        /// </summary>
        private void VisualizarNFCe(string xmlNFce, ConfiguracaoDanfeNfce configuracaoDanfe, string cIdToken, string csc)
        {
            try
            {
                #region Carrega um XML com nfeProc para a variável
                                
                if (string.IsNullOrEmpty(xmlNFce))
                    return;
                var proc = new nfeProc().CarregarDeXmlString(xmlNFce);
                if (proc.NFe.infNFe.ide.mod != ModeloDocumento.NFCe)
                    throw new Exception("O XML informado não é um NFCe!");

                #endregion

                #region Abre a visualização do relatório para impressão

                var danfe = new DanfeFrNfce(proc: proc, configuracaoDanfeNfce: configuracaoDanfe,
                    cIdToken: cIdToken,
                    csc: csc,
                    arquivoRelatorio: string.Empty);

                danfe.Visualizar(false);
                //danfe.Imprimir();
                //danfe.ExibirDesign();
                //danfe.ExportarPdf(@"d:\teste.pdf");
              
                #endregion

            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                    MessageBox.Show(ex.Message);
            }
        }
        #endregion


    }
}
