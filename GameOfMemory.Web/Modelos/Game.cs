using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfMemory.Web.Models
{
    public class Game
    {

      public Player Jogador1 { get; set; }
      public Player Jogador2 { get; set; }

      public Board Board { get; set; }
		
	  //Cria uma tributo do tipo Carta que armazena a última carta 	
      public Card LastCard { get; set; }

      public string Vez { get; set; }
    }
}