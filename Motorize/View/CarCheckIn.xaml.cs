using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Motorize.Models;
using Motorize.Services;
using MySql.Data.MySqlClient;

namespace Motorize.View
{
    public partial class CarCheckIn : Window
    {
        private readonly MainWindow mainWindow;

        // Adicionando a propriedade NovoCarro para evitar erro em MainWindow
        public Carro? NovoCarro { get; private set; }

        public CarCheckIn(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window ?? throw new ArgumentNullException(nameof(window));
        }

        //Botão de salvar dados
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mainWindow == null)
                {
                    MessageBox.Show("Erro ao referenciar a tela principal.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Função auxiliar para remover a máscara
                string RemoverMascara(string texto) => new string(texto?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());

                string placaDigitada = PlacaTextBox.Text;

                // Verificar se o carro já existe
                var carroExistente = BuscarCarroNoBanco(placaDigitada);
                if (carroExistente != null)
                {
                    MessageBox.Show("Este carro já está cadastrado! Não é necessário cadastrá-lo novamente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obter e validar CPF
                string cpfComMascara = CPFTextBox.Text;
                string cpfSemMascara = new string(cpfComMascara.Where(char.IsDigit).ToArray());

                if (string.IsNullOrWhiteSpace(cpfSemMascara))
                {
                    MessageBox.Show("Por favor, preencha o CPF do proprietário.", "CPF obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Obter e tirar mascara do telefone
                string contatoSemMascara = RemoverMascara(ContatoTextBox.Value?.ToString());

                // Prioridade selecionada
                int prioridadeSelecionada = PrioridadeComboBox.SelectedItem is ComboBoxItem selectedItem
                    ? int.TryParse(new string(selectedItem.Content.ToString().Where(char.IsDigit).ToArray()), out int prioridade) ? prioridade : 0
                    : 0;

                // Criar objeto Carro
                NovoCarro = new Carro
                {
                    Marca = MarcaTextBox.Text,
                    Modelo = ModeloTextBox.Text,
                    AnoFabricacao = int.TryParse(AnoTextBox.Text, out int ano) ? ano : 0,
                    Cor = CorTextBox.Text,
                    Placa = placaDigitada,
                    NomeProprietario = NomeProprietarioTextBox.Text,
                    Contato = contatoSemMascara,
                    Cpf = cpfSemMascara,
                    Prioridades = prioridadeSelecionada

                };

                int carroId = SalvarCarroNoBanco(NovoCarro);
                SalvarManutencaoNoBanco(carroId, ObservacoesTextBox.Text, MotivoPrincipalTextBox.Text, prioridadeSelecionada);

                // Atualiza a tela principal
                mainWindow.AdicionarCarro(NovoCarro);
                mainWindow.AtualizarListaDeCarros();

                MessageBox.Show("Carro incluído na oficina com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar carro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //Tirar máscara para salvar no banco de dados
        public static string RemoverMascara(string texto)
        {
            return new string(texto.Where(char.IsDigit).ToArray());
        }

        //Sair da tela
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Fecha a janela corretamente
        }

        //Minimizar tela
        private void MinimizarJanela_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //Fechar tela
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Faz com que o campo "Placa" seja sempre em maiusculo
        private void PlacaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var caretIndex = PlacaTextBox.CaretIndex; // Salva a posição do cursor
            string textoMaiusculo = PlacaTextBox.Text.ToUpper();

            if (PlacaTextBox.Text != textoMaiusculo)
            {
                PlacaTextBox.Text = textoMaiusculo;
                PlacaTextBox.CaretIndex = caretIndex; // Restaura o cursor
            }
        }

        //Verifica se carro já existe no banco
        private bool CarroJaExiste(string placa)
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = "SELECT COUNT(*) FROM carros WHERE placa = @Placa";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Placa", placa);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        //Salva novo carro no banco
        private int SalvarCarroNoBanco(Carro carro)
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = @"INSERT INTO carros (marca, modelo, ano_fabricacao, cor, placa, nome_proprietario, contato, cpf) 
                             VALUES (@Marca, @Modelo, @AnoFabricacao, @Cor, @Placa, @NomeProprietario, @Contato, @Cpf)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Marca", carro.Marca);
            cmd.Parameters.AddWithValue("@Modelo", carro.Modelo);
            cmd.Parameters.AddWithValue("@AnoFabricacao", carro.AnoFabricacao);
            cmd.Parameters.AddWithValue("@Cor", carro.Cor);
            cmd.Parameters.AddWithValue("@Placa", carro.Placa);
            cmd.Parameters.AddWithValue("@NomeProprietario", carro.NomeProprietario);
            cmd.Parameters.AddWithValue("@Contato", carro.Contato);
            cmd.Parameters.AddWithValue("@Cpf", carro.Cpf);

            cmd.ExecuteNonQuery();
            return (int)cmd.LastInsertedId;
        }
        //salva manutenção
        private void SalvarManutencaoNoBanco(int carroId, string observacoes, string motivoPrincipal, int prioridade)
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = @"INSERT INTO manutencoes (carro_id, observacoes, motivo_principal, Prioridades)  
                     VALUES (@CarroId, @Observacoes, @MotivoPrincipal, @Prioridade)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CarroId", carroId);
            cmd.Parameters.AddWithValue("@Observacoes", observacoes);
            cmd.Parameters.AddWithValue("@MotivoPrincipal", motivoPrincipal);
            cmd.Parameters.AddWithValue("@Prioridade", prioridade); // 🔥 Agora armazenamos a prioridade

            cmd.ExecuteNonQuery();
        }

        //Auxiliador do XAML
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Foreground == Brushes.Gray)
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }
        //Auxiliador que carrega os dados se já existir
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
                textBox.Text = textBox.Name switch
                {
                    "PlacaTextBox" => "Placa do carro",
                    "MarcaTextBox" => "Marca do carro",
                    "ModeloTextBox" => "Modelo do carro",
                    "AnoTextBox" => "Ano de fabricação",
                    "CorTextBox" => "Cor do carro",
                    "NomeProprietarioTextBox" => "Nome do proprietário",
                    "CPFTextBox" => "CPF",
                    "ContatoTextBox" => "Contato",
                    "ObservacoesTextBox" => "Observações",
                    "MotivoPrincipalTextBox" => "Motivo principal",
                    _ => ""
                };
            }
        }

        //buscar carro no banco
        private Carro? BuscarCarroNoBanco(string placa)
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = "SELECT * FROM carros WHERE placa = @Placa";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Placa", placa);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Carro
                {
                    Id = reader.GetInt32("id"),
                    Marca = reader.GetString("marca"),
                    Modelo = reader.GetString("modelo"),
                    AnoFabricacao = reader.GetInt32("ano_fabricacao"),
                    Cor = reader.GetString("cor"),
                    Placa = reader.GetString("placa"),
                    NomeProprietario = reader.GetString("nome_proprietario"),
                    Contato = reader.GetString("contato"),
                    Cpf = reader.GetString("cpf")
                };
            }

            return null; // Retorna NULL se o carro não existir
        }
        //Verificar se placa já existe no banco
        private void PlacaTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string placaDigitada = PlacaTextBox.Text.Trim();

            var carroExistente = BuscarCarroNoBanco(placaDigitada);
            if (carroExistente != null)
            {
                MarcaTextBox.Text = carroExistente.Marca;
                ModeloTextBox.Text = carroExistente.Modelo;
                AnoTextBox.Text = carroExistente.AnoFabricacao.ToString();
                CorTextBox.Text = carroExistente.Cor;
                NomeProprietarioTextBox.Text = carroExistente.NomeProprietario;
                ContatoTextBox.Text = carroExistente.Contato;
                CPFTextBox.Text = carroExistente.Cpf;

                MessageBox.Show("Este carro já está cadastrado! Dados preenchidos automaticamente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //Verificar se CPF já existe no banco
        private void CPFTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string cpfDigitado = CPFTextBox.Text.Trim();

            var carroExistente = BuscarCarroNoBancoPorCpf(cpfDigitado);
            if (carroExistente != null)
            {
                NomeProprietarioTextBox.Text = carroExistente.NomeProprietario;
                ContatoTextBox.Text = carroExistente.Contato;

                MessageBox.Show("Proprietário já cadastrado! Dados preenchidos automaticamente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        //auxiliador de prioridade
        private void PrioridadeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PrioridadeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int prioridade = int.Parse(selectedItem.Tag.ToString()); // Obtém apenas o número
            }
        }

        //Buscar carro no banco por CPF, para relaciona-lo
        private Carro? BuscarCarroNoBancoPorCpf(string cpf)
        {
            using var conn = new DatabaseService().GetConnection();
            conn.Open();

            string query = "SELECT * FROM carros WHERE cpf = @Cpf";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Cpf", cpf);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Carro
                {
                    Id = reader.GetInt32("id"),
                    Marca = reader.GetString("marca"),
                    Modelo = reader.GetString("modelo"),
                    AnoFabricacao = reader.GetInt32("ano_fabricacao"),
                    Cor = reader.GetString("cor"),
                    Placa = reader.GetString("placa"),
                    NomeProprietario = reader.GetString("nome_proprietario"),
                    Contato = reader.GetString("contato"),
                    Cpf = reader.GetString("cpf")
                };
            }

            return null; // Retorna NULL se o CPF não estiver no banco
        }
    }
}
