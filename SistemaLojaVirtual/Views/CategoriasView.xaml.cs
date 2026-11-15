using System.Windows;
using System.Windows.Controls;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Views
{
    public partial class CategoriasView : UserControl, IAtualizavel
    {
        private readonly CategoriaController _controller;

        public CategoriasView(CategoriaController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        public async Task AtualizarAsync()
        {
            gridCategorias.ItemsSource = await _controller.ListarAsync();
        }

        private async void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            await Tela.ExecutarAsync(async () =>
            {
                await _controller.CadastrarAsync(txtNome.Text);
                txtNome.Clear();
                await AtualizarAsync();
            }, "Categoria cadastrada com sucesso!");
        }

        private async void Excluir_Click(object sender, RoutedEventArgs e)
        {
            if (gridCategorias.SelectedItem is not Categoria selecionada)
            {
                MessageBox.Show("Selecione uma categoria na tabela.");
                return;
            }
            if (!Tela.Confirmar($"Excluir a categoria {selecionada.Nome}? Os produtos não serão apagados.")) return;

            await Tela.ExecutarAsync(async () =>
            {
                await _controller.ExcluirAsync(selecionada.Id);
                await AtualizarAsync();
            });
        }
    }
}
