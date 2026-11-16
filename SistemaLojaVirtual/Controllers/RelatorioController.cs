using Microsoft.EntityFrameworkCore;
using SistemaLojaVirtual.Data;

namespace SistemaLojaVirtual.Controllers
{
    
    public class VendaPorCliente
    {
        public string Cliente { get; set; } = "";
        public string Email { get; set; } = "";
        public int QuantidadePedidos { get; set; }
        public decimal TotalGasto { get; set; }
    }

    public class ProdutoMaisVendido
    {
        public string Produto { get; set; } = "";
        public int QuantidadeVendida { get; set; }
        public decimal Faturamento { get; set; }
    }

    
    public class RelatorioController
    {
        private readonly IDbContextFactory<LojaContext> _contextFactory;

        public RelatorioController(IDbContextFactory<LojaContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        
        public async Task<List<VendaPorCliente>> VendasPorClienteAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            return await db.Database.SqlQuery<VendaPorCliente>($"""
                SELECT c.Nome                          AS Cliente,
                       c.Email                         AS Email,
                       ISNULL(v.QuantidadePedidos, 0)  AS QuantidadePedidos,
                       ISNULL(v.TotalGasto, 0)         AS TotalGasto
                FROM Clientes c
                LEFT JOIN (
                    SELECT p.ClienteId,
                           COUNT(DISTINCT p.Id)                AS QuantidadePedidos,
                           SUM(i.Quantidade * i.PrecoUnitario) AS TotalGasto
                    FROM Pedidos p
                    INNER JOIN ItemPedidos i ON i.PedidoId = p.Id
                    WHERE p.Pago = 1
                    GROUP BY p.ClienteId
                ) v ON v.ClienteId = c.Id
                ORDER BY TotalGasto DESC, c.Nome
                """).ToListAsync();
        }

        
        public async Task<List<ProdutoMaisVendido>> ProdutosMaisVendidosAsync(int quantidade)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            return await db.Database.SqlQuery<ProdutoMaisVendido>($"""
                SELECT TOP ({quantidade})
                       pr.Nome             AS Produto,
                       v.QuantidadeVendida AS QuantidadeVendida,
                       v.Faturamento       AS Faturamento
                FROM (
                    SELECT i.ProdutoId,
                           SUM(i.Quantidade)                   AS QuantidadeVendida,
                           SUM(i.Quantidade * i.PrecoUnitario) AS Faturamento
                    FROM ItemPedidos i
                    GROUP BY i.ProdutoId
                ) v
                INNER JOIN Produtos pr ON pr.Id = v.ProdutoId
                ORDER BY v.QuantidadeVendida DESC
                """).ToListAsync();
        }

        
        public async Task<int> ReajustarPrecosPorCategoriaAsync(int categoriaId, decimal percentual)
        {
            if (percentual <= -100)
                throw new InvalidOperationException("O percentual deve ser maior que -100%.");

            using var db = await _contextFactory.CreateDbContextAsync();

            return await db.Database.ExecuteSqlAsync($"""
                UPDATE Produtos
                SET Preco = ROUND(Preco * (1 + {percentual} / 100.0), 2)
                WHERE Id IN (SELECT cp.ProdutosId
                             FROM CategoriaProduto cp
                             WHERE cp.CategoriasId = {categoriaId})
                """);
        }
    }
}
