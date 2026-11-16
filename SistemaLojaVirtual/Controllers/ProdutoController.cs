using Microsoft.EntityFrameworkCore;
using SistemaLojaVirtual.Data;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Controllers
{
    
    public record ProdutoResumo(int Id, string Nome, decimal Preco, int Estoque, string Categorias);

    public class ProdutoController
    {
        private readonly IDbContextFactory<LojaContext> _contextFactory;

        public ProdutoController(IDbContextFactory<LojaContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ProdutoResumo>> ListarAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var produtos = await db.Produtos
                .Include(p => p.Categorias)
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync();

            return produtos.Select(p => new ProdutoResumo(
                p.Id,
                p.Nome,
                p.Preco,
                p.Estoque,
                string.Join(", ", p.Categorias.Select(c => c.Nome)))).ToList();
        }

        public async Task<List<Produto>> ListarComEstoqueAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Produtos.AsNoTracking()
                .Where(p => p.Estoque > 0)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task CadastrarAsync(string nome, decimal preco, int estoque, IEnumerable<int> categoriaIds)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidOperationException("Informe o nome do produto.");
            if (preco <= 0)
                throw new InvalidOperationException("O preço deve ser maior que zero.");
            if (estoque < 0)
                throw new InvalidOperationException("O estoque não pode ser negativo.");

            using var db = await _contextFactory.CreateDbContextAsync();

            var ids = categoriaIds.ToList();
            var categorias = await db.Categorias.Where(c => ids.Contains(c.Id)).ToListAsync();

            db.Produtos.Add(new Produto
            {
                Nome = nome.Trim(),
                Preco = preco,
                Estoque = estoque,
                Categorias = categorias
            });
            await db.SaveChangesAsync();
        }

        public async Task ReporEstoqueAsync(int produtoId, int quantidade)
        {
            if (quantidade <= 0)
                throw new InvalidOperationException("A quantidade de reposição deve ser maior que zero.");

            using var db = await _contextFactory.CreateDbContextAsync();
            var produto = await db.Produtos.FindAsync(produtoId)
                ?? throw new InvalidOperationException("Produto não encontrado.");

            produto.Estoque += quantidade;
            await db.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int produtoId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var produto = await db.Produtos.FindAsync(produtoId)
                ?? throw new InvalidOperationException("Produto não encontrado.");

            bool jaFoiVendido = await db.ItemPedidos.AnyAsync(i => i.ProdutoId == produtoId);
            if (jaFoiVendido)
                throw new InvalidOperationException("Não é possível excluir um produto que já foi vendido.");

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();
        }
    }
}
