﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code. Version 3.4.0.32989
//    <NameSpace>testeNFCe</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><EnableLazyLoading>False</EnableLazyLoading><TrackingChangesEnable>False</TrackingChangesEnable><GenTrackingClasses>False</GenTrackingClasses><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>False</EnableSummaryComment><VirtualProp>False</VirtualProp><IncludeSerializeMethod>True</IncludeSerializeMethod><UseBaseClass>True</UseBaseClass><GenBaseClass>True</GenBaseClass><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>False</GenerateDataContracts><CodeBaseTag>Net40</CodeBaseTag><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><GenerateXMLAttributes>True</GenerateXMLAttributes><OrderXMLAttrib>True</OrderXMLAttrib><EnableEncoding>False</EnableEncoding><AutomaticProperties>True</AutomaticProperties><GenerateShouldSerialize>False</GenerateShouldSerialize><DisableDebug>False</DisableDebug><PropNameSpecified>Default</PropNameSpecified><Encoder>UTF8</Encoder><CustomUsings></CustomUsings><ExcludeIncludedTypes>False</ExcludeIncludedTypes><EnableInitializeFields>True</EnableInitializeFields>
//  </auto-generated>
// ------------------------------------------------------------------------------
namespace cwkNotaFiscalEletronica
{
    using System;
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Xml.Schema;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;


    #region Base entity class
    public partial class EntityBase<T>
    {

        private static System.Xml.Serialization.XmlSerializer serializer;

