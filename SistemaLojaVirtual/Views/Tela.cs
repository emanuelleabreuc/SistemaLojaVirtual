using System.Globalization;
using System.Windows;

namespace SistemaLojaVirtual.Views
{
    // Clean Code / DRY: tratamento de erro e conversões usados por todas as telas
    // ficam em um só lugar, em vez de repetir try/catch em cada botão.
    internal static class Tela
    {
        public static async Task ExecutarAsync(Func<Task> acao, string? mensagemSucesso = null)
        {
            try
            {
                await acao();
                if (mensagemSucesso is not null)
                    MessageBox.Show(mensagemSucesso, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (InvalidOperationException ex) // regra de negócio violada
            {
                MessageBox.Show(ex.Message, "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex) // erro inesperado (ex.: banco fora do ar)
            {
                var detalhe = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ocorreu um erro inesperado:\n{detalhe}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static bool Confirmar(string pergunta) =>
            MessageBox.Show(pergunta, "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public static decimal LerDecimal(string texto, string campo)
        {
            var normalizado = texto.Trim().Replace(',', '.');
            if (!decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
                throw new InvalidOperationException($"O campo \"{campo}\" deve ser um número.");
            return valor;
        }

        public static int LerInteiro(string texto, string campo)
        {
            if (!int.TryParse(texto.Trim(), out var valor))
                throw new InvalidOperationException($"O campo \"{campo}\" deve ser um número inteiro.");
            return valor;
        }
    }
}
