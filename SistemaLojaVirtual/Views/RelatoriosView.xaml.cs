using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Views
{
    public partial class RelatoriosView : UserControl, IAtualizavel
    {
        private const int QuantidadeRanking = 10;

        private readonly RelatorioController _relatorioController;
        private readonly CategoriaController _categoriaController;

        public RelatoriosView(RelatorioController relatorioController, CategoriaController categoriaController)
        {
            InitializeComponent();
            _relatorioController = relatorioController;
            _categoriaController = categoriaController;
        }

        public async Task AtualizarAsync()
        {
            cmbCategoria.ItemsSource = await _categoriaController.ListarAsync();
        }

        private async void VendasPorCliente_Click(object sender, RoutedEventArgs e)
        {
            await Tela.ExecutarAsync(async () =>
            {
                lblRelatorio.Text = "Vendas por cliente";
                gridRelatorio.ItemsSource = await _relatorioController.VendasPorClienteAsync();
            });
        }

        private async void ProdutosMaisVendidos_Click(object sender, RoutedEventArgs e)
        {
            await Tela.ExecutarAsync(async () =>
            {
                lblRelatorio.Text = $"Top {QuantidadeRanking} produtos mais vendidos";
                gridRelatorio.ItemsSource = await _relatorioController.ProdutosMaisVendidosAsync(QuantidadeRanking);
            });
        }

        private async void Reajustar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCategoria.SelectedItem is not Categoria categoria)
            {
                MessageBox.Show("Selecione uma categoria.");
                return;
            }

            await Tela.ExecutarAsync(async () =>
            {
                var percentual = Tela.LerDecimal(txtPercentual.Text, "Percentual");
                if (!Tela.Confirmar($"Aplicar {percentual}% nos produtos da categoria {categoria.Nome}?")) return;

                int afetados = await _relatorioController.ReajustarPrecosPorCategoriaAsync(categoria.Id, percentual);
                txtPercentual.Clear();
                MessageBox.Show($"{afetados} produto(s) tiveram o preço atualizado.", "Reajuste",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        // Formata colunas de dinheiro como R$ nas tabelas geradas automaticamente
        private void GridRelatorio_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(decimal) && e.Column is DataGridTextColumn coluna)
            {
                coluna.Binding = new Binding(e.PropertyName)
                {
                    StringFormat = "C",
                    ConverterCulture = new CultureInfo("pt-BR")
                };
            }
        }
    }
}
