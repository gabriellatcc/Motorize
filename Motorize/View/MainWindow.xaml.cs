using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Motorize.Services;
using Motorize.Models;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using K4os.Compression.LZ4.Internal;
using Mysqlx.Crud;
using System.Reflection.Metadata;
using System.Windows.Documents;

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
            CarregarCarrosDoBanco(); //Os veículos são carregados ao abrir o sistema
        
        }
        //teste de banco de dados
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
        //criar carro
        public void AdicionarCarro(Carro carro)
        {
            CarrosNaOficina.Add(carro);
            AtualizarListaDeCarros();
        }
        //atualizar a lista, caso o carro for adicionado
        public void AtualizarListaDeCarros()
        {
            CarContainerPanel.Children.Clear(); // Limpa os blocos antigos para evitar duplicações

            foreach (var carro in CarrosNaOficina)
            {
                var blocoCarro = CriarBlocoCarro(carro); //Garante que cada carro tenha seu bloco recriado
                CarContainerPanel.Children.Add(blocoCarro);
            }
        }
        //fechar tela
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        //leva para a tela que adiciona o carro no banco
        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            var novaJanela = new CarCheckIn(this);
            novaJanela.ShowDialog(); // Isso garante que a tela seja exibida

            if (novaJanela.NovoCarro != null)
            {
                AtualizarListaDeCarros(); // Apenas atualiza a interface
            }

        }
        //cria os blocos da tela principal
        private Border CriarBlocoCarro(Carro carro)
        {
            //Texto e cor da prioridade
            string prioridadeDescricao = carro.Prioridades switch
            {
                1 => "1 - Simples e rápido",
                2 => "2 - Simples, sem urgência",
                3 => "3 - Moderado e rápido",
                4 => "4 - Moderado, sem urgência",
                5 => "5 - Crítico e demorado",
                _ => "Não definida"
            };

            SolidColorBrush prioridadeCor = carro.Prioridades switch
            {
                1 or 2 => new SolidColorBrush(Color.FromRgb(76, 175, 80)),  // Verde
                3 or 4 => new SolidColorBrush(Color.FromRgb(255, 152, 0)),  // Laranja
                5 => new SolidColorBrush(Color.FromRgb(244, 67, 54)),       // Vermelho
                _ => new SolidColorBrush(Color.FromRgb(150, 150, 150))      // Cinza
            };

            var carContainer = new Border
            {
                BorderThickness = new Thickness(2),
                Width = 220,
                Height = 250,
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(12),
                Background = Brushes.White,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    ShadowDepth = 5,
                    BlurRadius = 8,
                    Opacity = 0.4
                }
            };

            var panel = new StackPanel
            {
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center
            };

            //Prioridade fixa + descrição colorida
            panel.Children.Add(new TextBlock
            {
                Text = "Prioridade:",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            panel.Children.Add(new TextBlock
            {
                Text = prioridadeDescricao,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = prioridadeCor,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            });

            // Imagem do carro
            panel.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri("C:/Users/livia/OneDrive/Documentos/B - Faculdade/Terceiro Semestre/POO-Carol/Carrinho.png")),
                Width = 120,
                Height = 100,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Informações com prefixo em negrito
            panel.Children.Add(new TextBlock
            {
                Inlines = {
            new Run("Marca: ") { FontWeight = FontWeights.Bold },
            new Run(carro.Marca)
        },
                FontSize = 12
            });

            panel.Children.Add(new TextBlock
            {
                Inlines = {
            new Run("Modelo: ") { FontWeight = FontWeights.Bold },
            new Run(carro.Modelo)
        },
                FontSize = 12
            });

            panel.Children.Add(new TextBlock
            {
                Inlines = {
            new Run("Placa: ") { FontWeight = FontWeights.Bold },
            new Run(carro.Placa)
        },
                FontSize = 12
            });

            panel.Children.Add(new TextBlock
            {
                Inlines = {
            new Run("Dono: ") { FontWeight = FontWeights.Bold },
            new Run(carro.NomeProprietario)
        },
                FontSize = 12
            });

            // Botões
            StackPanel botoesAcoes = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button detalhesButton = new Button
            {
                Content = "Detalhes",
                Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Verde
                Foreground = Brushes.White,
                FontSize = 12,
                Width = 100,
                Height = 30,
                BorderThickness = new Thickness(0)
            };

            Button proximaFaseButton = new Button
            {
                Content = "Avançar",
                Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Azul
                Foreground = Brushes.White,
                FontSize = 12,
                Width = 100,
                Height = 30,
                BorderThickness = new Thickness(0)
            };

            detalhesButton.Click += (sender, e) => AbrirDetalhesDoCarro(carro);
            proximaFaseButton.Click += (sender, e) => AvancarParaProximaFase(carro);

            botoesAcoes.Children.Add(detalhesButton);
            botoesAcoes.Children.Add(proximaFaseButton);

            panel.Children.Add(botoesAcoes);
            carContainer.Child = panel;

            return carContainer;
        }


        //ação para abrir a tela de detalhes
        private void AbrirDetalhesDoCarro(Carro carro)
        {
            try
            {
                var detalhesWindow = new Detalhes(this, carro.Id);
                detalhesWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir detalhes: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //ação de abrir a tela de manutenções
        private void AvancarParaProximaFase(Carro carro)
        {
            try
            {
                var manutencaoWindow = new ManutencaoWindow(carro.Id);
                manutencaoWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir detalhes: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //abrir tela de carros para alterar e excluir através do menu
        private void AbrirTelaCarro(object sender, RoutedEventArgs e)
        {
            try
            {
                var carrosWindow = new CarrosWindow(this); //Passa referência da MainWindow
                carrosWindow.ShowDialog(); //Usa ShowDialog para esperar fechar
                CarregarCarrosDoBanco();   //Recarrega após fechar a janela
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir a tela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //minimiza a tela
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        //abre a tela principal no menu, ao clicar em home
        private void AbrirHome(object sender, RoutedEventArgs e)
        {
            try
            {
                var homeWindow = new MainWindow();
                homeWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir a tela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //abre a tela de gerar relatório no menu
        private void TelaRelatorio(object sender, RoutedEventArgs e)
        {
            try
            {
                var relatorioWindow = new Relatorio();
                relatorioWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir a tela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //carrega os carros do banco
        private void CarregarCarrosDoBanco()
        {
            try
            {
                using var conn = new DatabaseService().GetConnection();
                conn.Open();

                string query = @"
            SELECT c.id, c.marca, c.modelo, c.placa, c.nome_proprietario, c.contato, c.cpf, c.ano_fabricacao, c.cor, m.prioridades 
            FROM carros c
            INNER JOIN manutencoes m ON c.id = m.carro_id"; // Busca a prioridade corretamente

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
                        Cor = reader.GetString("cor"),
                        Prioridades = reader["prioridades"] != DBNull.Value ? reader.GetInt32("prioridades") : 1
                    };

                    CarrosNaOficina.Add(carro);
                }

                AtualizarListaDeCarros(); // Atualiza os blocos na interface
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar carros: {ex.Message}", "Erro no Banco de Dados", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}

