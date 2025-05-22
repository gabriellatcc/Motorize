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
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using Motorize.Models;
using Motorize.Services;

namespace Motorize.View
{
    public partial class CarrosWindow : Window
    {
        public List<Carro> Carros { get; set; }
        private MainWindow _mainWindow; // <-- Adicionado campo privado para armazenar referência

        public CarrosWindow(MainWindow parent)
        {
            InitializeComponent();
            _mainWindow = parent; // <-- Correto
            CarregarCarros();
        }


        private void CarregarCarros()
        {
            Carros = new List<Carro>();

            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = @"SELECT c.id, 
                               COALESCE(c.marca, 'Não informado') AS marca, 
                               COALESCE(c.modelo, 'Não informado') AS modelo, 
                               COALESCE(c.placa, 'Não informado') AS placa, 
                               COALESCE(c.nome_proprietario, 'Não informado') AS nome_proprietario, 
                               COALESCE(c.ano_fabricacao, 0) AS ano_fabricacao, 
                               COALESCE(c.cor, 'Não informado') AS cor, 
                               COALESCE(m.observacoes, 'Não informado') AS observacoes, 
                               COALESCE(m.motivo_principal, 'Não informado') AS motivo_principal, 
                               COALESCE(m.problema_real, 'Não informado') AS problema_real, 
                               COALESCE(m.funcionario_responsavel, 'Não informado') AS funcionario_responsavel, 
                               COALESCE(m.Prioridades, 0) AS Prioridades, 
                               COALESCE(m.tempo_planejado, 'Não informado') AS tempo_planejado, 
                               COALESCE(m.trocas, 'Não informado') AS trocas, 
                               COALESCE(m.recursos_utilizados, 'Não informado') AS recursos_utilizados, 
                               COALESCE(m.valor_servico, 0.0) AS valor_servico
                        FROM carros c
                        LEFT JOIN manutencoes m ON c.id = m.carro_id";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Carros.Add(new Carro
                {
                    Id = reader.GetInt32("id"), 
                    Marca = reader["marca"].ToString(),
                    Modelo = reader["modelo"].ToString(),
                    Placa = reader["placa"].ToString(),
                    NomeProprietario = reader["nome_proprietario"].ToString(),
                    AnoFabricacao = reader.GetInt32("ano_fabricacao"),
                    Cor = reader["cor"].ToString(),
                    Observacoes = reader["observacoes"].ToString(),
                    MotivoPrincipal = reader["motivo_principal"].ToString(),
                    ProblemaReal = reader["problema_real"].ToString(),
                    FuncionarioResponsavel = reader["funcionario_responsavel"].ToString(),
                    Prioridades = reader.IsDBNull(reader.GetOrdinal("Prioridades")) ? 0 : reader.GetInt32(reader.GetOrdinal("Prioridades")),
                    TempoPlanejado = reader["tempo_planejado"].ToString(),
                    Trocas = reader["trocas"].ToString(),
                    RecursosUtilizados = reader["recursos_utilizados"].ToString(),
                    ValorServico = reader.GetDecimal("valor_servico")
                });

            }

            CarrosDataGrid.ItemsSource = Carros;
        }
        private void AlterarCarro_Click(object sender, RoutedEventArgs e)
        {
            if (CarrosDataGrid.SelectedItem is Carro carroSelecionado)
            {
                var editarcarroWindow = new EditarCarroWindow(carroSelecionado);
                editarcarroWindow.ShowDialog();

                AtualizarListaDeCarros(); // 🔥 Atualiza os dados na memória
                AtualizarDadosNosBlocos(); // 🔥 Atualiza os textos sem recriar os blocos
            }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private StackPanel BlocosCarrosPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10)
        };
        private void AtualizarDadosNosBlocos()
        {
            foreach (var bloco in BlocosCarrosPanel.Children.OfType<Border>()) // 🔹 Se "CarrosStackPanel" não existe, use "BlocosCarrosPanel"
            {
                var panel = bloco.Child as StackPanel;
                if (panel == null) continue;

                var idTextBlock = panel.Children.OfType<TextBlock>().FirstOrDefault(tb => tb.Text.StartsWith("ID: "));
                if (idTextBlock == null) continue;

                int idCarro = int.Parse(idTextBlock.Text.Replace("ID: ", ""));

                var carroAtualizado = Carros.FirstOrDefault(c => c.Id == idCarro);
                if (carroAtualizado == null) continue;

                // 🔹 Atualiza apenas os textos dentro dos blocos sem recriar
                foreach (var textBlock in panel.Children.OfType<TextBlock>())
                {
                    if (textBlock.Text.StartsWith("Marca: "))
                        textBlock.Text = $"Marca: {carroAtualizado.Marca}";

                    else if (textBlock.Text.StartsWith("Modelo: "))
                        textBlock.Text = $"Modelo: {carroAtualizado.Modelo}";

                    else if (textBlock.Text.StartsWith("Placa: "))
                        textBlock.Text = $"Placa: {carroAtualizado.Placa}";

                    else if (textBlock.Text.StartsWith("Dono: "))
                        textBlock.Text = $"Dono: {carroAtualizado.NomeProprietario}";
                }
            }
        }

        private void ExcluirCarro_Click(object sender, RoutedEventArgs e)
        {
            if (CarrosDataGrid.SelectedItem is Carro carroSelecionado)
            {
                var resultado = MessageBox.Show("Tem certeza que deseja excluir este carro?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    using var conn = new DatabaseService().GetConnection();
                    conn.Open();

                    using (var deleteCarro = new MySqlCommand("DELETE FROM carros WHERE id = @id", conn))
                    {
                        deleteCarro.Parameters.AddWithValue("@id", carroSelecionado.Id);
                        int afetadosCarro = deleteCarro.ExecuteNonQuery();

                        if (afetadosCarro > 0)
                        {
                            MessageBox.Show("Carro excluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                            AtualizarListaDeCarros();
                            AtualizarDadosNosBlocos(); // 🔥 Atualiza apenas os textos sem recriação
                        }
                        else
                        {
                            MessageBox.Show("Erro ao excluir o carro.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void AtualizarListaDeCarros()
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = @"SELECT c.id, c.marca, c.modelo, c.placa, c.nome_proprietario, 
                            c.contato, c.cpf, c.ano_fabricacao, c.cor, m.prioridades 
                     FROM carros c
                     INNER JOIN manutencoes m ON c.id = m.carro_id";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            Carros.Clear(); // 🔹 Limpa os dados antigos

            while (reader.Read())
            {
                var carro = new Carro
                {
                    Id = reader.GetInt32("id"),
                    Marca = reader.GetString("marca"),
                    Modelo = reader.GetString("modelo"),
                    Placa = reader.GetString("placa"),
                    NomeProprietario = reader.GetString("nome_proprietario"),
                    Contato = reader.GetString("contato"),
                    Cpf = reader.GetString("cpf"),
                    AnoFabricacao = reader.GetInt32("ano_fabricacao"),
                    Cor = reader.GetString("cor"),
                    Prioridades = reader["prioridades"] != DBNull.Value ? reader.GetInt32("prioridades") : 1
                };

                Carros.Add(carro);
            }

            CarrosDataGrid.ItemsSource = null; // 🔹 Força atualização da tabela
            CarrosDataGrid.ItemsSource = Carros;
            CarrosDataGrid.Items.Refresh();

            AtualizarDadosNosBlocos(); // 🔥 Atualiza os blocos sem recriá-los
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

    }

