using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    public class PessoaJuridica : Cliente
    {
        public string Cnpj { get; set; } = "";
        public override string Documento => Cnpj;
    }
}
