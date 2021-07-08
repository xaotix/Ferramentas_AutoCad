﻿using System;
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

namespace Ferramentas_DLM
{
    /// <summary>
    /// Interação lógica para MenuMarcas.xam
    /// </summary>
    public partial class MenuMarcas : Window
    {
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
        public void Iniciar()
        {
            if(this.IsLoaded)
            {
                this.Update(Comandos.TecnoMetal);
                this.Visibility = Visibility.Visible;
            }
            else
            {
                this.Show();
            }
        }
        public string Prefix { get; set; } = "P";
        public string Sufix { get; set; } = "01";
        public double Quantidade { get; set; } = 1;
        public double Escala { get; set; } = 1;

        public static List<DB.Valor> lista_mercadorias { get; set; } = new List<DB.Valor>();
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
        public string db_mercadoria
        {
            get
            {
                if (combo_mercadoria.SelectedItem is string)
                {
                    return combo_mercadoria.SelectedItem as string;
                }
                return null;
            }
        }
        public string db_material
        {
            get
            {
                if (combo_material.SelectedItem is string)
                {
                    return combo_material.SelectedItem as string;
                }
                return null;
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
        public static Conexoes.TecnoMetal_Perfil db_perfil { get; set; }
        public static Conexoes.TecnoMetal_Perfil db_perfil_m2 { get; set; }

        public static TecnoMetal TecnoMetal { get; set; }

        public MenuMarcas(TecnoMetal tecnoMetal)
        {

            InitializeComponent();

            try
            {

                this.Title = $"Medabil Plugin CAD V." + Conexoes.Utilz.GetVersao(Constantes.DLL_Local);
                TecnoMetal = tecnoMetal;
                this.combo_mercadoria.ItemsSource = TecnoMetal.GetMercadorias();
                this.combo_material.ItemsSource = TecnoMetal.GetMateriais();

                this.Escala = TecnoMetal.Getescala();

                combo_tipo_marca.ItemsSource = Conexoes.Utilz.GetLista_Enumeradores<Tipo_Bloco>().ToList().FindAll(x=> x!= Tipo_Bloco._ && x!= Tipo_Bloco.DUMMY);

                combo_tipo_marca.SelectedIndex = 0;

                this.combo_mercadoria.SelectedIndex = 0;
                this.combo_material.SelectedIndex = 0;

                this.DataContext = this;
                Update(tecnoMetal);
            }
            catch (Exception)
            {

            }


        }

        public int Sufix_Count { get; set; } = 1;
        public void Update(TecnoMetal tecnoMetal)
        {
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            var ms = MenuMarcas.TecnoMetal.GetMarcas(ref erros).ToList();
            var pos = ms.SelectMany(x => x.GetPosicoes()).ToList();

            MenuMarcas.TecnoMetal = tecnoMetal;
            this.seleciona_marca_composta.Visibility = Visibility.Visible;
            this.seleciona_marca_composta.ItemsSource = ms.FindAll(x=> x.Tipo_Marca == Tipo_Marca.MarcaComposta);

            this.Sufix_Count = ms.Count + pos.Count +1;

        

            if(this.seleciona_marca_composta.Items.Count>0 && this.seleciona_marca_composta.SelectedItem==null)
            {
                this.seleciona_marca_composta.SelectedIndex = 0;
            }
            else
            {
                this.seleciona_marca_composta.Visibility = Visibility.Collapsed;
            }



            SetTextos();


        }
        private void selecionar_perfil(object sender, RoutedEventArgs e)
        {

            perfil.Content = "...";
            this.Visibility = Visibility.Collapsed;
            switch (this.tipo)
            {

                case Tipo_Bloco.Chapa:
                    db_chapa = MenuMarcas.TecnoMetal.PromptChapa(Tipo_Chapa.Grossa);
                    db_bobina = Conexoes.Utilz.Clonar(Conexoes.DBases.GetBobinaDummy());
                    if (db_chapa != null)
                    {
                        perfil.Content = db_chapa.ToString();
                        db_bobina.Espessura = db_chapa.valor;
                        db_bobina.Material = this.db_material;
                    }
                    break;
                case Tipo_Bloco.Perfil:
                    db_perfil = Conexoes.Utilz.SelecionarPerfil();
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_M2:
                    this.Visibility = Visibility.Collapsed;
                    db_perfil_m2 = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetdbTecnoMetal().GetPerfis().FindAll(x => x.Tipo == DLMCam.TipoPerfil.Chapa_Xadrez), null, "Selecione");
                    if (db_perfil_m2 != null)
                    {
                        perfil.Content = db_perfil_m2.ToString();
                    }
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    db_unitario = Conexoes.Utilz.SelecionarObjeto(Conexoes.DBases.GetBancoRM().GetRMAs(), null, "Selecione");
                    if (db_unitario != null)
                    {
                        perfil.Content = db_unitario.ToString();
                    }
                    break;
                case Tipo_Bloco.Arremate:
                    db_chapa = TecnoMetal.PromptChapa(Tipo_Chapa.Fina);
                    if(db_chapa != null)
                    {
                        db_bobina = TecnoMetal.PromptBobina(db_chapa);
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
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            var ms = MenuMarcas.TecnoMetal.GetMarcas(ref erros).ToList();
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
                if(db_mercadoria==null)
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

           if(tipo == Tipo_Bloco.Elemento_Unitario && !NomeFim.EndsWith("_A"))
            {
                Conexoes.Utilz.Alerta($"Nome inválido: para elemento unitário deve sempre terminar com '_A'");
                return;
            }



            if (Conexoes.Utilz.CaracteresEspeciais(NomeFim) | NomeFim.Contains(" "))
            {
                Conexoes.Utilz.Alerta($"{NomeFim} - Nome não pode conter caracteres especiais ou espaços.");
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
                if(!Conexoes.Utilz.E_Multiplo(qtd_double,1))
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

                    var sel = Conexoes.Utilz.SelecionaCombo(new List<string> { "Sem Dobras", "Com Dobras" }, null);
                    if(sel == "Com Dobras")
                    {
                        this.combo_mercadoria.Text = "PERFIL DOBRADO";
                        MenuMarcas.TecnoMetal.InserirArremate(escala,nomeMarca, nomePos, (int)qtd_double, this.tratamento.Text, MenuMarcas.db_bobina, false, this.db_mercadoria);
                    }
                    else if(sel == "Sem Dobras")
                    {
                        MenuMarcas.TecnoMetal.InserirChapa(escala, nomeMarca, nomePos, this.db_material, (int)qtd_double, this.tratamento.Text, MenuMarcas.db_chapa, this.db_mercadoria);
                    }
                  
                    break;
                case Tipo_Bloco.Perfil:
                    MenuMarcas.TecnoMetal.InserirPerfil(escala, nomeMarca, nomePos, this.db_material, this.tratamento.Text, (int)qtd_double, MenuMarcas.db_perfil, this.db_mercadoria);
                    break;
                case Tipo_Bloco.Elemento_M2:
                    MenuMarcas.TecnoMetal.InserirElementoM2(escala, nomeMarca, nomePos,this.db_material,this.tratamento.Text, (int)qtd_double, MenuMarcas.db_perfil,this.db_mercadoria);
                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    MenuMarcas.TecnoMetal.InserirElementoUnitario(escala, nomeMarca, nomePos, qtd_double, this.db_mercadoria, MenuMarcas.db_unitario);
                    break;
                case Tipo_Bloco.Arremate:
                    var sel2 = Conexoes.Utilz.SelecionaCombo(new List<string> { "Corte", "Vista" }, null);
                    if (sel2 == "Corte")
                    {
                        MenuMarcas.TecnoMetal.InserirArremate(escala, nomeMarca, nomePos, (int)qtd_double, this.tratamento.Text, MenuMarcas.db_bobina, true, this.db_mercadoria);
                    }
                    else if (sel2 == "Vista")
                    {
                        MenuMarcas.TecnoMetal.InserirChapa(escala, nomeMarca, nomePos, this.db_material, (int)qtd_double, this.tratamento.Text, MenuMarcas.db_chapa, this.db_mercadoria, MenuMarcas.db_bobina);
                    }
                    break;
                case Tipo_Bloco._:
                    break;
            }

            if(this.Sufix_Count ==1)
            {
                FLayer.Desligar(Constantes.LayersMarcasDesligar);
            }

            this.Update(Comandos.TecnoMetal);
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
            
            var nova = MenuMarcas.TecnoMetal.InserirMarcaComposta(MenuMarcas.TecnoMetal.Getescala());
            if (nova != null)
            {
                this.Update(MenuMarcas.TecnoMetal);
                this.seleciona_marca_composta.SelectedItem = nova;
                this.tratamento.Text = nova.Tratamento;
      
            }
            this.Visibility = Visibility.Visible;

        }
        private void tipo_marca_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTextos();
        }

        private void SetTextos()
        {
            if (perfil == null) { return; }
            perfil.Content = "...";

            this.bt_criar.IsEnabled = true;

            this.sufix.Text = (Sufix_Count).ToString().PadLeft(2, '0');
            this.tratamento.Visibility = Visibility.Visible;
            this.combo_material.Visibility = Visibility.Visible;

            if((bool)rad_m_composta.IsChecked)
            {
                this.prefix.Text = "M";
            }
            else
            {
                this.prefix.Text = "P";
            }


            switch (tipo)
            {
                case Tipo_Bloco.Chapa:
                    if (db_chapa != null)
                    {
                        perfil.Content = db_chapa.ToString();
                    }
                    combo_mercadoria.Text = "CHAPA";
                    combo_material.Text = "CIVIL 350";
                    this.tratamento.Visibility = Visibility.Visible;
                    break;
                case Tipo_Bloco.Perfil:
                    if (db_perfil != null)
                    {
                        perfil.Content = db_perfil.ToString();
                    }
                    combo_material.Text = "CIVIL 350";
                    break;
                case Tipo_Bloco.Elemento_M2:
                    if (db_perfil_m2 != null)
                    {
                        perfil.Content = db_perfil_m2.ToString();

                    }
                    combo_mercadoria.Text = "CHAPA DE PISO";
                    combo_material.Text = "A572";

                    break;
                case Tipo_Bloco.Elemento_Unitario:
                    if (db_unitario != null)
                    {
                        perfil.Content = db_unitario.ToString();
                    }

                    if((bool)rad_m_simples.IsChecked)
                    {
                        bt_criar.IsEnabled = false;
                    }

                    combo_mercadoria.Text = "ALMOX";
                    combo_material.Text = "A325";
                    this.combo_material.Visibility = Visibility.Collapsed;
                    this.tratamento.Visibility = Visibility.Collapsed;
                    this.sufix.Text = (Sufix_Count).ToString().PadLeft(2, '0') + "_A";
                    break;
                case Tipo_Bloco.Arremate:
                    if (db_bobina != null)
                    {
                        perfil.Content = db_bobina.ToString();
                    }
                    combo_mercadoria.Text = "ARREMATE";
                    combo_material.Text = "PP ZINC";
                    this.tratamento.Visibility = Visibility.Collapsed;
                    break;
                case Tipo_Bloco._:
                    break;
            }
        }

 

        private void desliga_layer(object sender, RoutedEventArgs e)
        {

            FLayer.Desligar(Constantes.LayersMarcasDesligar);
            this.Visibility = Visibility.Collapsed;
        }

        private void liga_layer(object sender, RoutedEventArgs e)
        {
            FLayer.Ligar(Constantes.LayersMarcasDesligar);
            this.Visibility = Visibility.Collapsed;
        }

        private void insere_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            MenuMarcas.TecnoMetal.InserirTabela();
        }

        private void insere_tabela_auto(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            MenuMarcas.TecnoMetal.InserirTabelaAuto(ref erros);
        }

        private void gerar_dbf(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.gerardbf();
        }

        private void gerar_dbf_3d(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.gerardbf3d();
        }

        private void mercadorias(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.mercadorias();
        }

        private void materiais(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.materiais();
        }

        private void tratamentos(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            List<Conexoes.Report> erros = new List<Conexoes.Report>();
            Comandos.tratamentos();
        }

        private void quantificar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.quantificar();
        }

