using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using GameOfMemory.Web.Models;

namespace GameOfMemory.Web
{
    public class GameHub : Hub
    {
		//função que passa o nome + a mensagem no chat e distribui para todos os jogadores
		public void Enviar(string nome, string mensagem)
		{
			Clients.All.broadcastMessage(nome, mensagem);
		}
//----------------------------------------------------------------------------------------------------------------------------------------------------------------

					/*Tratamento do inicio do jogo, trata o registro de um novo jogador e oponente e criação de um novo jogo e*/

//----------------------------------------------------------------------------------------------------------------------------------------------------------------



		//Registrar um novo jogador...
		public bool Entrar(string nome)
			{
				var jogador = GameState.Instance.RetornaJogador(nome);

				if (jogador != null)
				{
					Clients.Caller.PlayerExists(); //Verifica se o jogador já está registrado..
					return true;
				}

				jogador = GameState.Instance.CriarJogador(nome); //cria um jogador passando o nome do parâmetro e pegando o id registrado do context

				jogador.ConnectionId = Context.ConnectionId; //O jogador recebe o id atribuido no Hub
				Clients.Caller.name = jogador.Nome; //O cliente armazena o nome cadastrado do jogador
				Clients.Caller.hash = jogador.Hash;
				Clients.Caller.id = jogador.Id;

				Clients.Caller.jogadorCadastrado(jogador);


				//Retorna a função que inicia o jogo
				return IniciarJogo(jogador);
			}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------



		//Função para iniciar o jogo começando pelo primeiro jogador conectado
		private bool IniciarJogo(Player Jogador)
        {
            if (Jogador != null)
            {
                Player Jogador2;

                var jogo = GameState.Instance.EncontrarPartida(Jogador, out Jogador2); //Cria uma variável que recebe o retorno da função de procura de uma nova partida

				
                if (jogo != null)
                {
                    Clients.Group(Jogador.Grupo).buildBoard(jogo); //Constrói o tabuleiro..
                    return true;
                }

				Jogador2 = GameState.Instance.RetornaNovoOponente(Jogador); //Um novo oponente é atribuido a um segundo jogador
                if (Jogador2 == null)
                {
                    Clients.Caller.espera(); //invoca o método de espera passado no Jquery caso ainda não aja um oponente
                    return true;
                }

				jogo = GameState.Instance.Criarjogo(Jogador, Jogador2); // cria uma variável jogo que recebe a instanciação da criaão de um novo jogo
				jogo.Vez = Jogador.Id; //Atribui ao Jogador a vez de jogar atual

                Clients.Group(Jogador.Grupo).buildBoard(jogo); //Atribui ao grupo do Hub o grupo atual cadastrado no jogador e constrói o tabuleiro passando o jogo como parâmetro.
                return true;
            }

			
            return false;
        }

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

												/*Aqui começa o tratamento da lógica do jogo */

//----------------------------------------------------------------------------------------------------------------------------------------------------------------


		public bool SelecionarCarta(string nomeCarta)
        {
            var nome = Clients.Caller.name;

            Player jogador = GameState.Instance.RetornaJogador(nome); //Instancia um jogador que retorna o nome do jogador cadastrado

			if (jogador != null)
            {
                Player jogadorOponente;

                var jogo = GameState.Instance.EncontrarPartida(jogador, out jogadorOponente); // cria uma variável que recebe a partida 


				if (jogo != null)
                {
					//Se o atributo vez está setado como nulo ou vazio e não é a vez do jogador
                    if (!string.IsNullOrEmpty(jogo.Vez) && jogo.Vez != jogador.Id)
                    {
                      Clients.Group(jogador.Grupo).NaoESuaVez(jogo); //retorna a função de não é sua vez no jquery para o jogo atual
                      return true;
                    }

                    var carta = EncontrarCarta(jogo, nomeCarta);
                    Clients.Group(jogador.Grupo).flipCard(carta); //passa para o grupo o Hub a carta selecionada..
                    return true;
                }
            }
            return false;
        }

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Método que retorna a carta com mesmo nome atribuido no Board.
		private Card EncontrarCarta(Game jogo, string nomeCarta)
        {
            return jogo.Board.Pieces.FirstOrDefault(c => c.Name == nomeCarta);
        }

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Função para verificar as cartas 
		public bool VerificaCarta(string cartaNome)
        {
            var nome = Clients.Caller.name;
			
			//Cria um jogador e atribui e retorna para ele o nome do jogador cadastrado
            Player jogador = GameState.Instance.RetornaJogador(nome); 

			if (jogador != null)
            {
                Player jogadorOponente; //Cria um jogador oponente 

				Game jogo = GameState.Instance.EncontrarPartida(jogador, out jogadorOponente);
                if (jogo != null)
                {
                    if (!string.IsNullOrEmpty(jogo.Vez) && jogo.Vez != jogador.Id) //Se for mesmoa  vez dele, continua a execução..
                        return true;

                    Card carta = EncontrarCarta(jogo, cartaNome); //Instancio uma carta que recebe a função de encontrar carta

					//Se o atributo e ultima carta estiver vazio, atribui a carta a ele como ultima selecionada
					if (jogo.LastCard == null)
                    {
						jogo.Vez = jogador.Id;
						jogo.LastCard = carta;
                        return true;
                    }

                    
                    bool isMatch = Combinou(jogo, carta); //cria um atributo booleano recebendo a função d eocmbinação correta

						//Se for verdadeiro
						if (isMatch)
						{
						ArmazenarCombinacao(jogador, carta); //armazena o jogador e a cartaa na função de armazenamento de combinações
							jogo.LastCard = null; //zera o atributo  de ultima carta
							Clients.Group(jogador.Grupo).showMatch(carta, nome); //exibe para todos as cartas combinada
							
						    //Caso acerte mais da metade das combinações, invoca a função de vencedor..
							if (jogador.Combinacoes.Count > 15)
							{
								Clients.Group(jogador.Grupo).winner(carta, nome); //Exibe para todos a função de vencedor

								GameState.Instance.Reiniciar(jogo); //Reinicia o jogo
								return true;
							}

							return true;
						}

                    Player oponente = GameState.Instance.RetornaOponente(jogador, jogo);
					
					//mudar para o outro oponente caso não faça a combinação
					jogo.Vez = oponente.Id; //o oponente recebe a vez

                    Clients.Group(jogador.Grupo).resetFlip(jogo.LastCard, carta); //invoca função do jquery de voltar o giro das cartas
					jogo.LastCard = null; //zera o atributo  de ultima carta
					return true;
                }
            }

            return false;
        }

//----------------------------------------------------------------------------------------------------------------------------------------------------------------
		
		//Função que armanzena o id e o par da carta na lista criada na classe de jogador..
		private void ArmazenarCombinacao(Player jogador, Card carta)
        {
			jogador.Combinacoes.Add(carta.Id); //armazena o id da carta na lista e combinações do jogador
			jogador.Combinacoes.Add(carta.Pair); //armazena o par da carta na lista e combinações do jogador
		}

		//Função para determinar combinação de cartas
        private bool Combinou(Game jogo, Card carta)
        {
            if (carta == null)
                return false;


            if (jogo.LastCard != null)
            {
                if (jogo.LastCard.Pair == carta.Id) //se as cartas corresponderem, ultima carta selecionada com a crata ID..
                {
                    return true;
                }

                return false;
            }
            return false;
        }

    }
}