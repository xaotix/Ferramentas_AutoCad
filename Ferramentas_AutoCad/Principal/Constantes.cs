﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoeditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferramentas_DLM
{
    public enum Tipo_Objeto
    {
        Texto,
        Bloco,
    }
    public enum Tipo_Calculo_Contorno
    {
        Maximo,
        Bordas
    }
    public enum Opcao
    {
        Nao = 0,
        Sim = 1,
    }
    public enum Tipo_Coordenada
    {
        Furo_Vista,
        Furo_Corte,
        Linha,
        Projecao,
        Sem,
        Ponto,
        Bloco,
    }

    public enum Tipo_Bloco
    {
        Chapa,
        Perfil,
        Elemento_M2,
        Elemento_Unitario,
        Arremate,
        DUMMY,
        DUMMY_Perfil,
        _ = -1
    }

    public enum Tipo_Marca
    {
        MarcaComposta,
        MarcaSimples,
        Posicao,
        _ = -1
    }
    public enum Tipo_Chapa
    {
        Fina,
        Grossa,
        Tudo,
    }

    internal static class CAD
    {
        public static DocumentCollection documentManager
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager;
            }
        }
        public static Document acDoc
        {
            get
            {
                return Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            }
        }
        public static Editor editor
        {
            get
            {
                return acDoc.Editor;
            }
        }
        public static Database acCurDb
        {
            get
            {
                return acDoc.Database;
            }
        }
        public static dynamic acadApp
        {
            get
            {
                dynamic acadApp = Autodesk.AutoCAD.ApplicationServices.Application.AcadApplication;
                return acadApp;
            }
        }
    }
    internal static class Constantes
    {

        public static string RaizApp { get; set; } =@"C:\Medabil\Ferramentas_DLM\";
        public static string Raiz { get; set; } = @"\\10.54.0.4\BancoDeDados\";
        public static string Raiz_Blocos_A2 { get; set; } = Raiz + @"Blocos\SELO A2\";
        public static string Raiz_Blocos_Listagem { get; set; } = Raiz_Blocos_A2 + @"Listagem\";
        public static string Raiz_Blocos_Pcs { get; set; } = Raiz_Blocos_Listagem + @"Peças Mapeáveis\";
        public static string Raiz_Blocos_Indicacao { get; set; } = Raiz_Blocos_A2 + @"Indicação\";
        public static string Raiz_Blocos_TecnoMetal_Marcacao { get; set; } = Raiz + @"Simbologias\usr\";
        public static string Raiz_Blocos_TecnoMetal_Simbologias { get; set; } = Raiz + @"Simbologias\";
        public static string DBPROF { get; set; } = Raiz + @"\DB2011\DBPROF.dbf";

        public static string Arquivo_CTV { get; set; } = RaizApp + "DE_PARA_CTV.CFG";
        public static string Arquivo_Ignorar { get; set; } = RaizApp + "IGNORAR.CFG";
        private static List<CTV_de_para> _cts { get; set; }

        private static List<string> _ignorar { get; set; }

        public static List<string> Ignorar()
        {
            if(_ignorar ==null)
            {
                _ignorar = new List<string>();
                _ignorar = Conexoes.Utilz.Arquivo.Ler(Arquivo_Ignorar);
            }
            return _ignorar;
        }
        public static List<CTV_de_para> CTVs()
        {
            if (_cts == null)
            {
                _cts = new List<CTV_de_para>();
                var arq = Arquivo_CTV;
                var linhas = Conexoes.Utilz.Arquivo.Ler(arq);
                if (linhas.Count > 1)
                {
                    for (int i = 1; i < linhas.Count; i++)
                    {
                        var col = linhas[i].Split(';').ToList();
                        if (col.Count >= 5)
                        {
                            _cts.Add(
                                new CTV_de_para(
                                col[0],
                                col[2],
                                Conexoes.Utilz.Double(col[1]),
                                Conexoes.Utilz.Int(col[3]),
                                col[4]
                                )
                                );
                        }
                    }
                }
            }
            return _cts;
        }

        public static string Tabela_Correntes_Titulo { get; set; } = Raiz_Blocos_Listagem + @"CORRENTES_TITULO.dwg";
        public static string Tabela_Correntes { get; set; } = Raiz_Blocos_Listagem + @"CORRENTES.dwg";
        public static string Tabela_Tercas_Titulo { get; set; } = Raiz_Blocos_Listagem + @"TERCAS_TITULO.dwg";
        public static string Tabela_Almox_Titulo { get; set; } = Raiz_Blocos_Listagem + @"ALMOX_TITULO.dwg";
        public static string Tabela_Almox { get; set; } = Raiz_Blocos_Listagem + @"ALMOX.dwg";
        public static string Tabela_Tirantes_Titulo { get; set; } = Raiz_Blocos_Listagem + @"TIRANTES_TITULO.dwg";
        public static string Tabela_Tirantes { get; set; } = Raiz_Blocos_Listagem + @"TIRANTES.dwg";
        public static string Tabela_Tercas { get; set; } = Raiz_Blocos_Listagem + @"TERCAS.dwg";
        public static string Tabela_TecnoMetal { get; set; } = Raiz_Blocos_Listagem + @"TECNOMETAL_TAB_LIN.dwg";
        public static string Tabela_TecnoMetal_Titulo { get; set; } = Raiz_Blocos_Listagem + @"TECNOMETAL_TAB_CAB.dwg";
        public static string Tabela_TecnoMetal_Vazia { get; set; } = Raiz_Blocos_Listagem + @"TECNOMETAL_TAB_LIN_VAZIA.dwg"; 
        public static string Tabela_Pecas_Titulo { get; set; } = Raiz_Blocos_Listagem + @"PECAS_TITULO.dwg";
        public static string Tabela_Pecas_Linha { get; set; } = Raiz_Blocos_Listagem + @"PECAS_LINHA.dwg";


        public static string Incicacao_Tercas { get; set; } = Raiz_Blocos_Listagem + @"TERCA_INDICACAO.dwg";
        public static string Indicacao_Tirantes { get; set; } = Raiz_Blocos_Listagem + @"TIRANTE_INDICACAO.dwg";
        public static string Indicacao_Correntes { get; set; } = Raiz_Blocos_Listagem + @"CORRENTE_INDICACAO.dwg";

        public static string Texto { get; set; } = Raiz_Blocos_Listagem + @"TEXTO.dwg";


        public static string BL_M_Composta { get; set; } = "M8_COM";
        public static string BL_M_PERF { get; set; } = "M8_PRO";
        public static string BL_M_CH { get; set; } = "M8_LAM";
        public static string BL_M_ARR { get; set; } = "M8_LAM_ARR";
        public static string BL_M_ELEM2 { get; set; } = "M8_ELE";
        public static string BL_M_ELUNIT { get; set; } = "M8_ELU";


        public static string BL_P_PERF { get; set; } = "P8_PRO";
        public static string BL_P_CH { get; set; } = "P8_LAM";
        public static string BL_P_ELEM2 { get; set; } = "P8_ELE";
        public static string BL_P_ELUNIT { get; set; } = "P8_ELU";


        public static string BL_SubConjunto { get; set; } = "S8_ASS";
        public static string BL_Posicao_Perfil_M { get; set; } = "P8_PRO_M";
        public static string BL_Posicao_Chapa_M { get; set; } = "P8_LAM_M";
        public static string BL_Posicao_Elemento_M2_M { get; set; } = "P8_ELE_M";
        public static string BL_Posicao_Elemento_Unitario_M { get; set; } = "P8_ELU_M";
        public static string BL_Posicao_Repetida { get; set; } = "P8_RIP";
        public static string BL_Posicao_Parafuso { get; set; } = "B8_BUL";
        public static string BL_Posicao_Elemento { get; set; } = "VIT_4";
        public static string BL_Marca_Repetida { get; set; } = "R8_COM";

        public static List<string> BlocosTecnoMetalMarcas
        {
            get
            {
                return new List<string>() { 
                    BL_M_Composta, 
                    BL_M_ELEM2, 
                    BL_M_ELUNIT, 
                    BL_M_CH, 
                    BL_M_PERF, 
                    BL_M_ARR
                };
            }
        }
        public static List<string> BlocosTecnoMetalPosicoes
        {
            get
            {
                return new List<string>() { 
                    BL_P_PERF, 
                    BL_P_CH, 
                    BL_P_ELEM2, 
                    BL_P_ELUNIT 
                };
            }
        }

        public static List<string> LayersMarcasDesligar
        {
            get
            {
                return new List<string>() { 
                    "POS_NOT", 
                    "POS_MAT", 
                    "POS_LIN", 
                    "POS_DES", 
                    "MARK_SBX", 
                    "MARK_SBA", 
                    "MARK_MAT", 
                    "MARK_DES", 
                    "MARK_CIR", 
                    "MARK_BOX" 
                };
            }
        }

        public static List<string> BlocosTecnoMetal
        {
            get
            {
                List<string> retorno = new List<string>();
                retorno.AddRange(BlocosTecnoMetalMarcas);
                retorno.AddRange(BlocosTecnoMetalPosicoes);
                return retorno;
            }
        }

        public static List<string> BlocosIndicacao()
        {
            return Conexoes.Utilz.GetArquivos(Raiz_Blocos_Indicacao, "*.dwg");
        }


        public static string Bloco_Indicacao_Texto { get; set; } = Raiz_Blocos_Indicacao + "PECA_INDICACAO_TEXTO.dwg";

        public static string Marca_Composta { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_M_Composta}.dwg";
        public static string Marca_Perfil { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_M_PERF}.dwg";
        public static string Marca_Chapa { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_M_CH}.dwg";
        public static string Marca_Arremate { get; set; } = Raiz + $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_M_ARR}.dwg";
        public static string Marca_Elemento_M2 { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_M_ELEM2}.dwg";
        public static string Marca_Elemento_Unitario { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_M_ELUNIT}.dwg";

        public static string Posicao_Perfil { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_P_PERF}.dwg";
        public static string Posicao_Chapa { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_P_CH}.dwg";
        public static string Posicao_Elemento_M2 { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_P_ELEM2}.dwg";
        public static string Posicao_Elemento_Unitario { get; set; } = $@"{Raiz_Blocos_TecnoMetal_Marcacao }{BL_P_ELUNIT}.dwg";

        public static string Peca_ESTICADOR { get; set; } = Raiz_Blocos_Pcs + @"ESTICADOR.dwg";
        public static string Peca_MANILHA { get; set; } = Raiz_Blocos_Pcs + @"MANILHA.dwg";
        public static string Peca_PASSARELA { get; set; } = Raiz_Blocos_Pcs + @"PASSARELA.dwg";
        public static string Peca_SFLH { get; set; } = Raiz_Blocos_Pcs + @"SFLH.dwg";
        public static string Peca_SFLI { get; set; } = Raiz_Blocos_Pcs + @"SFLI.dwg";


        public static string Bloco_3D_Montagem_Tecnometal { get; set; } = "3D_INFOM";
        public static string Bloco_3D_Montagem_Info { get; set; } = "3D_INFOTEXT";

        /// <summary>
        /// Subconjunto<br/>
        /// SBA_PEZ
        /// </summary>
        public static string ATT_SUB { get; set; } = "SBA_PEZ";
        /// <summary>
        /// Marca<br/>
        /// MAR_PEZ
        /// </summary>
        public static string ATT_MAR { get; set; } = "MAR_PEZ";
        /// <summary>
        /// Posição<br/>
        /// POS_PEZ
        /// </summary>
        public static string ATT_POS { get; set; } = "POS_PEZ";
        /// <summary>
        /// Perfil<br/>
        /// NOM_PRO
        /// </summary>
        public static string ATT_PER { get; set; } = "NOM_PRO";
        /// <summary>
        /// Quantidade<br/>
        /// QTA_PEZ
        /// </summary>
        public static string ATT_QTD { get; set; } = "QTA_PEZ";
        /// <summary>
        /// Comprimento<br/>
        /// LUN_PRO
        /// </summary>
        public static string ATT_CMP { get; set; } = "LUN_PRO";
        /// <summary>
        /// Largura<br/>
        /// LAR_PRO
        /// </summary>
        public static string ATT_LRG { get; set; } = "LAR_PRO";
        /// <summary>
        /// Espessura<br/>
        /// SPE_PRO
        /// </summary>
        public static string ATT_ESP { get; set; } = "SPE_PRO";
        /// <summary>
        /// Material<br/>
        /// MAT_PRO
        /// </summary>
        public static string ATT_MAT { get; set; } = "MAT_PRO";
        /// <summary>
        /// Tratamento<br/>
        /// TRA_PEZ
        /// </summary>
        public static string ATT_FIC { get; set; } = "TRA_PEZ";
        /// <summary>
        /// Peso<br/>
        /// PUN_LIS
        /// </summary>
        public static string ATT_PES { get; set; } = "PUN_LIS";
        /// <summary>
        /// Superficie<br/>
        /// SUN_LIS
        /// </summary>
        public static string ATT_SUP { get; set; } = "SUN_LIS";
        /// <summary>
        /// Mercadoria<br/>
        /// DES_PEZ
        /// </summary>
        public static string ATT_MER { get; set; } = "DES_PEZ";
        /// <summary>
        /// Notas<br/>
        /// NOT_PEZ
        /// </summary>
        public static string ATT_NOT { get; set; } = "NOT_PEZ";
        /// <summary>
        /// Tipologia Construtiva<br/>
        /// TIP_PEZ
        /// </summary>
        public static string ATT_TPC { get; set; } = "TIP_PEZ";
        /// <summary>
        /// Voluma (Comp X Larg x Alt)<br/>
        /// ING_PEZ
        /// </summary>
        public static string ATT_VOL { get; set; } = "ING_PEZ";
        /// <summary>
        /// Ciclo de trabalho<br/>
        /// MCL_PEZ
        /// </summary>
        public static string ATT_CIC { get; set; } = "MCL_PEZ";
        /// <summary>
        /// Código SAP<br/>
        /// COD_PEZ
        /// </summary>
        public static string ATT_SAP { get; set; } = "COD_PEZ";
        /// <summary>
        /// Código de Custo<br/>
        /// COS_PEZ
        /// </summary>
        public static string ATT_CCC { get; set; } = "COS_PEZ";
        /// <summary><br/>
        /// Geometria (dados do banco de dados do perfil)<br/>
        /// DIM_PRO
        /// </summary>
        public static string ATT_GEO { get; set; } = "DIM_PRO";
        /// <summary>
        /// Pedido<br/>
        /// Somente para DBF<br/>
        /// NUM_COM
        /// </summary>
        public static string ATT_PED { get; set; } = "NUM_COM";
        /// <summary>
        /// Obra<br/>
        /// Somente para DBF<br/>
        /// DES_COM
        /// </summary>
        public static string ATT_OBR { get; set; } = "DES_COM";
        /// <summary>
        /// Subetapa<br/>
        /// Somente para DBF<br/>
        /// LOT_COM
        /// </summary>
        public static string ATT_ETP { get; set; } = "LOT_COM";
        /// <summary>
        /// Prancha<br/>
        /// Somente para DBF<br/>
        /// FLG_DWG
        /// </summary>
        public static string ATT_DWG { get; set; } = "FLG_DWG";
        /// <summary>
        /// Prancha<br/>
        /// Somente para DBF<br/>
        /// NUM_DIS
        /// </summary>
        public static string ATT_NUM { get; set; } = "NUM_DIS";

        /// <summary>
        /// Tipo 03 = MARCA / 04 = POSIÇÃO <br/>
        /// Somente para DBF<br/>
        /// FLG_REC
        /// </summary>
        public static string ATT_REC { get; set; } = "FLG_REC";
        /// <summary>
        /// Data
        /// Somente para DBF<br/>
        /// DAT_DIS
        /// </summary>
        public static string ATT_DAT { get; set; } = "DAT_DIS";

        public static string ATT_REC_MARCA { get; set; } = "03";
        public static string ATT_REC_POSICAO { get; set; } = "04";

        /// <summary>
        /// Nome do arquivo
        /// Somente para DBF<br/>
        /// Não grava no arquivo, somente para lógicas internas
        /// ARQUIVO
        /// </summary>
        public static string ATT_ARQ { get; set; } = "ARQUIVO";


        /// <summary>
        /// Descrição da Obra
        /// Somente para DBF<br/>
        /// CLI_COM
        /// </summary>
        public static string ATT_CLI { get; set; } = "CLI_COM";

        /// <summary>
        /// Prefixo Chapa
        /// Somente para DBF<br/>
        /// PRE_LIS
        /// </summary>
        public static string ATT_PRE { get; set; } = "PRE_LIS";

        /// <summary>
        /// Nome do bloco
        /// Somente para funções internas<br/>
        /// BLOCO
        /// </summary>
        public static string ATT_BLK { get; set; } = "BLOCO";
    }
}