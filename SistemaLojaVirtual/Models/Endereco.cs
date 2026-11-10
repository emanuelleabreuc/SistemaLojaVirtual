using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Models
{
    public class Endereco
    {
        public int Id { get; set; }
        public string Rua { get; set; } = "";
        public string Cidade { get; set; } = "";

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
    }
}