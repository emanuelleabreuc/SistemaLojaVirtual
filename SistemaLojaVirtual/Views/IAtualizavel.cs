namespace SistemaLojaVirtual.Views
{
    // ISP: interface pequena, só com o que a MainWindow precisa saber das telas
    // (recarregar os dados quando a aba é aberta).
    public interface IAtualizavel
    {
        Task AtualizarAsync();
    }
}
