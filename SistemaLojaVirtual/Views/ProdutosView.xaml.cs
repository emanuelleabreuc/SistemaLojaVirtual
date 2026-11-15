using System.Windows;
using System.Windows.Controls;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Views
{
    public partial class ProdutosView : UserControl, IAtualizavel
    {
        private readonly ProdutoController _produtoController;
        private readonly CategoriaController _categoriaController;

        public ProdutosView(ProdutoController produtoController, CategoriaController categoriaController)
        {
            InitializeComponent();
            _produtoController = produtoController;
            _categoriaController = categoriaController;
        }

        public async Task AtualizarAsync()
        {
            gridProdutos.ItemsSource = await _produtoController.ListarAsync();
            lstCategorias.ItemsSource = await _categoriaController.ListarAsync();
        }

        private async void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            await Tela.ExecutarAsync(async () =>
            {
                var preco = Tela.LerDecimal(txtPreco.Text, "Preço");
                var estoque = Tela.LerInteiro(txtEstoque.Text, "Estoque inicial");
                var categoriaIds = lstCategorias.SelectedItems.Cast<Categoria>().Select(c => c.Id);

                await _produtoController.CadastrarAsync(txtNome.Text, preco, estoque, categoriaIds);

                txtNome.Clear();
                txtPreco.Clear();
                txtEstoque.Text = "0";
                await AtualizarAsync();
            }, "Produto cadastrado com sucesso!");
        }

        private async void ReporEstoque_Click(object sender, RoutedEventArgs e)
        {
            if (gridProdutos.SelectedItem is not ProdutoResumo selecionado)
            {
                MessageBox.Show("Selecione um produto na tabela.");
                return;
            }

            await Tela.ExecutarAsync(async () =>
            {
                var quantidade = Tela.LerInteiro(txtReposicao.Text, "Repor estoque");
                await _produtoController.ReporEstoqueAsync(selecionado.Id, quantidade);
                txtReposicao.Clear();
                await AtualizarAsync();
            });
        }

        private async void Excluir_Click(object sender, RoutedEventArgs e)
        {
            if (gridProdutos.SelectedItem is not ProdutoResumo selecionado)
            {
                MessageBox.Show("Selecione um produto na tabela.");
                return;
            }
            if (!Tela.Confirmar($"Excluir o produto {selecionado.Nome}?")) return;

            await Tela.ExecutarAsync(async () =>
            {
                await _produtoController.ExcluirAsync(selecionado.Id);
                await AtualizarAsync();
            });
        }
    }
}
