using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Motorize.Services;
using Motorize.Models;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Motorize.View
{
    public partial class MainWindow : Window
    {
        public List<Carro> CarrosNaOficina { get; private set; } = new List<Carro>();

        public MainWindow()
        {
            InitializeComponent();
            TestarBanco();
            AtualizarListaDeCarros();
            CarregarCarrosDoBanco(); //Os veículos são carregados ao abrir o sistema!
        
        }

        private void TestarBanco()
        {
            try
            {
                using var conn = new DatabaseService().GetConnection();
                conn.Open();
                MessageBox.Show("Banco conectado com sucesso!", "Conexão OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar ao banco de dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AdicionarCarro(Carro carro)
        {
            CarrosNaOficina.Add(carro);
            AtualizarListaDeCarros();
        }

        public void AtualizarListaDeCarros()
        {
            CarContainerPanel.Children.Clear(); // Limpa os blocos antigos para evitar duplicações

            foreach (var carro in CarrosNaOficina)
            {
                var blocoCarro = CriarBlocoCarro(carro); // 🔥 Garante que cada carro tenha seu bloco recriado
                CarContainerPanel.Children.Add(blocoCarro);
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            var novaJanela = new CarCheckIn(this);
            novaJanela.ShowDialog(); // Isso garante que a tela seja exibida

            if (novaJanela.NovoCarro != null) // Se um carro foi adicionado
            {
                CarrosNaOficina.Add(novaJanela.NovoCarro);
                AtualizarListaDeCarros();
            }
        }
        private Border CriarBlocoCarro(Carro carro)
        {
            var carContainer = new Border
            {
                BorderThickness = new Thickness(2),
                Width = 250,
                Height = 300,
                Margin = new Thickness(15),
                CornerRadius = new CornerRadius(12),
                Background = System.Windows.Media.Brushes.White
            };

            var panel = new StackPanel { Margin = new Thickness(10) };

            //panel.Children.Add(new TextBlock { Text = $"Fase: {carro.Fase}", FontWeight = FontWeights.Bold });
            panel.Children.Add(new TextBlock { Text = $"Marca: {carro.Marca}" });
            panel.Children.Add(new TextBlock { Text = $"Modelo: {carro.Modelo}" });
            panel.Children.Add(new TextBlock { Text = $"Placa: {carro.Placa}" });
            panel.Children.Add(new TextBlock { Text = $"Dono: {carro.NomeProprietario}" });

            var openButton = new Button
            {
                Content = "Ver Detalhes",
                Margin = new Thickness(0, 10, 0, 0),
                FontWeight = FontWeights.Bold
            };

            panel.Children.Add(openButton);
            carContainer.Child = panel;

            return carContainer;
        }
        private void CarregarCarrosDoBanco()
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = "SELECT id, marca, modelo, placa, nome_proprietario, contato, cpf, ano_fabricacao, cor FROM carros";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            CarrosNaOficina.Clear(); // Limpa a lista antes de recarregar os dados

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
                    Cor = reader.GetString("cor")
                };

                CarrosNaOficina.Add(carro);
            }

            AtualizarListaDeCarros(); // 🚀 Recria os blocos na interface ao abrir o sistema!
        }

    }
}

