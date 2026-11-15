using System.Windows;
using System.Windows.Controls;
using SistemaLojaVirtual.Controllers;
using SistemaLojaVirtual.Models;

namespace SistemaLojaVirtual.Views
{
    // View: só lê os campos da tela e chama o controller. Nenhuma regra de negócio aqui.
    public partial class ClientesView : UserControl, IAtualizavel
    {
        private readonly ClienteController _controller;

        public ClientesView(ClienteController controller)
        {
            InitializeComponent();
            _controller = controller;
        }

        public async Task AtualizarAsync()
        {
            gridClientes.ItemsSource = await _controller.ListarAsync();
        }

        private void TipoCliente_Checked(object sender, RoutedEventArgs e)
        {
            if (lblDocumento is null) return; // evento disparado antes da tela terminar de carregar
            lblDocumento.Text = rbPessoaFisica.IsChecked == true ? "CPF" : "CNPJ";
        }

        private async void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            await Tela.ExecutarAsync(async () =>
            {
                Cliente cliente = rbPessoaFisica.IsChecked == true
                    ? new PessoaFisica { Cpf = txtDocumento.Text.Trim() }
                    : new PessoaJuridica { Cnpj = txtDocumento.Text.Trim() };

                cliente.Nome = txtNome.Text.Trim();
                cliente.Email = txtEmail.Text.Trim();

                var endereco = new Endereco { Rua = txtRua.Text.Trim(), Cidade = txtCidade.Text.Trim() };

                await _controller.CadastrarAsync(cliente, endereco);
                LimparCampos();
                await AtualizarAsync();
            }, "Cliente cadastrado com sucesso!");
        }

        private async void Excluir_Click(object sender, RoutedEventArgs e)
        {
            if (gridClientes.SelectedItem is not ClienteResumo selecionado)
            {
                MessageBox.Show("Selecione um cliente na tabela.");
                return;
            }
            if (!Tela.Confirmar($"Excluir o cliente {selecionado.Nome}?")) return;

            await Tela.ExecutarAsync(async () =>
            {
                await _controller.ExcluirAsync(selecionado.Id);
                await AtualizarAsync();
            });
        }

        private void LimparCampos()
        {
            txtNome.Clear();
            txtEmail.Clear();
            txtDocumento.Clear();
            txtRua.Clear();
            txtCidade.Clear();
        }
    }
}
