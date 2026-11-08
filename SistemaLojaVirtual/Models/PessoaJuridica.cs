using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    class PessoaJuridica
    {
        public string Cnpj { get; set; } = "";
        public override string Documento => Cnpj;
    }
}
