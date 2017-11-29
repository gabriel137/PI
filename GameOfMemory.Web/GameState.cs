using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using GameOfMemory.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Security.Cryptography;
using System.Text;

namespace GameOfMemory.Web
{
  public class GameState
  {

			//Torna a inicialização do objeto mais lenta para manter a espera pelo adversário, evita desperdicio de memória pelo objeto enquanto ainda não está sendo usado
			private static readonly Lazy<GameState> _estadoJogo = new Lazy<GameState>(() => new GameState(GlobalHost.ConnectionManager.GetHubContext<GameHub>()));

			//Cria uma coleção que armazena uma chave em string(um nome para busca) e um valor(o jogador cadastrado)
			private readonly ConcurrentDictionary<string, Player> _jogadores =  new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

			//Cria uma coleção que armazena uma chave em string(um nome para busca) e um valor(o jogo criado)
			private readonly ConcurrentDictionary<string, Game> _jogos =  new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);


//----------------------------------------------------------------------------------------------------------------------------------------------------------------

			public static GameState Instance
			{
			  get { return _estadoJogo.Value; }
			}


			//O IHubContext é quem permite o acesso as informações da interface IHub
			private GameState(IHubContext context)
			{
			  Clients = context.Clients;
			  Groups = context.Groups;
			}

			private IHubConnectionContext Clients { get; set; } //encapsula as informações de conexão
			private IGroupManager Groups { get; set; } //O IGroupManager é quem gerencia um grupo para conexão no hub ele adicona e remove uma conexão ao grupo



//----------------------------------------------------------------------------------------------------------------------------------------------------------------


			//Função que irá criar um novo jogador
			public Player CriarJogador(string nomeJogador)
			{

			  Player player = new Player(nomeJogador, GetMD5Hash(nomeJogador)); //cria um jogador e seu código único

				_jogadores[nomeJogador] = player; //atribui o jogador criado a lista de jogadores

			  return player; //retorna o jogador
			}


			//cria um código único para ser atribuído ao jogador em sua criação Ex: 0C2E99D0949684278C30B9369B82638E1CEAD415
			private string GetMD5Hash(string userName)
			{
			  return string.Join("", MD5.Create()
				.ComputeHash(Encoding.Default.GetBytes(userName))
				.Select(b => b.ToString()));
			}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------





				//método que 'pega' o jogador..
				public Player RetornaJogador(string nomeJogador)
				{
					//Compara o userName passado no parâmetro com o nome registrado no jogador
					return _jogadores.Values.FirstOrDefault(p => p.Nome == nomeJogador); 
				}



				//método que 'pega' um novo oponente..
				public Player RetornaNovoOponente(Player player)
				{
					  //Retorna caso o jogador não esteja jogando e o ID seja diferente do jogador da fila de espera..	
					  return _jogadores.Values.FirstOrDefault((p => !p.EstaJogando && p.Id != player.Id));
				}




				//Método que retorna o oponente 
				public Player RetornaOponente(Player player, Game jogo)
				{
	
				  //Se o ID cadastrado na classe 'game' for igual o ID do jogador..
				  if (jogo.Jogador1.Id == player.Id)
					return jogo.Jogador2; //Retorna o oponente que é o jogador 2

				  //Se não, o jogador oponente é o Jogador 1.
				  return jogo.Jogador1;

				}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------


		//Função que cria o jogo criando nos parâmetros o jogador 1 e 2 
		public Game Criarjogo(Player jogador1, Player jogador2)
		{
		
			  //Atribui a variável um objeto instanciado do jogo já cirando os jogadores e o tabuleiro(Board)..			
			  var jogo = new Game()
				  {
					Jogador1 = jogador1,
					Jogador2 = jogador2,
					Board = new Board() //cria um novo tabuleiro
				  };


			  var grupo = Guid.NewGuid().ToString(); //cria uma variável grupo que recebe um identificador único em string
					_jogos[grupo] = jogo;

					jogador1.EstaJogando = true;
					jogador1.Grupo = grupo;

					jogador1.EstaJogando = true;
					jogador2.Grupo = grupo;

				  Groups.Add(jogador1.ConnectionId, grupo); //Adiciona o id do jogador 1 e o grupo atual do jogo ao parâmetro Groups do Hub
				  Groups.Add(jogador2.ConnectionId, grupo); //Adiciona o id do jogador 2 e o grupo atual do jogo ao parâmetro Groups do Hub

			return jogo;
		}

//----------------------------------------------------------------------------------------------------------------------------------------------------------------

		//Funçao para encontrar um novo jogo
		public Game EncontrarPartida(Player jogador, out Player oponente)
		{

				oponente = null;

				  if (jogador.Grupo == null)
					return null;

			  Game jogo;
				_jogos.TryGetValue(jogador.Grupo, out jogo); //tenta pegar o valor de grupo do context passando a variável como referência


			  if (jogo != null)
			  {
				if (jogo.Jogador1.Id == jogador.Id) //se o mesmo jogador cadastrado na classe Game for igual ao id da classe Player...
				{
							oponente = jogo.Jogador2; 
				  return jogo;
				}

						oponente = jogo.Jogador1;
						return jogo;
			  }

			  return null;
		 }
//----------------------------------------------------------------------------------------------------------------------------------------------------------------


		//Função para reniciar o jogo
		public void Reiniciar(Game game)
		{
			  var grupo = game.Jogador1.Grupo;
			  var j1Nome = game.Jogador1.Nome;
			  var j2Nome = game.Jogador2.Nome;

			  Groups.Remove(game.Jogador1.ConnectionId, grupo); //Remove a conexão atual do jogador 1..
			  Groups.Remove(game.Jogador2.ConnectionId, grupo); //remove a conexão atual do jogador 2..


			  Player j1;
					_jogadores.TryRemove(j1Nome, out j1); //remove da lista de jogadores o jogador1

			  Player j2;
					_jogadores.TryRemove(j2Nome, out j2); //remove da lista de jogadores o jogador2

			Game jogo;
					_jogos.TryRemove(grupo, out jogo); //remove remove o grupo da lista de jogos
		}
  }
}