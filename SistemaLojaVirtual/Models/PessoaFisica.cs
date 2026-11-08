using SistemaLojaVirtual.Models;

namespace LojaVirtual.Models
{
    public class PessoaFisica : Cliente
    {
        public string Cpf { get; set; } = "";
        public override string Documento => Cpf;
    }
}