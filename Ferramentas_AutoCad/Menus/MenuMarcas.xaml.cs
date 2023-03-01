﻿using Conexoes;
using DLM.db;
using DLM.encoder;
using DLM.vars;
using DLM.vars.cad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DLM.cad
{
    /// <summary>
    /// Interação lógica para MenuMarcas.xam
    /// </summary>
    public partial class MenuMarcas : Window
    {
        public List<MarcaTecnoMetal> Marcas { get; set; } = new List<MarcaTecnoMetal>();
        public List<MarcaTecnoMetal> Posicoes { get; set; } = new List<MarcaTecnoMetal>();

        public void Iniciar()
        {
            if(this.IsLoaded)
            {
                this.Update();
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Show();
            }
        }

        public int Sufix_Count { get; set; } = 1;
        public double Escala { get; set; } = 1;

        public string NomeFim
        {
            get
            {
                return this.prefix.Text + this.sufix.Text;
            }
        }

        private Tipo_Bloco tipo
        {
            get
            {
                var s = combo_tipo_marca.SelectedItem;
                if (s is Tipo_Bloco)
                {
                    return (Tipo_Bloco)s;
                }

                return Tipo_Bloco._;
            }
        }


        public MarcaTecnoMetal marca_selecionada
        {
            get
            {
                if(this.seleciona_marca_composta.SelectedItem is MarcaTecnoMetal)
                {
                    return this.seleciona_marca_composta.SelectedItem as MarcaTecnoMetal;
                }
                return null;
            }
        }
        public static Conexoes.Chapa db_chapa { get; set; }
        public static Conexoes.RMA db_unitario { get; set; }
        public static Conexoes.Bobina db_bobina { get; set; }
        public static DLM.cam.Perfil db_perfil { get; set; }
        public static DLM.cam.Perfil db_perfil_m2 { get; set; }



        public MenuMarcas(CADTecnoMetal tecnoMetal)
        {

            InitializeComponent();

            try
            {

                this.Title = $"Medabil Plugin CAD V." + Conexoes.Utilz.GetVersao(Cfg.Init.CAD_DLL_Local) + $" [{DLM.vars.Cfg_User.Init.MySQL_Servidor}]";




                this.Escala = Core.TecnoMetal.GetEscala();

                combo_tipo_marca.ItemsSource = Conexoes.Utilz.GetLista_Enumeradores<Tipo_Bloco>().ToList().FindAll(x=> x!= Tipo_Bloco._ && x!= Tipo_Bloco.DUMMY);

                combo_tipo_marca.SelectedIndex = 0;

                if (Conexoes.DBases.GetBancoRM().GetMercadorias().Count > 0)
                {
                    this.combo_mercadoria.Content = DBases.GetBancoRM().GetMercadorias().First();
                }
                this.bt_material.Content = Cfg.Init.Material;
                this.bt_tratamento.Content = Cfg.Init.RM_SEM_PINTURA;

                this.DataContext = this;
                Update();
            }
            catch (Exception ex)
            {
                Conexoes.Utilz.Alerta(ex);
            }


        }

        public void Update()
        {

            GetMarcas();

            SetTextos();


        }

        public void GetMarcas()
        {
            List<Report> erros = new List<Report>();
            Marcas = Core.TecnoMetal.GetMarcas(ref erros).ToList();
            Posicoes = Marcas.SelectMany(x => x.GetPosicoes()).ToList();
        }

        private void selecionar_perfil(object sender, RoutedEventArgs e)
        {

            perfil.Content = "...";
            this.Visibility = Visibility.Collapsed;
            switch (this.tipo)
            {

                case Tipo_Bloco.Chapa:
                    db_chapa = Core.TecnoMetal.PromptChapa(Tipo_Chapa.Grossa);
                    db_bobina = DBases.GetBobinaDummy(Cfg.Init.Material).Clonar();
                    if (db_chapa != null)
                    {
                        perfil.Content = db_chapa.ToString();
                        db_bobina.Espessura = db_chapa.valor;
                        db_bobina.Material = this.bt_material.Content.ToString();
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    db_perfil = Conexoes.Utilz.Selecao.SelecionarPerfil();
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    this.Visibility = Visibility.Collapsed;
                    db_perfil_m2 = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetdbPerfil().GetPerfisTecnoMetal().FindAll(x => x.Tipo== DLM.vars.CAM_PERFIL_TIPO.Chapa_Xadrez), null, "Selecione");
                    if (db_perfil_m2 != null)
                    {
                        perfil.Content = db_perfil_m2.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    db_unitario = Conexoes.Utilz.Selecao.SelecionarObjeto(DBases.GetBancoRM().GetRMAs(), null, "Selecione");
                    if (db_unitario != null)
                    {
                        perfil.Content = db_unitario.ToString();
                    }
                    break;
                case Tipo_Bloco.Arremate:
                    db_chapa = Core.TecnoMetal.PromptChapa(Tipo_Chapa.Fina);
                    if(db_chapa != null)
                    {
                        db_bobina = Core.TecnoMetal.PromptBobina(db_chapa);
                    }
                    
                    if (db_bobina != null)
                    {
                        perfil.Content = db_bobina.ToString();
                    }
                    break;
                case Tipo_Bloco._:
                    break;
            }
            this.Visibility = Visibility.Visible;

        }

        private void criar_bloco(object sender, RoutedEventArgs e)
        {
            double escala = Conexoes.Utilz.Double(txt_escala.Text);
            if(escala<1)
            {
                Conexoes.Utilz.Alerta("Valor escala inválido.");
                return;
            }
            List<Report> erros = new List<Report>();
            var ms = Core.TecnoMetal.GetMarcas(ref erros).ToList();
            var pos = ms.SelectMany(x => x.GetPosicoes()).ToList();

            var qtd_double = Conexoes.Utilz.Double(this.quantidade.Text);
            if ((bool)rad_m_composta.IsChecked)
            {
                if (this.marca_selecionada == null)
                {
                    Conexoes.Utilz.Alerta("Selecione uma marca na lista ou crie uma marca nova para poder criar essa posição.");
                    return;
                }
            }
            else
            {
                if(combo_mercadoria.Content.ToString() == "")
                {
                    Conexoes.Utilz.Alerta($"Selecione uma mercadoria.");
                    return;
                }
            }

            if(qtd_double <= 0)
            {
                Conexoes.Utilz.Alerta($"{qtd_double} quantidade inválida.");
                return;
            }


            if(NomeFim.Replace(" ","").Replace("_","").Length==0)
            {
                Conexoes.Utilz.Alerta($"Nome inválido.");
                return;
            }

           if(tipo!= Tipo_Bloco.Elemento_Unitario && NomeFim.Contains("_"))
            {
                Conexoes.Utilz.Alerta($"Nome inválido.");
                return;
            }

           if(this.Posicoes.FindAll(x=>x.Marca.ToUpper() == NomeFim.ToUpper()).Count>0)
            {
                Conexoes.Utilz.Alerta($"Nome inválido: {NomeFim} Já existe uma posição com o mesmo nome.");
                return;
            }

           if(tipo == Tipo_Bloco.Elemento_Unitario && !NomeFim.EndsWith("_A"))
            {
                Conexoes.Utilz.Alerta($"Nome inválido: {NomeFim} para elemento unitário deve sempre terminar com '_A'");
                return;
            }



            if (NomeFim.CaracteresEspeciais() | NomeFim.Contains(" "))
            {
                Conexoes.Utilz.Alerta($"Nome inválido: {NomeFim} - Nome não pode conter caracteres especiais ou espaços.");
                return;
            }



            if (ms.FindAll(x => x.Marca == NomeFim).Count > 0 | pos.FindAll(x => x.Posicao == NomeFim).Count > 0)
            {
                if (this.marca_selecionada == null)
                {
                    Conexoes.Utilz.Alerta($"{NomeFim} - Já existe uma marca / posição com o mesmo nome no desenho.");
                    return;
                }
            }

            if(tipo!= Tipo_Bloco.Elemento_Unitario)
            {
                if(!qtd_double.E_Multiplo(1))
                {
                    Conexoes.Utilz.Alerta($"Quantidade inválida: {qtd_double}. Quantidades com números quebrados somente para elemento unitário.");
                    return;
                }
            }

            switch (tipo)
            {
                case Tipo_Bloco.Chapa:
                    if (MenuMarcas.db_chapa == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione uma espessura.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    if (MenuMarcas.db_perfil == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um perfil.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    if (MenuMarcas.db_perfil_m2 == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um perfil m2.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    if (MenuMarcas.db_unitario == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione um item.");
                        return;
                    }
                    break;
                case Tipo_Bloco.Arremate:
                    if (MenuMarcas.db_bobina == null)
                    {
                        Conexoes.Utilz.Alerta("Selecione uma bobina.");
                        return;
                    }
                    break;
                case Tipo_Bloco._:
                    Conexoes.Utilz.Alerta("Seleção inválida.");
                    return;
            }



            this.Visibility = Visibility.Collapsed;

            string nomeMarca = "";
            string nomePos = "";

            if((bool)rad_m_simples.IsChecked)
            {
                nomeMarca = this.NomeFim;
            }
            else
            {
                nomeMarca = this.marca_selecionada.Marca;
                nomePos = this.NomeFim;
            }

            switch (this.tipo)
            {
                case Tipo_Bloco.Chapa:

                    var sel = Conexoes.Utilz.Selecao.SelecionaCombo(new List<string> { "Sem Dobras", "Com Dobras" }, null);
                    if(sel == "Com Dobras")
                    {
                        this.combo_mercadoria.Content = "PERFIL DOBRADO";
                        Core.TecnoMetal.InserirArremate(escala,nomeMarca, nomePos, (int)qtd_double, this.bt_tratamento.Content.ToString(), MenuMarcas.db_bobina, false, this.combo_mercadoria.Content.ToString());
                    }
                    else if(sel == "Sem Dobras")
                    {
                        Core.TecnoMetal.InserirChapa(escala, nomeMarca, nomePos, this.bt_material.Content.ToString(), (int)qtd_double, this.bt_tratamento.Content.ToString(), MenuMarcas.db_chapa, this.combo_mercadoria.Content.ToString());
                    }
                  
                    break;
                case Tipo_Bloco.Perfil:
                    Core.TecnoMetal.InserirPerfil(escala, nomeMarca, nomePos, this.bt_material.Content.ToString(), this.bt_tratamento.Content.ToString(), (int)qtd_double, MenuMarcas.db_perfil, this.combo_mercadoria.Content.ToString());
                    break;
                case Tipo_Bloco.Elemento_M2:
                    Core.TecnoMetal.InserirElementoM2(escala, nomeMarca, nomePos,this.bt_material.Content.ToString(),this.bt_tratamento.Content.ToString(), (int)qtd_double, MenuMarcas.db_perfil,this.combo_mercadoria.Content.ToString());
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    Core.TecnoMetal.InserirElementoUnitario(escala, nomeMarca, nomePos, qtd_double, this.combo_mercadoria.Content.ToString(), MenuMarcas.db_unitario);
                    break;
                case Tipo_Bloco.Arremate:
                    var sel2 = Conexoes.Utilz.Selecao.SelecionaCombo(new List<string> { "Corte", "Vista" }, null);
                    if (sel2 == "Corte")
                    {
                        Core.TecnoMetal.InserirArremate(escala, nomeMarca, nomePos, (int)qtd_double, this.bt_tratamento.Content.ToString(), MenuMarcas.db_bobina, true, this.combo_mercadoria.Content.ToString());
                    }
                    else if (sel2 == "Vista")
                    {
                        Core.TecnoMetal.InserirChapa(escala, nomeMarca, nomePos, this.bt_material.Content.ToString(), (int)qtd_double, this.bt_tratamento.Content.ToString(), MenuMarcas.db_chapa, this.combo_mercadoria.Content.ToString(), MenuMarcas.db_bobina);
                    }
                    break;
                case Tipo_Bloco._:
                    break;
            }

            if(this.Sufix_Count ==1)
            {
                FLayer.Desligar(Cfg.Init.GetLayersMarcasDesligar());
            }

            this.GetMarcas();

            this.Update();
            this.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }
        private void nova_marca_Click(object sender, RoutedEventArgs e)
        {

            this.Visibility = Visibility.Collapsed;
            
            var nova = Core.TecnoMetal.InserirMarcaComposta(Core.TecnoMetal.GetEscala());
            if (nova != null)
            {
                this.Update();
                this.seleciona_marca_composta.SelectedItem = nova;
                this.bt_tratamento.Content = nova.Tratamento;
                
            }
            this.Visibility = Visibility.Visible;

        }
        private void tipo_marca_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTextos();
        }

        private void SetTextos()
        {
            /*se a janela ainda nao carregou, ele sai*/
            if (perfil == null) { return; }

            this.seleciona_marca_composta.ItemsSource = null;
            this.seleciona_marca_composta.ItemsSource = Marcas.FindAll(x => x.Tipo_Marca == Tipo_Marca.MarcaComposta);
            if((bool)rad_m_simples.IsChecked)
            {
                this.Sufix_Count = Marcas.Count + 1;
            }
            else
            {
                this.Sufix_Count = Posicoes.Count + 1;
            }
            if (this.seleciona_marca_composta.Items.Count > 0 && (bool)rad_m_composta.IsChecked)
            {
                this.seleciona_marca_composta.Visibility = Visibility.Visible;
                if(this.seleciona_marca_composta.SelectedIndex<0)
                {
                    this.seleciona_marca_composta.SelectedIndex = 0;
                }
            }
            else
            {
                this.seleciona_marca_composta.Visibility = Visibility.Collapsed;
            }

            if((bool)rad_m_simples.IsChecked)
            {
                nova_marca.Visibility = Visibility.Collapsed;

            }
            else
            {
                nova_marca.Visibility = Visibility.Visible;
            }


           
            perfil.Content = "...";

            this.bt_criar.IsEnabled = true;

            this.sufix.Text = (Sufix_Count).ToString().PadLeft(2, '0');
            this.bt_tratamento.Visibility = Visibility.Visible;
            this.bt_material.Visibility = Visibility.Visible;

            if ((bool)rad_m_composta.IsChecked)
            {
                this.prefix.Text = "P";
            }
            else
            {
                this.prefix.Text = "M";
            }


            switch (tipo)
            {
                case Tipo_Bloco.Chapa:
                    if (db_chapa != null)
                    {
                        perfil.Content = db_chapa.ToString();
                    }
                    combo_mercadoria.Content = "CHAPA";
                    bt_material.Content = Cfg.Init.Material;
                    this.bt_tratamento.Visibility = Visibility.Visible;
                    break;
                case Tipo_Bloco.Perfil:
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil.ToString();
                    }
                    bt_material.Content = Cfg.Init.Material;
                    break;
                case Tipo_Bloco.Elemento_M2:
                    if (db_perfil_m2 != null)
                    {
                        perfil.Content = db_perfil_m2.ToString();

                    }
                    combo_mercadoria.Content = "CHAPA DE PISO";
                    bt_material.Content = "A572";

                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    if (db_unitario != null)
                    {
                        perfil.Content = db_unitario.ToString();
                    }

                    if ((bool)rad_m_simples.IsChecked)
                    {
                        bt_criar.IsEnabled = false;
                    }

                    combo_mercadoria.Content = "ALMOX";
                    bt_material.Content = "A325";
                    this.bt_material.Visibility = Visibility.Collapsed;
                    this.bt_tratamento.Content = Cfg.Init.RM_SEM_PINTURA;
                    this.sufix.Text = (Sufix_Count).ToString().PadLeft(2, '0') + "_A";
                    break;
                case Tipo_Bloco.Arremate:
                    if (db_bobina != null)
                    {
                        perfil.Content = db_bobina.ToString();
                    }
                    combo_mercadoria.Content = "ARREMATE";
                    bt_material.Content = "PP ZINC";
                    this.bt_tratamento.Content = Cfg.Init.RM_SEM_PINTURA;
                    break;
                case Tipo_Bloco._:
                    break;
            }
        }

 

        private void desliga_layer(object sender, RoutedEventArgs e)
        {

            FLayer.Desligar(Cfg.Init.GetLayersMarcasDesligar());
            this.Visibility = Visibility.Collapsed;
        }

        private void liga_layer(object sender, RoutedEventArgs e)
        {
            FLayer.Ligar(Cfg.Init.GetLayersMarcasDesligar());
            this.Visibility = Visibility.Collapsed;
        }

        private void insere_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.TecnoMetal.InserirTabela();
        }

        private void insere_tabela_auto(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.TecnoMetal.InserirTabelaAuto();
        }

        private void gerar_dbf(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Report> erros = new List<Report>();
            Core.gerardbf();
        }

        private void gerar_dbf_3d(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Report> erros = new List<Report>();
            Core.gerardbf3d();
        }

        private void mercadorias(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Report> erros = new List<Report>();
            Core.mercadorias21();
        }

        private void materiais(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Report> erros = new List<Report>();
            Core.materiais21();
        }

        private void tratamentos(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Report> erros = new List<Report>();
            Core.tratamentos21();
        }

        private void quantificar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.quantificar();
        }

        private void purlin(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.purlin();
        }

        private void cotar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.Cotar();
        }

        private void limpar_cotas(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.LCotas();
        }

        private void boneco(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.boneco();
        }

        private void passarelas(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.passarela();
        }

        private void passarelas_apaga(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.apagapassarela();
        }

        private void linha_de_vida(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.linhadevida();
        }

        private void linha_de_vida_apaga(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.apagalinhadevida();
        }

        private void linha_de_vida_alinha(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.alinharlinhadevida();
        }

        private void preenche_selo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.selopreenche();
        }

        private void limpa_selo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.selolimpar();
        }

        private void criar_marcas_cam(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.criarmarcasdecam();
        }

        private void rodar_macro(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.rodarmacros();
        }

        private void gerar_dxf_cams(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.gerardxf();
        }

        private void marcar_montagem(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.marcarmontagem();
        }

        private void rad_m_simples_Checked(object sender, RoutedEventArgs e)
        {
            SetTextos();
        }

        private void gerar_pdf(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.gerarPDFEtapa();
        }

        private void composicao(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.TecnoMetal.InserirSoldaComposicao();
        }

        private void preenche_selo_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.preencheSelo();
        }

        private void limpar_selo_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.tabela_limpa();
        }

        private void purlin_muda_perfil(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.mudaperfiltercas();
        }

        private void abre_pasta(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.abrepasta();
        }

        private void exporta_rma(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.exportarma();
        }

        private void importa_rm(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.importarm();
        }

        private void listar_quantidades(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.ListarQuantidadeBlocos();
        }

        private void bloqueia_mviews(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.bloqueiamviews();
        }

        private void desloqueia_mviews(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.desbloqueiamviews();
        }

        private void cria_layers_padrao(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.criarlayersPadrao();
        }

        private void abre_versionamento(object sender, RoutedEventArgs e)
        {
            Cfg.Init.CAD_Versionamento();
        }

        private void rodar_macros(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.rodarmacros();
        }

        private void editar_transpasse(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetTranspasse();
        }

        private void editar_ficha(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetFicha();
        }

        private void editar_trocar_perfil(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetPurlin();
        }

        private void editar_furacao_suporte(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetSuporte();
        }

        private void editar_ver_croqui(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetSuporte();
        }

        private void editar_criar_manual(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.PurlinManual();
        }

        private void editar_edicao_completa(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.EdicaoCompleta();
        }

        private void editar_corrente(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetCorrente();
        }

        private void editar_corrente_descontar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetCorrenteDescontar();
        }

        private void editar_corrente_fixador(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.SetCorrenteSuporte();
        }

        private void apagar_blocos(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.ApagarBlocosPurlin();
        }

        private void apagar_tudo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.ApagarBlocosPurlin();
        }

        private void desnha_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.desenharmline();
        }

        private void troca_polyline_por_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.substituirpolylinepormultiline();
        }

        private void trocar_multiline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.mudarmultiline();
        }

        private void marcar_inserir_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.Exportar(Conexoes.Utilz.Pergunta("Gerar Tabela?"), Conexoes.Utilz.Pergunta("Exportar arquivo .RM?"));
        }

        private void gerar_croquis(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            this.Close();
            Core.CADPurlin.GerarCroquis();

        }

        private void atualiza_peso_arremate(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
           Core.AtualizarPesoChapas();
        }

        private void criar_cam_polyline(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Core.cam_de_polilinha();
        }
        private void seleciona_tudo(object sender, RoutedEventArgs e)
        {
            TextBox textBox = null;
            if (sender is TextBox)
            {
                textBox = ((TextBox)sender);

            }


            if (textBox != null)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.SelectAll();
                }));
            }
        }

        private void set_material(object sender, RoutedEventArgs e)
        {
            var sel = DBases.GetBancoRM().GetMateriais().ListaSelecionar();
            if(sel!=null)
            {
                bt_material.Content = sel.Nome;
            }
        }

        private void set_mercadoria(object sender, RoutedEventArgs e)
        {
            var sel = DBases.GetBancoRM().GetMercadorias().ListaSelecionar();
            if(sel!=null)
            {
                combo_mercadoria.Content = sel;
            }
        }

        private void set_ficha(object sender, RoutedEventArgs e)
        {
            var sel = DBases.GetBancoRM().Get_Pinturas().ListaSelecionar();
            if(sel!=null)
            {
                bt_tratamento.Content = sel;
            }
        }
    }
}