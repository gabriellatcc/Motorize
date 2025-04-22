using System;
using System.Windows;
using Motorize.Services; // Certifique-se de que está importando corretamente

namespace Motorize.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Testar a conexão ao banco ao iniciar a interface
            TestarBanco();
        }

        private void TestarBanco()
        {
            var dbService = new DatabaseService();
            try
            {
                using (var conn = dbService.GetConnection())
                {
                    conn.Open();
                    MessageBox.Show("Banco conectado com sucesso!", "Conexão OK", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar ao banco de dados: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}