        private void purlin(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.purlin();
        }

        private void cotar(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.Cotar();
        }

        private void limpar_cotas(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.LimparCotas();
        }

        private void boneco(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.boneco();
        }

        private void passarelas(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.passarela();
        }

        private void passarelas_apaga(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.apagapassarela();
        }

        private void linha_de_vida(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.linhadevida();
        }

        private void linha_de_vida_apaga(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.apagalinhadevida();
        }

        private void linha_de_vida_alinha(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.alinharlinhadevida();
        }

        private void preenche_selo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.selopreenche();
        }

        private void limpa_selo(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.selolimpar();
        }

        private void criar_marcas_cam(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.criarmarcasdecam();
        }

        private void rodar_macro(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.rodarmacros();
        }

        private void gerar_dxf_cams(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.gerardxf();
        }

        private void marcar_montagem(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.marcarmontagem();
        }

        private void rad_m_simples_Checked(object sender, RoutedEventArgs e)
        {
            SetTextos();
        }

        private void gerar_pdf(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.gerarPDFEtapa();
        }

        private void composicao(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            MenuMarcas.TecnoMetal.InserirSoldaComposicao();
        }

        private void preenche_selo_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.preenche();
        }

        private void limpar_selo_tabela(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.limpa();
        }

        private void purlin_muda_perfil(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.mudaperfiltercas();
        }

        private void abre_pasta(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.abrepasta();
        }

        private void exporta_rma(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.exportarma();
        }

        private void importa_rm(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.importarm();
        }

        private void listar_quantidades(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.listarquantidadeblocos();
        }

        private void bloqueia_mviews(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.bloqueiamviews();
        }

        private void desloqueia_mviews(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.desbloqueiamviews();
        }

        private void cria_layers_padrao(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.criarlayersPadrao();
        }

        private void abre_versionamento(object sender, RoutedEventArgs e)
        {
            Constantes.Versionamento();
        }

        private void rodar_macros(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            Comandos.rodarmacros();
        }
    }
}