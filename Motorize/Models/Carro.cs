using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motorize.Models
{
    public class Carro
    {
        public int Id { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Placa { get; set; }
        public int AnoFabricacao { get; set; }
        public string Cor { get; set; }
        public string NomeProprietario { get; set; }
        public string Contato { get; set; }
        public string Cpf { get; set; }
    }
}


