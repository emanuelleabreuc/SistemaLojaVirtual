using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Data;
using SistemaLojaVirtual.Views;
using System.Windows;

namespace SistemaLojaVirtual
{
    public partial class App : Application
    {
        private ServiceProvider? _servicos;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var colecao = new ServiceCollection();
            RegistrarServicos(colecao);
            _servicos = colecao.BuildServiceProvider();

            _servicos.GetRequiredService<MainWindow>().Show();
        }

        // DIP (SOLID): as classes não criam suas dependências com "new";
        // elas são registradas aqui e entregues pelo construtor.
        private static void RegistrarServicos(IServiceCollection servicos)
        {
            // Model / acesso a dados
            servicos.AddDbContextFactory<LojaContext>(options =>
                options.UseSqlServer(LojaContext.ConnectionStringPadrao));

            // Controllers
            servicos.AddTransient<ClienteController>();
            servicos.AddTransient<ProdutoController>();
            servicos.AddTransient<CategoriaController>();
            servicos.AddTransient<PedidoController>();
            servicos.AddTransient<RelatorioController>();

            // Views
            servicos.AddTransient<LojaView>();
            servicos.AddTransient<ClientesView>();
            servicos.AddTransient<ProdutosView>();
            servicos.AddTransient<CategoriasView>();
            servicos.AddTransient<PedidosView>();
            servicos.AddTransient<RelatoriosView>();
            servicos.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _servicos?.Dispose();
            base.OnExit(e);
        }
    }
}