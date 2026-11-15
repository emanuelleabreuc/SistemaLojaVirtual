using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Views
{
    public partial class PedidosView : UserControl, IAtualizavel
    {
        private static readonly CultureInfo PtBr = new("pt-BR");

        private readonly PedidoController _pedidoController;
        private readonly ClienteController _clienteController;
        private readonly ProdutoController _produtoController;

        private readonly ObservableCollection<ItemCarrinho> _carrinho = new();

        public PedidosView(PedidoController pedidoController,
                           ClienteController clienteController,
                           ProdutoController produtoController)
        {
            InitializeComponent();
            _pedidoController = pedidoController;
            _clienteController = clienteController;
            _produtoController = produtoController;
            gridCarrinho.ItemsSource = _carrinho;
        }

        public async Task AtualizarAsync()
        {
            cmbCliente.ItemsSource = await _clienteController.ListarAsync();

            // Mostra "Nome - R$ preço (estoque)" no combo
            var produtos = await _produtoController.ListarComEstoqueAsync();
            cmbProduto.ItemsSource = produtos.Select(p => new ProdutoOpcao(p)).ToList();

            gridPedidos.ItemsSource = await _pedidoController.ListarAsync();
        }

        private void AdicionarItem_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProduto.SelectedItem is not ProdutoOpcao opcao)
            {
                MessageBox.Show("Selecione um produto.");
                return;
            }
            if (!int.TryParse(txtQuantidade.Text, out var quantidade) || quantidade <= 0)
            {
                MessageBox.Show("Informe uma quantidade válida.");
                return;
            }

            var produto = opcao.Produto;
            var itemExistente = _carrinho.FirstOrDefault(i => i.ProdutoId == produto.Id);
            var quantidadeTotal = quantidade + (itemExistente?.Quantidade ?? 0);

            if (!produto.TemEstoque(quantidadeTotal))
            {
                MessageBox.Show($"Estoque insuficiente. Disponível: {produto.Estoque}.");
                return;
            }

            if (itemExistente is not null)
                _carrinho.Remove(itemExistente);

            _carrinho.Add(new ItemCarrinho(produto.Id, produto.Nome, quantidadeTotal, produto.Preco));
            txtQuantidade.Text = "1";
            AtualizarTotal();
        }

        private void RemoverItem_Click(object sender, RoutedEventArgs e)
        {
            if (gridCarrinho.SelectedItem is ItemCarrinho item)
            {
                _carrinho.Remove(item);
                AtualizarTotal();
            }
        }

        private void FormaPagamento_Checked(object sender, RoutedEventArgs e)
        {
            if (painelCartao is null || painelPix is null) return; // tela ainda carregando
            bool cartao = rbCartao.IsChecked == true;
            painelCartao.Visibility = cartao ? Visibility.Visible : Visibility.Collapsed;
            painelPix.Visibility = cartao ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void Finalizar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCliente.SelectedItem is not ClienteResumo cliente)
            {
                MessageBox.Show("Selecione um cliente.");
                return;
            }

            await Tela.ExecutarAsync(async () =>
            {
                var pagamento = CriarPagamento();
                var numero = await _pedidoController.FinalizarAsync(cliente.Id, _carrinho.ToList(), pagamento);

                MessageBox.Show($"Pedido nº {numero} finalizado com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _carrinho.Clear();
                AtualizarTotal();
                await AtualizarAsync();
            });
        }

        // Polimorfismo: a tela escolhe qual Pagamento criar; o controller só chama processar().
        private Pagamento CriarPagamento()
        {
            if (rbCartao.IsChecked == true)
                return new Cartao { Parcelas = Tela.LerInteiro(txtParcelas.Text, "Parcelas") };

            return new Pix { Chave = txtChavePix.Text.Trim() };
        }

        private void AtualizarTotal()
        {
            var total = _carrinho.Sum(i => i.Subtotal);
            lblTotal.Text = $"Total: {total.ToString("C", PtBr)}";
        }

        // Item do ComboBox de produtos
        private sealed record ProdutoOpcao(Produto Produto)
        {
            public override string ToString() =>
                $"{Produto.Nome} - {Produto.Preco.ToString("C", PtBr)} (estoque: {Produto.Estoque})";
        }
    }
}
