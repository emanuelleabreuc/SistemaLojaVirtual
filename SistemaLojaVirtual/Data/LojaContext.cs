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
        public DbSet<Cartao> PagamentosCartao { get; set; } = null!;   // herança TPH
        public DbSet<Pix> PagamentosPix { get; set; } = null!;         // herança TPH
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Endereco> Enderecos { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<ItemPedido> ItemPedidos { get; set; } = null!;

        public const string ConnectionStringPadrao =
            @"Server=(localdb)\mssqllocaldb;Database=LojaVirtualDb;Trusted_Connection=True;";

        // Usado pelo Add-Migration / Update-Database (tempo de design)
        public LojaContext() { }

        // Usado pela aplicação via injeção de dependência (App.xaml.cs)
        public LojaContext(DbContextOptions<LojaContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
                options.UseSqlServer(ConnectionStringPadrao);
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // 1x1 Cliente <-> Endereco
            mb.Entity<Cliente>()
                .HasOne(c => c.Endereco).WithOne(e => e.Cliente)
                .HasForeignKey<Endereco>(e => e.ClienteId);

            // 1x1 Pedido <-> Pagamento
            mb.Entity<Pedido>()
                .HasOne(p => p.pagamento).WithOne(pg => pg.Pedido)
                .HasForeignKey<Pagamento>(pg => pg.PedidoId);

            // 1xN Cliente -> Pedidos (não deixa apagar cliente com pedido)
            mb.Entity<Pedido>()
                .HasOne(p => p.Cliente).WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composição: apagou o pedido, apaga os itens
            mb.Entity<ItemPedido>()
                .HasOne(i => i.Pedido).WithMany(p => p.Itens)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Não deixa apagar produto que já foi vendido (preserva o histórico de pedidos)
            mb.Entity<ItemPedido>()
                .HasOne(i => i.Produto).WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            // NxN Produto <-> Categoria (o EF cria a tabela de junção sozinho)
            mb.Entity<Produto>()
                .HasMany(p => p.Categorias).WithMany(c => c.Produtos);

            // Dinheiro com 2 casas decimais
            mb.Entity<Produto>().Property(p => p.Preco).HasPrecision(10, 2);
            mb.Entity<ItemPedido>().Property(i => i.PrecoUnitario).HasPrecision(10, 2);
            mb.Entity<Pagamento>().Property(p => p.Valor).HasPrecision(10, 2);
        }




    }
}