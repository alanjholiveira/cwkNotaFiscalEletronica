using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cwkNotaFiscalEletronica.Modelo
{
    public class ConfiguracaoManager
    {
        public virtual string grupo { get; set; }
        public virtual string usuario { get; set; }
        public virtual string senha { get; set; }
        public virtual string host { get; set; }
        public virtual string cnpj { get; set; }
    }
}
