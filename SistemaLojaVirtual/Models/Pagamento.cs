using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    public abstract class Pagamento : Ipagamento
    {
        public int Id { get; set; }
        public decimal Valor { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; } = null!;

        public abstract bool processar();
    }
}
