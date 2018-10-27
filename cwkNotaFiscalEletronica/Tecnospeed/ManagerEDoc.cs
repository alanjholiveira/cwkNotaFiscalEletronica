using cwkNotaFiscalEletronica.Modelo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace cwkNotaFiscalEletronica
{
    public class ManagerEDoc
    {
        private ConfiguracaoManager ConfigManager { get; set; }
        /// <summary>
        /// Configuração do Manager para enviar as requisições.
        /// </summary>
        /// <param name="configManager">Parâmetros do Manager</param>
        public ManagerEDoc (ConfiguracaoManager configManager)
        {
            ConfigManager = configManager;
        }

        /// <summary>
        /// Trata e envia o XML da NFC-e
        /// </summary>
        /// <param name="strXML">string do XML da NFC-e</param>
        /// <returns>Retorna o resultado o envio</returns>
        public string EnviarNFCe(string strXML)
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();
            string arquivo = "Formato=XML" + Environment.NewLine + strXML;
            arquivo = HttpUtility.UrlEncode(arquivo);
            parms["arquivo"] = arquivo;
            return HttpClientPostAsync("envia", parms);
        }

        /// <summary>
        /// Realiza a consulta dos dados da nfc-e
        /// </summary>
        /// <param name="parametros">Lista com os parametros a ser consultado e filtrado </param>
        /// <returns>Retorna string com os dados da consulta</returns>
        public string Consultar(Dictionary<string, string> parametros)
        {
            using (HttpClient client = new HttpClient())
            {
                string retorno = HttpClientGetAsync(client, "consulta", parametros);
                return retorno;
            }
        }

        public Dictionary<string, string> Imprimir(List<string> ChavesAcesso)
        {
            Dictionary<string, string> retorno = new Dictionary<string, string>();
            foreach (string chaveAcesso in ChavesAcesso)
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> parametros = new Dictionary<string, string>();
                    parametros["ChaveNota"] = chaveAcesso;
                    string ret = HttpClientGetAsync(client, "imprime", parametros);
                    string[] retSplit = ret.Split(',');
                    if (retSplit[0].Contains("EXCEPTION"))
                    {
                        retorno[chaveAcesso] = "Erro: " + retSplit[1] + " Detalhe: " + retSplit[2];
                    }
                    else
                    {
                        retorno[chaveAcesso] = "";
                    }
                }
            }
            return retorno;
        }

        
        public string CancelarNfce(String chaveNota, String justificativa)
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();
            parms["ChaveNota"] = chaveNota;
            parms["Justificativa"] = justificativa;

            return HttpClientPostAsync("cancela", parms);
        }

        public string ResolverNfce(String chaveNota)
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();
            parms["ChaveNota"] = chaveNota;

            return HttpClientPostAsync("resolve", parms);
        }

        public string InutilizarNfce(string ano, string serie, string numeroInicio, string numeroFim, string justificativa)
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();
            parms["ano"] = ano;
            parms["serie"] = serie;
            parms["NFIni"] = numeroInicio;
            parms["NFFin"] = numeroFim;
            parms["Justificativa"] = justificativa;

            return HttpClientPostAsync("inutiliza", parms);
        }

        private string HttpClientGetAsync(HttpClient client, string metodo, Dictionary<string, string> parms)
        {
            string uri;
            string content;
            IniciaHttpClientManagerEdoc(client, metodo, parms, out uri, out content);

            using (HttpResponseMessage response = client.GetAsync(uri + content).Result)
            {
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public string EnviarDanfeNFCe(String chaveNota, String cnpjCliente, String emailCliente, String assuntoEmail, String corpoEmail, ConfiguracaoManager configManager)
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();

            parms["grupo"] = configManager.grupo;
            parms["cnpj"] = cnpjCliente;
            parms["ChaveNota"] = chaveNota;
            parms["EmailDestinatario"] = emailCliente;
            parms["Assunto"] = assuntoEmail;
            parms["Texto"] = corpoEmail;

            return HttpClientPostAsync("email", parms);
        }

        public string AlterarFormaDeEmissao(String formaEmissao)
        {
            if (formaEmissao != "Normal")
                formaEmissao = "Contingencia Offline";

            Dictionary<string, string> parms = new Dictionary<string, string>();

            parms["modo"] = formaEmissao;

            using (HttpClient client = new HttpClient())
            {
                string retorno = HttpClientGetAsync(client, "modo", parms);
                return retorno;
            }

        }

        /// <summary>
        ///     Baixa o arquivo do manager e-doc e retorno o caminha onde o mesmo foi adicionado
        /// </summary>
        /// <param name="client">HttpClient a ser configurado</param>
        /// <param name="metodo">Método que será utilizado so Manager e-Doc (Consulta, Envia ...)</param>
        /// <param name="parms">Lista onde será adicionado os parâmetros basicos para comunicação</param>
        /// <param name="caminhoArquivo">Caminho onde o arquivo será adicionado</param>
        /// <param name="nomeArquivo">nome do arquivo a ser salvo</param>
        /// <returns>Retorno o endereço do arquivo</returns>
        public String HttpClientGetFile(HttpClient client, string metodo, Dictionary<string, string> parms, String caminhoArquivo, String nomeArquivo)
        {
            string caminhoRetorno = String.Empty;
            String uri;
            String content;

            try
            {
                IniciaHttpClientManagerEdoc(client, metodo, parms, out uri, out content);
                caminhoRetorno = caminhoArquivo + @"\" + nomeArquivo;
                using (HttpResponseMessage response = client.GetAsync(uri + content).Result)
                {
                    response.EnsureSuccessStatusCode();
                    String responseBody = response.Content.ReadAsStringAsync().Result;
                    WebClient cliente = new WebClient();
                    cliente.DownloadFileAsync(new Uri(responseBody, UriKind.Absolute), caminhoRetorno);
                    Stream stream = new MemoryStream();

                    return caminhoRetorno;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método para enviar o comando de post para o Manager e-Doc
        /// </summary>
        /// <param name="metodo">Método para onde será enviado o post</param>
        /// <param name="parms">Parametros a ser submetido</param>
        /// <returns>string com o resultado do post</returns>
        private string HttpClientPostAsync(string metodo, Dictionary<string, string> parms)
        {
            using (HttpClient client = new HttpClient())
            {
                string uri;
                string content;
                IniciaHttpClientManagerEdoc(client, metodo, parms, out uri, out content);
                using (HttpResponseMessage response = client.PostAsync(uri + content, new StringContent("")).Result)
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    return responseBody;
                }
            }
        }

        /// <summary>
        /// Configura o HttpClient para comunicação com o Manager e-Doc
        /// </summary>
        /// <param name="client">HttpClient a ser configurado</param>
        /// <param name="metodo">Método que será utilizado so Manager e-Doc (Consulta, Envia ...)</param>
        /// <param name="parms">Lista onde será adicionado os parâmetros basicos para comunicação</param>
        /// <param name="uri">Uri com o endereço e porta do Manager e-Doc</param>
        /// <param name="conteudo">Dados dos parâmetros a ser adicionado na URL</param>
        private void IniciaHttpClientManagerEdoc(HttpClient client, string metodo, Dictionary<string, string> parms, out string uri, out string conteudo)
        {
            client.DefaultRequestHeaders.Accept.Add(
                            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xml"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", ConfigManager.usuario, ConfigManager.senha))));
            parms["grupo"] = ConfigManager.grupo;
            parms["cnpj"] = Funcoes.LimpaStr(ConfigManager.cnpj);
            parms["ignorarModo"] = "True";
            uri = ConfigManager.host;
            if (!uri.Contains("http://"))
            {
                uri = "http://" + ConfigManager.host;
            }
            if (!uri.Substring(uri.Length-1,1).Contains('/'))
            {
                uri += "/";
            }
            uri = uri+"ManagerAPIWeb/nfce/" + metodo + "?";
            conteudo = string.Join("&", parms.Select(x => x.Key + "=" + x.Value).ToArray());
        }
    }
}
