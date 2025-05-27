using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MySql.Data.MySqlClient;
using Motorize.Models;
using Motorize.Services;
using System.Windows.Media;

namespace Motorize.View
{
    public partial class Relatorio : Window
    {
        public List<Carro> Carros { get; set; }

        public Relatorio()
        {
            InitializeComponent();
            CarregarCarros();
        }
        //carregar dados dos carros na tabela
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
        //emitir relatório (modelo do relatório)
        private void EmitirRelatorio(object sender, RoutedEventArgs e)
        {
            if (CarrosDataGrid.SelectedItem is Carro carroSelecionado)
            {
                var doc = new FlowDocument();
                var para = new Paragraph();

              
                // Nome da empresa 
                para.Inlines.Add(new Bold(new Run("MOTORIZE\n"))
                {
                    Foreground = Brushes.DarkBlue,
                    FontSize = 28 
                });
                para.Inlines.Add(new Italic(new Run("A excelência na manutenção automotiva\n\n"))
                {
                    FontSize = 20
                });

                // Separação visual
                para.Inlines.Add(new Run("\n------------------------------------------------------\n\n"));

                // Informações do veículo com espaçamento adequado
                para.Inlines.Add(new Bold(new Run("🚗 Dados do Veículo\n\n"))
                {
                    FontSize = 22
                });
                para.Inlines.Add(new Run($"Marca: {carroSelecionado.Marca}\n\n"));
                para.Inlines.Add(new Run($"Modelo: {carroSelecionado.Modelo}\n\n"));
                para.Inlines.Add(new Run($"Placa: {carroSelecionado.Placa}\n\n"));
                para.Inlines.Add(new Run($"Proprietário: {carroSelecionado.NomeProprietario}\n\n"));
                para.Inlines.Add(new Run($"Ano de Fabricação: {carroSelecionado.AnoFabricacao}\n\n"));
                para.Inlines.Add(new Run($"Cor: {carroSelecionado.Cor}\n\n"));

                
                para.Inlines.Add(new Run("\n\n------------------------------------------------------\n\n"));

                // Informações de manutenção
                para.Inlines.Add(new Bold(new Run("🛠️ Detalhes da Manutenção\n\n"))
                {
                    FontSize = 22
                });
                para.Inlines.Add(new Run($"Observações: {carroSelecionado.Observacoes}\n\n"));
                para.Inlines.Add(new Run($"Motivo Principal: {carroSelecionado.MotivoPrincipal}\n\n"));
                para.Inlines.Add(new Run($"Problema Real: {carroSelecionado.ProblemaReal}\n\n"));
                para.Inlines.Add(new Run($"Funcionário Responsável: {carroSelecionado.FuncionarioResponsavel}\n\n"));
                para.Inlines.Add(new Run($"Valor do Serviço: {carroSelecionado.ValorServico:C}\n\n"));

                // Espaçamento para assinatura
                para.Inlines.Add(new Run("\n\n------------------------------------------------------\n\n"));
                para.Inlines.Add(new Run("\n\n_____________________________\n"));
                para.Inlines.Add(new Run("Assinatura - Proprietário do Veículo\n\n"));
                para.Inlines.Add(new Run("_____________________________\n"));
                para.Inlines.Add(new Run("Assinatura - Dono da Empresa\n"));

                doc.Blocks.Add(para);

                var pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    pd.PrintDocument(paginator, "Relatório do Carro - Motorize");
                }
            }
        }

        //minimizar tela
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //fechar tela
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}