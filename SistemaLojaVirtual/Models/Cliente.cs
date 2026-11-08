using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    public abstract class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }

        public Endereco? Endereco { get; set; }
        public List<Pedido> Pedidos { get; set; }

        public abstract string Documento { get; }
    }
}
