﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace cwkNotaFiscalEletronica
{
    public enum TipoRetorno
    {
        Codigo = 1,
        Motivo = 2
    }

    public enum TipoNotaEntSaida
    { 
        Entrada = 0,
        Saida = 1
    }

    public enum TipoST
    {
        Substituído = 0,
        Substituto = 1
    }

    public enum Danfe
    { 
        Retrato = 1,
        Paisagem = 2,
        Simplificado = 3
    }

    public enum DanfeNFCe
    {
        DANFE = 4,
        msgEletronica = 5
    }

    //public enum TipoEmissao
    //{
    //    Normal = 1,
    //    FS = 2,
    //    SCAN = 3,
    //    DPEC = 4,
    //    FSDA = 5,
    //    SVCAN = 6,
    //    SVCRS = 7
    //}

    public enum cwkAmbiente
    {
        Producao = 1,
        Homologacao = 2
    }

    public enum TipoRetornoDoEnvio
    { 
        ErroFormatacao = 0,
        ServidorOffline = 1,
        NaoAceito = 2,
        Aceito = 3

    }
    
    public enum VersaoXML
    { 
        v3 = 0,
        v4 = 1,
        v5a = 2,
        v6 = 3
    }

    public enum EmpresaCRT
    { 
        SimplesNacional = 1,
        SimplesNacionalExcedido = 2,
        Normal = 3
    }

    public enum TipoDoCertificado
    { 
        ckFile = 0,
        ckSmartCard = 1,
        ckMemory = 2,
        ckLocalMachine = 3,
        ckActiveDirectory = 4
        
    }
    
    //public enum FinalidadeNFe
    //{
    //    Normal = 1,
    //    Complementar = 2,
    //    Ajuste = 3,
    //    DevolucaoRetorno = 4 
    //}
   
    public enum TipoOperacaoNota
    {
        OperacaoInterna = 1,
        OperacaoInterestadual = 2,
        OperacaoComExterior = 3
    }

    public enum IdentificadorConsumidorFinal
    {
        Nao = 0,
        ConsumidorFinal = 1
    }

    public enum IndPres
    {
        OperacaoPresencial = 1,
        OperacaoNaoPresencialInternet = 2,
        OperacaoNaoPresencialAtendimento = 3,
        OperacaoPresencialForaEstabelecimento = 5,
        OperacaoNaoPresencialOutros = 9
    }
}
