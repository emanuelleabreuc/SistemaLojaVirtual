using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaLojaVirtual.Models
{
    public class PessoaFisica : Cliente
    {
        public string Cpf { get; set; } = "";
        public override string Documento => Cpf;
    }
}
