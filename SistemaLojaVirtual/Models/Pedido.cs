using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;


namespace SistemaLojaVirtual.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public bool Pago { get; set; }
        public DateTime Data { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public List<ItemPedido> Itens { get; set; } = new(); 
        public Pagamento? Pagamento { get; set; }
        public decimal Total => Itens.Sum(i => i.Subtotal);


    }
}
