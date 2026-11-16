using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Views
{
    // Tela da loja: vitrine de produtos + carrinho + finalização da compra.
    // Continua sendo só View: as regras (estoque, pagamento, gravação) ficam nos controllers.
    public partial class LojaView : UserControl, IAtualizavel
    {
        private const int MaximoParcelas = 12;
        private static readonly CultureInfo PtBr = new("pt-BR");
        private static readonly Categoria TodasAsCategorias = new() { Id = 0, Nome = "Todas" };

        private readonly ProdutoController _produtoController;
        private readonly CategoriaController _categoriaController;
        private readonly ClienteController _clienteController;
        private readonly PedidoController _pedidoController;

        private readonly ObservableCollection<ItemCarrinho> _carrinho = new();
        private List<Produto> _produtos = new();

        // Avisa a janela principal para atualizar o número no ícone do carrinho
        public event Action<int>? CarrinhoAlterado;

        public LojaView(ProdutoController produtoController,
                        CategoriaController categoriaController,
                        ClienteController clienteController,
                        PedidoController pedidoController)
        {
            InitializeComponent();
            _produtoController = produtoController;
            _categoriaController = categoriaController;
            _clienteController = clienteController;
            _pedidoController = pedidoController;

            itensCarrinho.ItemsSource = _carrinho;
            AtualizarResumoCarrinho();
        }

        public async Task AtualizarAsync()
        {
            _produtos = await _produtoController.ListarVitrineAsync();

            var categoriaSelecionada = (lstCategorias.SelectedItem as Categoria)?.Id ?? 0;
            var categorias = new List<Categoria> { TodasAsCategorias };
            categorias.AddRange(await _categoriaController.ListarAsync());
            lstCategorias.ItemsSource = categorias;
            lstCategorias.SelectedItem = categorias.FirstOrDefault(c => c.Id == categoriaSelecionada) ?? TodasAsCategorias;

            var clienteSelecionado = (cmbCliente.SelectedItem as ClienteResumo)?.Id;
            var clientes = await _clienteController.ListarAsync();
            cmbCliente.ItemsSource = clientes;
            cmbCliente.SelectedItem = clientes.FirstOrDefault(c => c.Id == clienteSelecionado);

            AplicarFiltro();
        }

        // ===================== Vitrine =====================

        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            lblDicaBusca.Visibility = string.IsNullOrEmpty(txtBusca.Text) ? Visibility.Visible : Visibility.Collapsed;
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            var categoriaId = (lstCategorias.SelectedItem as Categoria)?.Id ?? 0;
            var busca = txtBusca.Text.Trim();

            var cards = _produtos
                .Where(p => categoriaId == 0 || p.Categorias.Any(c => c.Id == categoriaId))
                .Where(p => busca.Length == 0 || p.Nome.Contains(busca, StringComparison.OrdinalIgnoreCase))
                .Select(p => new ProdutoCard(p))
                .ToList();

            itensVitrine.ItemsSource = cards;
            lblQuantidadeProdutos.Text = cards.Count == 1 ? "1 produto" : $"{cards.Count} produtos";
            lblVitrineVazia.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AdicionarAoCarrinho_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ProdutoCard card })
                AlterarQuantidade(card.Produto.Id, +1);
        }

        // ===================== Carrinho =====================

        private void Aumentar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ItemCarrinho item }) AlterarQuantidade(item.ProdutoId, +1);
        }

        private void Diminuir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ItemCarrinho item }) AlterarQuantidade(item.ProdutoId, -1);
        }

        private void RemoverItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ItemCarrinho item })
            {
                _carrinho.Remove(item);
                AtualizarResumoCarrinho();
            }
        }

        private void AlterarQuantidade(int produtoId, int variacao)
        {
            var produto = _produtos.FirstOrDefault(p => p.Id == produtoId);
            if (produto is null) return;

            var itemAtual = _carrinho.FirstOrDefault(i => i.ProdutoId == produtoId);
            var novaQuantidade = (itemAtual?.Quantidade ?? 0) + variacao;

            if (!produto.TemEstoque(novaQuantidade))
            {
                MessageBox.Show($"Só temos {produto.Estoque} unidade(s) de {produto.Nome} em estoque.",
                    "Estoque", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var novoItem = new ItemCarrinho(produto.Id, produto.Nome, novaQuantidade, produto.Preco);

            if (itemAtual is null)
                _carrinho.Add(novoItem);
            else if (novaQuantidade <= 0)
                _carrinho.Remove(itemAtual);
            else
                _carrinho[_carrinho.IndexOf(itemAtual)] = novoItem; // mantém a posição na lista

            AtualizarResumoCarrinho();
        }

        private void AtualizarResumoCarrinho()
        {
            var total = _carrinho.Sum(i => i.Subtotal);
            var quantidadeItens = _carrinho.Sum(i => i.Quantidade);

            lblTotal.Text = total.ToString("C", PtBr);
            lblItensCarrinho.Text = quantidadeItens == 1 ? "(1 item)" : $"({quantidadeItens} itens)";
            painelCarrinhoVazio.Visibility = _carrinho.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            btnFinalizar.IsEnabled = _carrinho.Count > 0;

            AtualizarOpcoesParcelamento(total);
            CarrinhoAlterado?.Invoke(quantidadeItens);
        }

        // ===================== Pagamento =====================

        private void AtualizarOpcoesParcelamento(decimal total)
        {
            var parcelasEscolhidas = (cmbParcelas.SelectedItem as OpcaoParcela)?.Numero ?? 1;

            var opcoes = Enumerable.Range(1, MaximoParcelas)
                .Select(n => new OpcaoParcela(n, $"{n}x de {(total / n).ToString("C", PtBr)} sem juros"))
                .ToList();

            cmbParcelas.ItemsSource = opcoes;
            cmbParcelas.SelectedIndex = parcelasEscolhidas - 1;
        }

        private void FormaPagamento_Checked(object sender, RoutedEventArgs e)
        {
            if (cmbParcelas is null || painelPix is null) return; // tela ainda carregando
            bool cartao = rbCartao.IsChecked == true;
            cmbParcelas.Visibility = cartao ? Visibility.Visible : Visibility.Collapsed;
            painelPix.Visibility = cartao ? Visibility.Collapsed : Visibility.Visible;
        }

        // Polimorfismo: a tela escolhe qual Pagamento criar; o controller só chama Processar().
        private Pagamento CriarPagamento()
        {
            if (rbCartao.IsChecked == true)
                return new Cartao { Parcelas = (cmbParcelas.SelectedItem as OpcaoParcela)?.Numero ?? 1 };

            return new Pix { Chave = txtChavePix.Text.Trim() };
        }

        private async void Finalizar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCliente.SelectedItem is not ClienteResumo cliente)
            {
                MessageBox.Show("Selecione o cliente que está comprando.", "Cliente",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await Tela.ExecutarAsync(async () =>
            {
                var total = _carrinho.Sum(i => i.Subtotal);
                var numero = await _pedidoController.FinalizarAsync(cliente.Id, _carrinho.ToList(), CriarPagamento());

                MessageBox.Show($"Compra realizada com sucesso!\n\nPedido nº {numero}\nTotal: {total.ToString("C", PtBr)}",
                    "Obrigado pela compra!", MessageBoxButton.OK, MessageBoxImage.Information);

                _carrinho.Clear();
                txtChavePix.Clear();
                AtualizarResumoCarrinho();
                await AtualizarAsync(); // recarrega o estoque da vitrine
            });
        }

        // ===================== Classes de apoio da tela =====================

        public sealed record OpcaoParcela(int Numero, string Texto)
        {
            public override string ToString() => Texto;
        }

        // Dados já formatados para o card da vitrine
        public sealed class ProdutoCard
        {
            private static readonly string[] Cores =
                { "#6366F1", "#F97316", "#10B981", "#EC4899", "#0EA5E9", "#8B5CF6", "#EAB308", "#14B8A6" };

            public ProdutoCard(Produto produto) => Produto = produto;

            public Produto Produto { get; }
            public string Nome => Produto.Nome;
            public decimal Preco => Produto.Preco;
            public bool Disponivel => Produto.Estoque > 0;
            public string Inicial => Produto.Nome.Length > 0 ? Produto.Nome[..1].ToUpper() : "?";
            public Brush CorFundo => (Brush)new BrushConverter().ConvertFrom(Cores[Produto.Id % Cores.Length])!;
            public string CategoriasTexto => Produto.Categorias.Count == 0
                ? "Sem categoria"
                : string.Join(" • ", Produto.Categorias.Select(c => c.Nome));
            public string TextoParcelamento =>
                $"ou {MaximoParcelas}x de {(Produto.Preco / MaximoParcelas).ToString("C", PtBr)}";
            public string TextoEstoque => Disponivel ? $"{Produto.Estoque} em estoque" : "Indisponível";
            public Visibility VisibilidadeEsgotado => Disponivel ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}