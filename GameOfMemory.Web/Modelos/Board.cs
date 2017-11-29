using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfMemory.Web.Models
{
    public class Board
    {
		//Cria uma lista para armazenar as cartas.
        private List<Card> _pieces = new List<Card>();

		//Métodos de acesso as cartas
		public List<Card> Pieces
        {
            get { return _pieces; }
            set { _pieces = value; }
        }

		//Cria um 'tabuleiro' das cartas, percorrendo todas as 30 cartas armazenadas e atribuindo elas ao atributo Image
        public Board()
        {
            int imgIndex = 1;

            for (var i = 1; i <= 30; i++)
            {

				
                if(IsOdd(i))
                {
                    _pieces.Add(new Card {
                    Id = i,
                    Pair = i + 1,
                    Name = "card-" + i.ToString(),
                    Image = string.Format("/estilo/img/{0}.png", imgIndex), //atribui a carta do número atual do vetor ao nome 'card-()' e vai criando os nomes das cartas
					Question = string.Format("/estilo/img/question.png", imgIndex) //atribui a carta de interrogação que ficará no front inicial
					});

                }else{
                    _pieces.Add(new Card {
                    Id = i,
                    Pair = i-1,
                    Name = "card-" + i.ToString(),
                    Image = string.Format("/estilo/img/{0}.png", imgIndex),
					Question = string.Format("/estilo/img/question.png", imgIndex)
					});
                    imgIndex++;
                }

            }

            _pieces.Shuffle();
        }

		


		//Par ou ímpar, atribui o deck de cartas.
        private bool IsOdd(int i)
        {
            return (i % 2 != 0);
        }
    }
}