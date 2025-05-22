using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows;
using MySql.Data.MySqlClient;
using Motorize.Models;
using Motorize.Services;

namespace Motorize.View
{
    public partial class EditarCarroWindow : Window
    {
        private Carro carroAtual;

        public EditarCarroWindow(Carro carro)
        {
            InitializeComponent();
            carroAtual = carro;
            PreencherCampos();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PreencherCampos()
        {
            MarcaTextBox.Text = carroAtual.Marca ?? "Não informado";
            ModeloTextBox.Text = carroAtual.Modelo ?? "Não informado";
            PlacaTextBox.Text = carroAtual.Placa ?? "Não informado";
            NomeProprietarioTextBox.Text = carroAtual.NomeProprietario ?? "Não informado";
            PrioridadeTextBox.Text = carroAtual.Prioridades.ToString();
            ObservacoesTextBox.Text = carroAtual.Observacoes ?? "Não informado";
            MotivoPrincipalTextBox.Text = carroAtual.MotivoPrincipal ?? "Não informado";
            ProblemaRealTextBox.Text = carroAtual.ProblemaReal ?? "Não informado";
            TempoPlanejadoTextBox.Text = carroAtual.TempoPlanejado ?? "Não informado";
            RecursosUtilizadosTextBox.Text = carroAtual.RecursosUtilizados ?? "Não informado";
            ValorServicoTextBox.Text = carroAtual.ValorServico.ToString("0.00");
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        // Permitir arrastar a janela mesmo com WindowStyle=None
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void SalvarAlteracoes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var conn = new DatabaseService().GetConnection();
                conn.Open();

                // 1º UPDATE - Tabela carros
                string updateCarros = @"UPDATE carros 
                                SET marca = @Marca, modelo = @Modelo, placa = @Placa, nome_proprietario = @NomeProprietario 
                                WHERE id = @Id";

                using (var cmdCarros = new MySqlCommand(updateCarros, conn))
                {
                    cmdCarros.Parameters.AddWithValue("@Id", carroAtual.Id);
                    cmdCarros.Parameters.AddWithValue("@Marca", string.IsNullOrWhiteSpace(MarcaTextBox.Text) ? (object)DBNull.Value : MarcaTextBox.Text);
                    cmdCarros.Parameters.AddWithValue("@Modelo", string.IsNullOrWhiteSpace(ModeloTextBox.Text) ? (object)DBNull.Value : ModeloTextBox.Text);
                    cmdCarros.Parameters.AddWithValue("@Placa", string.IsNullOrWhiteSpace(PlacaTextBox.Text) ? (object)DBNull.Value : PlacaTextBox.Text);
                    cmdCarros.Parameters.AddWithValue("@NomeProprietario", string.IsNullOrWhiteSpace(NomeProprietarioTextBox.Text) ? (object)DBNull.Value : NomeProprietarioTextBox.Text);
                    cmdCarros.ExecuteNonQuery();
                }

                // 2º UPDATE - Tabela manutencoes
                string updateManutencoes = @"UPDATE manutencoes 
                                     SET observacoes = @Observacoes, motivo_principal = @MotivoPrincipal, problema_real = @ProblemaReal, 
                                         Prioridades = @Prioridade, tempo_planejado = @TempoPlanejado, recursos_utilizados = @RecursosUtilizados, 
                                         valor_servico = @ValorServico 
                                     WHERE carro_id = @Id";

                using (var cmdManut = new MySqlCommand(updateManutencoes, conn))
                {
                    cmdManut.Parameters.AddWithValue("@Id", carroAtual.Id);
                    cmdManut.Parameters.AddWithValue("@Observacoes", string.IsNullOrWhiteSpace(ObservacoesTextBox.Text) ? (object)DBNull.Value : ObservacoesTextBox.Text);
                    cmdManut.Parameters.AddWithValue("@MotivoPrincipal", string.IsNullOrWhiteSpace(MotivoPrincipalTextBox.Text) ? (object)DBNull.Value : MotivoPrincipalTextBox.Text);
                    cmdManut.Parameters.AddWithValue("@ProblemaReal", string.IsNullOrWhiteSpace(ProblemaRealTextBox.Text) ? (object)DBNull.Value : ProblemaRealTextBox.Text);
                    cmdManut.Parameters.AddWithValue("@TempoPlanejado", string.IsNullOrWhiteSpace(TempoPlanejadoTextBox.Text) ? (object)DBNull.Value : TempoPlanejadoTextBox.Text);
                    cmdManut.Parameters.AddWithValue("@RecursosUtilizados", string.IsNullOrWhiteSpace(RecursosUtilizadosTextBox.Text) ? (object)DBNull.Value : RecursosUtilizadosTextBox.Text);

                    // Trata os campos numéricos
                    int prioridade = int.TryParse(PrioridadeTextBox.Text, out var p) ? p : 1;
                    decimal valorServico = decimal.TryParse(ValorServicoTextBox.Text, out var v) ? v : 0.0m;

                    cmdManut.Parameters.AddWithValue("@Prioridade", prioridade);
                    cmdManut.Parameters.AddWithValue("@ValorServico", valorServico);

                    cmdManut.ExecuteNonQuery();
                }

                MessageBox.Show("Alterações salvas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                // Apenas feche a janela se essa for uma janela secundária (ex: edição)
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar alterações: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}

