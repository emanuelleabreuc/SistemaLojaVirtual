using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    public class Pix : Pagamento
    {
        public string Chave { get; set; } = "";
        public override bool processar() => !string.IsNullOrEmpty(Chave);
    }
}
