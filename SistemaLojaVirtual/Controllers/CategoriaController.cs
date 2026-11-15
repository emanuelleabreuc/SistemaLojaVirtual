using Microsoft.EntityFrameworkCore;
using SistemaLojaVirtual.Data;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Controllers
{
    public class CategoriaController
    {
        private readonly IDbContextFactory<LojaContext> _contextFactory;

        public CategoriaController(IDbContextFactory<LojaContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Categoria>> ListarAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();
            return await db.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task CadastrarAsync(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidOperationException("Informe o nome da categoria.");

            using var db = await _contextFactory.CreateDbContextAsync();

            bool jaExiste = await db.Categorias.AnyAsync(c => c.Nome == nome.Trim());
            if (jaExiste)
                throw new InvalidOperationException("Já existe uma categoria com esse nome.");

            db.Categorias.Add(new Categoria { Nome = nome.Trim() });
            await db.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int categoriaId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var categoria = await db.Categorias.FindAsync(categoriaId)
                ?? throw new InvalidOperationException("Categoria não encontrada.");

            // Remove só o vínculo NxN; os produtos continuam existindo
            db.Categorias.Remove(categoria);
            await db.SaveChangesAsync();
        }
    }
}
