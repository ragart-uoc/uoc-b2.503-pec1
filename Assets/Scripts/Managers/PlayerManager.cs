using System.Collections.Generic;
using UnityEngine;
using PEC1.Entities;

namespace PEC1.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static PlayerManager _instance;

        /// <value>Property <c>m_Players</c> represents the list of players.</value>
        private List<Player> m_Players;

        /// <value>Property <c>MinPlayerNumber</c> represents the minimum number of players.</value>
        private const int MinPlayers = 2;

        /// <value>Property <c>MaxPlayerNumber</c> represents the maximum number of players.</value>
        private const int MaxPlayers = 4;

        /// <value>Property <c>m_BasePlayers</c> represents the number of players in the base game.</value>
        private int m_BasePlayers;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            // Initialize the number of players in the base game
            m_BasePlayers = MinPlayers;

            // Initialize the list of players
            m_Players = new List<Player>();
        }

        /// <summary>
        /// Method <c>GetPlayers</c> returns the list of players.
        /// </summary>
        public List<Player> GetPlayers()
        {
            return m_Players;
        }

        /// <summary>
        /// Method <c>GetPlayer</c> returns the player with the specified number.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        public Player GetPlayer(int playerNumber)
        {
            return m_Players.Find(player => player.PlayerNumber == playerNumber);
        }

        /// <summary>
        /// Method <c>AddPlayer</c> adds a player to the list of players.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        /// <param name="tank">The instance of the tank that the player controls.</param>
        public void AddPlayer(int playerNumber, Tank tank)
        {
            m_Players.Add(new Player(playerNumber, tank));
        }

        /// <summary>
        /// Method <c>GetMinPlayers</c> returns the minimum number of players.
        /// </summary>
        public int GetMinPlayers()
        {
            return MinPlayers;
        }

        /// <summary>
        /// Method <c>GetMaxPlayers</c> returns the maximum number of players.
        /// </summary>
        public int GetMaxPlayers()
        {
            return MaxPlayers;
        }

        /// <summary>
        /// Method <c>GetBasePlayers</c> returns the number of players in the base game.
        /// </summary>
        public int GetBasePlayers()
        {
            return m_BasePlayers;
        }

        /// <summary>
        /// Method <c>SetBasePlayers</c> sets the number of players in the base game.
        /// </summary>
        /// <param name="basePlayerNumber">The number of players in the base game.</param>
        public void SetBasePlayers(int basePlayerNumber)
        {
            m_BasePlayers = basePlayerNumber;
        }
    }
}
