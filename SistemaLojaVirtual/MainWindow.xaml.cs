using SistemaLojaVirtual.Views;
using System.Windows;
using System.Windows.Controls;

namespace SistemaLojaVirtual
{
    public partial class MainWindow : Window
    {
        // Liga cada item do menu à sua tela
        private readonly Dictionary<RadioButton, UserControl> _paginas;

        // As telas chegam prontas pela injeção de dependência (ver App.xaml.cs)
        public MainWindow(LojaView lojaView,
                          PedidosView pedidosView,
                          ProdutosView produtosView,
                          CategoriasView categoriasView,
                          ClientesView clientesView,
                          RelatoriosView relatoriosView)
        {
            InitializeComponent();

            _paginas = new Dictionary<RadioButton, UserControl>
            {
                [menuLoja] = lojaView,
                [menuPedidos] = pedidosView,
                [menuProdutos] = produtosView,
                [menuCategorias] = categoriasView,
                [menuClientes] = clientesView,
                [menuRelatorios] = relatoriosView
            };

            // Atualiza o número no ícone do carrinho sempre que ele muda
            lojaView.CarrinhoAlterado += quantidade => lblQuantidadeCarrinho.Text = quantidade.ToString();

            Loaded += (_, _) => menuLoja.IsChecked = true; // abre na vitrine
        }

        private async void Menu_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton item || !_paginas.TryGetValue(item, out var pagina)) return;

            conteudo.Content = pagina;

            if (pagina is IAtualizavel tela)
                await Tela.ExecutarAsync(tela.AtualizarAsync);
        }

        private void Carrinho_Click(object sender, RoutedEventArgs e)
        {
            menuLoja.IsChecked = true;
        }
    }
}