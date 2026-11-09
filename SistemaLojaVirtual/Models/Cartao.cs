using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    public abstract class Cartao : Pagamento
    {
        private const int MaximoParcelas = 12;
        public int Parcelas { get; set; } = 1;

        public override bool processar() => Parcelas >= 1 && Parcelas <= MaximoParcelas;

    }
}
