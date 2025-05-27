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
using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;
using Motorize.Services;


namespace Motorize.View
{
    public partial class ManutencaoWindow : Window
    {
        private int carroId;

        public ManutencaoWindow(int idCarro)
        {
            InitializeComponent();
            carroId = idCarro;
            CarregarDadosFases();
        }
        //carregar dados se tiver alguma fase preenchida
        private void CarregarDadosFases()
        {
            try
            {
                using var conn = new DatabaseService().GetConnection();
                conn.Open();

                string query = "SELECT problema_real, funcionario_responsavel, tempo_planejado, trocas, recursos_utilizados, valor_servico " +
                               "FROM manutencoes WHERE carro_id = @Id";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", carroId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ProblemaRealTextBox.Text = reader["problema_real"] != DBNull.Value ? reader["problema_real"].ToString() : string.Empty;
                    FuncionarioResponsavelTextBox.Text = reader["funcionario_responsavel"] != DBNull.Value ? reader["funcionario_responsavel"].ToString() : string.Empty;
                    TempoPlanejadoTextBox.Text = reader["tempo_planejado"] != DBNull.Value ? reader["tempo_planejado"].ToString() : string.Empty;
                    TrocasTextBox.Text = reader["trocas"] != DBNull.Value ? reader["trocas"].ToString() : string.Empty;
                    RecursosUtilizadosTextBox.Text = reader["recursos_utilizados"] != DBNull.Value ? reader["recursos_utilizados"].ToString() : string.Empty;
                    ValorServicoTextBox.Text = reader["valor_servico"] != DBNull.Value ? reader["valor_servico"].ToString() : "0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar os dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //salvar os dados das novas fases
        private void AtualizarFase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var conn = new DatabaseService().GetConnection();
                conn.Open();

                string query = "UPDATE manutencoes SET problema_real = @ProblemaReal, funcionario_responsavel = @Funcionario, " +
                               "tempo_planejado = @Tempo, trocas = @Trocas, recursos_utilizados = @Recursos, valor_servico = @Valor " +
                               "WHERE carro_id = @Id";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProblemaReal", ProblemaRealTextBox.Text);
                cmd.Parameters.AddWithValue("@Funcionario", FuncionarioResponsavelTextBox.Text);
                cmd.Parameters.AddWithValue("@Tempo", TempoPlanejadoTextBox.Text);
                cmd.Parameters.AddWithValue("@Trocas", TrocasTextBox.Text);
                cmd.Parameters.AddWithValue("@Recursos", RecursosUtilizadosTextBox.Text);
                cmd.Parameters.AddWithValue("@Valor", decimal.TryParse(ValorServicoTextBox.Text, out var valor) ? valor : 0.00m);
                cmd.Parameters.AddWithValue("@Id", carroId);

                int linhasAfetadas = cmd.ExecuteNonQuery();

                
                MessageBox.Show("Fase da manutenção atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar a fase: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //minimizar tela
        private void MinimizarJanela_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //fechar tela
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}