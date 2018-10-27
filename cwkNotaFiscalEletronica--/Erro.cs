using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace cwkNotaFiscalEletronica
{
    public class Erro
    {
        public int Id { get; set; }
        public string Instrucao { get; set; }

        internal Erro(int id) {
            XElement xml = XElement.Load("xmlTeste.xml");

            XElement consulta = (from p in xml.Elements("Erro")
                                 where (int)p.Element("Id") == id
                                 select p)
                                .Single<XElement>();

            this.Id = Convert.ToInt32(consulta.Element("Id").Value);
            this.Instrucao = Convert.ToString(consulta.Element("Instrucao").Value);

        }
    }
}