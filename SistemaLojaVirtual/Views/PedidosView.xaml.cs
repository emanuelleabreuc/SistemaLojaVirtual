using System.Globalization;
using System.Windows.Controls;
using SistemaLojaVirtual.Controllers;

namespace SistemaLojaVirtual.Views
{
    // Histórico de pedidos. A compra em si é feita na LojaView.
    public partial class PedidosView : UserControl, IAtualizavel
    {
        private static readonly CultureInfo PtBr = new("pt-BR");
        private readonly PedidoController _pedidoController;

        public PedidosView(PedidoController pedidoController)
        {
            InitializeComponent();
            _pedidoController = pedidoController;
        }

        public async Task AtualizarAsync()
        {
            var pedidos = await _pedidoController.ListarAsync();
            gridPedidos.ItemsSource = pedidos;

            var faturamento = pedidos.Sum(p => p.Total);
            var ticketMedio = pedidos.Count == 0 ? 0 : faturamento / pedidos.Count;

            lblTotalPedidos.Text = pedidos.Count.ToString();
            lblFaturamento.Text = faturamento.ToString("C", PtBr);
            lblTicketMedio.Text = ticketMedio.ToString("C", PtBr);
        }
    }
}