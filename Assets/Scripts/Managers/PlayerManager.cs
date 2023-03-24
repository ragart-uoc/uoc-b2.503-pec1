using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PEC1.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static PlayerManager _instance;
        
        /// <value>Property <c>playerNumber</c> represents the number of players.</value>
        private int m_Players;
        
        /// <value>Property <c>MinPlayerNumber</c> represents the minimum number of players.</value>
        private const int MinPlayers = 2;

        /// <value>Property <c>MaxPlayerNumber</c> represents the maximum number of players.</value>
        private const int MaxPlayers = 4;

        /// <value>Property <c>playerList</c> represents the list of players.</value>
        public List<int> playerList;

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
            
            // Initialize player list
            SetPlayers(MinPlayers);
        }
        
        // Method to get the number of players
        public int GetPlayers()
        {
            return m_Players;
        }
        
        // Method to get the minimum number of players
        public int GetMinPlayers()
        {
            return MinPlayers;
        }
        
        // Method to get the maximum number of players
        public int GetMaxPlayers()
        {
            return MaxPlayers;
        }
        
        public List<int> GetPlayerList()
        {
            return playerList;
        }
        
        public void SetPlayers(int playerNumber)
        {
            m_Players = playerNumber;
            playerList.Clear();
            playerList.AddRange(Enumerable.Range(1, m_Players));
        }

        /// <summary>
        /// Method <c>AddPlayer</c> adds a player to the player list.
        /// </summary>
        public void AddPlayer(int playerId)
        {
            playerList.Add(playerId);
            m_Players++;
        }
    }
}
