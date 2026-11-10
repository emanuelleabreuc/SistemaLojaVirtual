namespace SistemaLojaVirtual.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }
        public int Estoque { get; set; }

        public List<Categoria> Categorias { get; set; } = new(); // NxN

        public bool TemEstoque(int quantidade) => Estoque >= quantidade;

        public void BaixarEstoque(int quantidade)
        {
            if (!TemEstoque(quantidade))
                throw new InvalidOperationException($"Estoque insuficiente de {Nome}.");
            Estoque -= quantidade;
        }
    }
}