        private static System.Xml.Serialization.XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize
        /// <summary>
        /// Serializes current EntityBase object into an XML document
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            System.IO.StreamReader streamReader = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                Serializer.Serialize(memoryStream, this);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                streamReader = new System.IO.StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes workflow markup into an EntityBase object
        /// </summary>
        /// <param name="xml">string workflow markup to deserialize</param>
        /// <param name="obj">Output EntityBase object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string xml, out T obj, out System.Exception exception)
        {
            exception = null;
            obj = default(T);
            try
            {
                obj = Deserialize(xml);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string xml, out T obj)
        {
            System.Exception exception = null;
            return Deserialize(xml, out obj, out exception);
        }

        public static T Deserialize(string xml)
        {
            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(xml);
                return ((T)(Serializer.Deserialize(System.Xml.XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        /// <summary>
        /// Serializes current EntityBase object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an EntityBase object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output EntityBase object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this XmlSerializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out T obj, out System.Exception exception)
        {
            exception = null;
            obj = default(T);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out T obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static T LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }
        #endregion
    }
    #endregion

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.portalfiscal.inf.br/nfe")]
    [System.Xml.Serialization.XmlRootAttribute("retConsReciNFe", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public partial class TRetConsReciNFe : EntityBase<TRetConsReciNFe>
    {

        private string verAplicField;

        private string nRecField;

        private string cStatField;

        private string xMotivoField;

        private string dhRecbtoField;

        private string cMsgField;

        private string xMsgField;

        private List<TProtNFe> protNFeField;

        private string versaoField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public int tpAmb { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public string verAplic { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public string nRec { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public string cStat { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public string xMotivo { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public TCodUfIBGE cUF { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public string dhRecbto { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
        public string cMsg { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 8)]
        public string xMsg { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string versao { get; set; }


        public TRetConsReciNFe()
        {
            this.protNFeField = new List<TProtNFe>();
        }

        [System.Xml.Serialization.XmlElementAttribute("protNFe", Order = 9)]
        public List<TProtNFe> protNFe
        {
            get
            {
                return this.protNFeField;
            }
            set
            {
                this.protNFeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.portalfiscal.inf.br/nfe")]
    //public enum TAmb
    //{

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlEnumAttribute("1")]
    //    Producao,

    //    /// <remarks/>
    //    [System.Xml.Serialization.XmlEnumAttribute("2")]
    //    Homologacao,
    //}

    public enum TCodUfIBGE
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("11")]
        Item11,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("12")]
        Item12,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("13")]
        Item13,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("14")]
        Item14,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("15")]
        Item15,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("16")]
        Item16,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("17")]
        Item17,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("21")]
        Item21,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("22")]
        Item22,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("23")]
        Item23,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("24")]
        Item24,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("25")]
        Item25,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("26")]
        Item26,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("27")]
        Item27,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("28")]
        Item28,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("29")]
        Item29,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("31")]
        Item31,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("32")]
        Item32,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("33")]
        Item33,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("35")]
        Item35,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("41")]
        Item41,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("42")]
        Item42,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("43")]
        Item43,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("50")]
        Item50,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("51")]
        Item51,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("52")]
        Item52,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("53")]
        Item53,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.portalfiscal.inf.br/nfe")]
    public partial class TProtNFe : EntityBase<TProtNFe>
    {

        private TProtNFeInfProt infProtField;



        private string versaoField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public TProtNFeInfProt infProt { get; set; }


        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string versao { get; set; }


        public TProtNFe()
        {
       
            this.infProtField = new TProtNFeInfProt();
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.portalfiscal.inf.br/nfe")]
    public partial class TProtNFeInfProt : EntityBase<TProtNFeInfProt>
    {

        private string verAplicField;

        private string chNFeField;

        private string dhRecbtoField;

        private string nProtField;

        private byte[] digValField;

        private string cStatField;

        private string xMotivoField;

        private string idField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public int tpAmb { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public string verAplic { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public string chNFe { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public string dhRecbto { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public string nProt { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public string cStat { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
        public string xMotivo { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id { get; set; }


    //    [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary", Order = 5)]
    //    public byte[] digVal
    //    {
    //        get
    //        {
    //            return this.digValField;
    //        }
    //        set
    //        {
    //            this.digValField = value;
    //        }
    //    }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class X509DataType : EntityBase<X509DataType>
    {

        private byte[] x509CertificateField;

        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary", Order = 0)]
        public byte[] X509Certificate
        {
            get
            {
                return this.x509CertificateField;
            }
            set
            {
                this.x509CertificateField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class KeyInfoType : EntityBase<KeyInfoType>
    {

        private X509DataType x509DataField;

        private string idField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public X509DataType X509Data { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id { get; set; }


        public KeyInfoType()
        {
            this.x509DataField = new X509DataType();
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureValueType : EntityBase<SignatureValueType>
    {

        private string idField;

        private byte[] valueField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id { get; set; }


        [System.Xml.Serialization.XmlTextAttribute(DataType = "base64Binary")]
        public byte[] Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class TransformType : EntityBase<TransformType>
    {

        private List<string> xPathField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TTransformURI Algorithm { get; set; }


        public TransformType()
        {
            this.xPathField = new List<string>();
        }

        [System.Xml.Serialization.XmlElementAttribute("XPath", Order = 0)]
        public List<string> XPath
        {
            get
            {
                return this.xPathField;
            }
            set
            {
                this.xPathField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public enum TTransformURI
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("http://www.w3.org/2000/09/xmldsig#enveloped-signature")]
        httpwwww3org200009xmldsigenvelopedsignature,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("http://www.w3.org/TR/2001/REC-xml-c14n-20010315")]
        httpwwww3orgTR2001RECxmlc14n20010315,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class ReferenceType : EntityBase<ReferenceType>
    {

        private List<TransformType> transformsField;

        private ReferenceTypeDigestMethod digestMethodField;

        private byte[] digestValueField;

        private string idField;

        private string uRIField;

        private string typeField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public ReferenceTypeDigestMethod DigestMethod { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string URI { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Type { get; set; }


        public ReferenceType()
        {
            this.digestMethodField = new ReferenceTypeDigestMethod();
            this.transformsField = new List<TransformType>();
        }

        [System.Xml.Serialization.XmlArrayAttribute(Order = 0)]
        [System.Xml.Serialization.XmlArrayItemAttribute("Transform", IsNullable = false)]
        public List<TransformType> Transforms
        {
            get
            {
                return this.transformsField;
            }
            set
            {
                this.transformsField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary", Order = 2)]
        public byte[] DigestValue
        {
            get
            {
                return this.digestValueField;
            }
            set
            {
                this.digestValueField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class ReferenceTypeDigestMethod : EntityBase<ReferenceTypeDigestMethod>
    {

        private string algorithmField;

        public ReferenceTypeDigestMethod()
        {
            this.algorithmField = "http://www.w3.org/2000/09/xmldsig#sha1";
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignedInfoType : EntityBase<SignedInfoType>
    {

        private SignedInfoTypeCanonicalizationMethod canonicalizationMethodField;

        private SignedInfoTypeSignatureMethod signatureMethodField;

        private ReferenceType referenceField;

        private string idField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public SignedInfoTypeCanonicalizationMethod CanonicalizationMethod { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public SignedInfoTypeSignatureMethod SignatureMethod { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public ReferenceType Reference { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id { get; set; }


        public SignedInfoType()
        {
            this.referenceField = new ReferenceType();
            this.signatureMethodField = new SignedInfoTypeSignatureMethod();
            this.canonicalizationMethodField = new SignedInfoTypeCanonicalizationMethod();
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignedInfoTypeCanonicalizationMethod : EntityBase<SignedInfoTypeCanonicalizationMethod>
    {

        private string algorithmField;

        public SignedInfoTypeCanonicalizationMethod()
        {
            this.algorithmField = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignedInfoTypeSignatureMethod : EntityBase<SignedInfoTypeSignatureMethod>
    {

        private string algorithmField;

        public SignedInfoTypeSignatureMethod()
        {
            this.algorithmField = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string Algorithm
        {
            get
            {
                return this.algorithmField;
            }
            set
            {
                this.algorithmField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.79.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureType : EntityBase<SignatureType>
    {

        private SignedInfoType signedInfoField;

        private SignatureValueType signatureValueField;

        private KeyInfoType keyInfoField;

        private string idField;

        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public SignedInfoType SignedInfo { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public SignatureValueType SignatureValue { get; set; }

        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public KeyInfoType KeyInfo { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id { get; set; }


        public SignatureType()
        {
            this.keyInfoField = new KeyInfoType();
            this.signatureValueField = new SignatureValueType();
            this.signedInfoField = new SignedInfoType();
        }
    }
}
