using System.Windows;
using System.Windows.Controls;
using SistemaLojaVirtual.Views;

namespace SistemaLojaVirtual
{
    public partial class MainWindow : Window
    {
        // As telas chegam prontas pela injeção de dependência (ver App.xaml.cs)
        public MainWindow(PedidosView pedidosView,
                          ClientesView clientesView,
                          ProdutosView produtosView,
                          CategoriasView categoriasView,
                          RelatoriosView relatoriosView)
        {
            InitializeComponent();
            abaPedidos.Content = pedidosView;
            abaClientes.Content = clientesView;
            abaProdutos.Content = produtosView;
            abaCategorias.Content = categoriasView;
            abaRelatorios.Content = relatoriosView;

            Loaded += async (_, _) => await AtualizarAbaAtualAsync();
        }

        // Recarrega os dados sempre que o usuário troca de aba
        private async void Abas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignora o evento quando ele vem de um ComboBox/DataGrid dentro da aba
            if (!ReferenceEquals(e.OriginalSource, abas) || !IsLoaded) return;
            await AtualizarAbaAtualAsync();
        }

        private async Task AtualizarAbaAtualAsync()
        {
            if (abas.SelectedContent is IAtualizavel tela)
                await Tela.ExecutarAsync(tela.AtualizarAsync);
        }
    }
}
