using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfMemory.Web.Models
{
    public class Player
    {

        public Player(string nome, string hash)
        {
            this.Nome = nome;
            this.Hash = hash;

			//instancia o id com 32 bits e separados por hífen
			this.Id = Guid.NewGuid().ToString("d");

			//instancia a lista para armazenar as partidas..
			this.Combinacoes = new List<int>(); 
        }
        
        public string ConnectionId { get; set; }
        public string Nome { get; set; }
        public string Id { get; set; }
        public string Hash { get; set; }
        public string Grupo { get; set; }
        public bool EstaJogando { get; set; }

		//Lista para armazenar as combinações das cartas
        public List<int> Combinacoes { get; set; }
    }
}