using Microsoft.EntityFrameworkCore;
using SistemaLojaVirtual.Data;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Controllers
{
    // Item do "carrinho" montado na tela antes de finalizar o pedido
    public record ItemCarrinho(int ProdutoId, string Produto, int Quantidade, decimal PrecoUnitario)
    {
        public decimal Subtotal => Quantidade * PrecoUnitario;
    }

    
    public record PedidoResumo(int Id, DateTime Data, string Cliente, int QuantidadeItens, decimal Total, string FormaPagamento, bool Pago);

    public class PedidoController
    {
        private readonly IDbContextFactory<LojaContext> _contextFactory;

        public PedidoController(IDbContextFactory<LojaContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

       
        public async Task<int> FinalizarAsync(int clienteId, IReadOnlyCollection<ItemCarrinho> itens, Pagamento pagamento)
        {
            if (itens.Count == 0)
                throw new InvalidOperationException("Adicione pelo menos um item ao pedido.");

            using var db = await _contextFactory.CreateDbContextAsync();

            var cliente = await db.Clientes.FindAsync(clienteId)
                ?? throw new InvalidOperationException("Selecione um cliente válido.");

            var pedido = new Pedido { Cliente = cliente, Data = DateTime.Now };

            foreach (var item in itens)
            {
                var produto = await db.Produtos.FindAsync(item.ProdutoId)
                    ?? throw new InvalidOperationException($"Produto {item.Produto} não encontrado.");

                produto.BaixarEstoque(item.Quantidade); // lança exceção se n tiver estoque

                pedido.Itens.Add(new ItemPedido
                {
                    Produto = produto,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = produto.Preco // preco do momento da venda
                });
            }

            pagamento.Valor = pedido.Total;
            if (!pagamento.Processar())
                throw new InvalidOperationException("Pagamento recusado. Verifique os dados do pagamento.");

            pedido.Pagamento = pagamento;
            pedido.Pago = true;

            db.Pedidos.Add(pedido);
            await db.SaveChangesAsync(); // sincroniza

            return pedido.Id;
        }

        public async Task<List<PedidoResumo>> ListarAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var pedidos = await db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .Include(p => p.Pagamento)
                .AsNoTracking()
                .OrderByDescending(p => p.Data)
                .ToListAsync();

            return pedidos.Select(p => new PedidoResumo(
                p.Id,
                p.Data,
                p.Cliente.Nome,
                p.Itens.Sum(i => i.Quantidade),
                p.Total,
                DescreverPagamento(p.Pagamento),
                p.Pago)).ToList();
        }

        private static string DescreverPagamento(Pagamento? pagamento) => pagamento switch
        {
            Cartao cartao => $"Cartão ({cartao.Parcelas}x)",
            Pix => "Pix",
            _ => "-"
        };
    }
}
