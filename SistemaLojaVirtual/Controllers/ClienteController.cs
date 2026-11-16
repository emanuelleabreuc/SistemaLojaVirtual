using Microsoft.EntityFrameworkCore;
using SistemaLojaVirtual.Data;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Controllers
{
    
    public record ClienteResumo(int Id, string Nome, string Email, string Tipo, string Documento, string Cidade);

    
    public class ClienteController
    {
        private readonly IDbContextFactory<LojaContext> _contextFactory;

        public ClienteController(IDbContextFactory<LojaContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ClienteResumo>> ListarAsync()
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var clientes = await db.Clientes
                .Include(c => c.Endereco)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return clientes.Select(c => new ClienteResumo(
                c.Id,
                c.Nome,
                c.Email,
                c is PessoaFisica ? "Pessoa Física" : "Pessoa Jurídica",
                c.Documento,
                c.Endereco?.Cidade ?? "")).ToList();
        }

        public async Task CadastrarAsync(Cliente cliente, Endereco endereco)
        {
            ValidarCliente(cliente);

            using var db = await _contextFactory.CreateDbContextAsync();
            cliente.Endereco = endereco;
            db.Clientes.Add(cliente);
            await db.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int clienteId)
        {
            using var db = await _contextFactory.CreateDbContextAsync();

            var cliente = await db.Clientes.FindAsync(clienteId)
                ?? throw new InvalidOperationException("Cliente não encontrado.");

            bool possuiPedidos = await db.Pedidos.AnyAsync(p => p.ClienteId == clienteId);
            if (possuiPedidos)
                throw new InvalidOperationException("Não é possível excluir um cliente que possui pedidos.");

            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync();
        }

        private static void ValidarCliente(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nome))
                throw new InvalidOperationException("Informe o nome do cliente.");
            if (string.IsNullOrWhiteSpace(cliente.Email) || !cliente.Email.Contains('@'))
                throw new InvalidOperationException("Informe um e-mail válido.");
            if (string.IsNullOrWhiteSpace(cliente.Documento))
                throw new InvalidOperationException("Informe o CPF/CNPJ do cliente.");
        }
    }
}
