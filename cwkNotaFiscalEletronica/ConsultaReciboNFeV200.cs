using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace cwkNotaFiscalEletronica
{
    public class ConsultaReciboNFeV200
    {
        public ConsultaReciboNFeV200 primFilho; //Na hora da serialização não esta reconhecendo como um objeto filho e sim como um unico objeto. 
                                                //Possivelmente com um objeto diferente possa resolver.

        public String _versao;
        public Int32 _tpAmb;
        public String _verAplic;
        public String _nRec;
        public String _nProt;
        public String _digVal;
        public Int32 _cStat;
        public String _xMotivo;
        public String _cUF;
        public String _dhRecibo;
        public Int32 _cMsg;
        public String _xMsg;
        public Int32 _Id;
        public String _chNFe;
        public String _Signature; 

        private String versao
        {
            get
            {
                return _versao;
            }
            set
            {
                _versao = value;
            }
        }

        private Int32 tpAmb
        {
            get
            {
                return _tpAmb;
            }
            set
            {
                _tpAmb = value;
            }
        }

        private String verAplic
        {
            get
            {
                return _verAplic;
            }
            set
            {
                _verAplic = value;
            }
        }

        private String nRec
        {
            get
            {
               return _nRec;
            }
            set
            {
                _nRec = value;
            }
        }

        private Int32 cStat
        {
            get
            {
                return _cStat;
            }
            set
            {
                _cStat = value;
            }
        }

        private String xMotivo
        {
            get
            {
                return _xMotivo;
            }
            set
            {
                _xMotivo = value;
            }
        }

        private String cUF
        {
            get
            {
                return _cUF;
            }
            set
            {
                _cUF = value;
            }
        }

        private String dhRecibo
        {
            get
            {
                return _dhRecibo;
            }
            set
            {
                _dhRecibo = value;
            }
        }

        private Int32 cMsg
        {
            get
            {
                return _cMsg;
            }
            set
            {
                _cMsg = value;
            }
        }

        private String xMsg
        {
            get
            {
                return _xMsg;
            }
            set
            {
                _xMsg = value;
            }
        }

        private Int32 Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }

        private String chNFe
        {
            get
            {
                return _chNFe;
            }
            set
            {
                _chNFe = value;
            }
        }

        private Int32 nProt
        {
            get
            {
                return nProt;
            }
            set
            {
                nProt = value;
            }
        }

        private String digVal
        {
            get
            {
                return _digVal;
            }
            set
            {
                _digVal = value; 
            }
        }

        private String Signature
        {
            get
            {
                return _Signature;
            }
            set
            {
                _Signature = value;
            }
        }
    }
}
