using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Motorize.Models;
using Motorize.Services;
using Motorize.View;
using MySql.Data.MySqlClient;

namespace Motorize.View
{
    public partial class Detalhes : Window
    {
        private readonly MainWindow mainWindow;

        public Detalhes(MainWindow window, int carroId)
        {
            InitializeComponent();
            mainWindow = window ?? throw new ArgumentNullException(nameof(window));
            CarregarDetalhesDoCarro(carroId);
        }

        private void CarregarDetalhesDoCarro(int carroId)
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = @"SELECT 
        c.id, 
        c.marca, 
        c.modelo, 
        c.placa, 
        c.nome_proprietario, 
        c.ano_fabricacao, 
        c.cor, 
        m.observacoes, 
        m.motivo_principal, 
        m.problema_real, 
        m.funcionario_responsavel, 
        m.Prioridades, 
        m.tempo_planejado, 
        m.trocas, 
        m.recursos_utilizados, 
        m.valor_servico
    FROM carros c
    INNER JOIN manutencoes m ON c.id = m.carro_id
    WHERE c.id = @Id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", carroId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                MarcaTextBlock.Text = "Marca: " + reader["marca"].ToString();
                ModeloTextBlock.Text = "Modelo: " + reader["modelo"].ToString();
                PlacaTextBlock.Text = "Placa: " + reader["placa"].ToString();
                ProprietarioTextBlock.Text = "Proprietário: " + reader["nome_proprietario"].ToString();
                ObservacoesTextBlock.Text = "Observações: " + reader["observacoes"].ToString();
                MotivoTextBlock.Text = "Motivo: " + reader["motivo_principal"].ToString();
                ProblemaTextBlock.Text = "Problema: " + reader["problema_real"].ToString();
                ResponsavelTextBlock.Text = "Responsável: " + reader["funcionario_responsavel"].ToString();

                PrioridadeTextBlock.Text = "Prioridade: " +
                    (!reader.IsDBNull(reader.GetOrdinal("Prioridades"))
                        ? reader["Prioridades"].ToString()
                        : "Indefinido");

                TempoPlanejadoTextBlock.Text = "Tempo planejado: " + reader["tempo_planejado"].ToString();
                TrocasTextBlock.Text = "Trocas: " + reader["trocas"].ToString();
                RecursosTextBlock.Text = "Recursos: " + reader["recursos_utilizados"].ToString();

                ValorServicoTextBlock.Text = "Valor do serviço: " +
                    (!reader.IsDBNull(reader.GetOrdinal("valor_servico"))
                        ? Convert.ToDecimal(reader["valor_servico"]).ToString("C")
                        : "Indisponível");

            }
        }


        private void VoltarParaCarros_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                var carrosPage = new CarrosWindow(mainWindow); // 🔹 Passando o argumento necessário
                carrosPage.Show();
                this.Close();
            }
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}