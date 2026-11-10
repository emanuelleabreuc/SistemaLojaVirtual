using SistemaLojaVirtual.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemaLojaVirtual.Data
{
    public class LojaContext : DbContext
    { 
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<PessoaFisica> PessoasFisicas { get; set; } = null!;
        public DbSet<PessoaJuridica> PessoasJuridicas { get; set; } = null!;
        public DbSet<Produto> Produtos { get; set; } = null!;
        public DbSet<Pagamento> Pagamentos { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Endereco> Enderecos { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<ItemPedido> ItemPedidos { get; set; } = null!;


        



    }
}