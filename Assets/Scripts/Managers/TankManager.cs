using UnityEngine;
using PEC1.Entities;

namespace PEC1.Managers
{
    public class TankManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static TankManager _instance;
        
        /// <value>Property <c>tankContainer</c> represents the container for the tanks.</value>
        public GameObject tankContainer;

        /// <value>Property <c>tanks</c> is a collection of managers for enabling and disabling different aspects of the tanks.</value>
        [SerializeField]
        private Tank[] tanks;

        /// <value>Property <c>tankPrefab</c> represents the prefab to use for the tanks.</value>
        public GameObject tankPrefab;
        
        /// <value>Property <c>m_PlayerManager</c> is a reference to the PlayerManager script.</value>
        private PlayerManager m_PlayerManager;

        /// <value>Property <c>m_CameraManager</c> is a reference to the CameraManager script.</value>
        private CameraManager m_CameraManager;

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
            
            // Get the managers
            m_PlayerManager = FindObjectOfType<PlayerManager>();
            m_CameraManager = FindObjectOfType<CameraManager>();
        }

        /// <summary>
        /// Method <c>GetTanks</c> returns the list of tanks.
        /// </summary>
        public Tank[] GetTanks()
        {
            return tanks;
        }

        /// <summary>
        /// Method <c>SpawnAllTanks</c> spanws all the tanks and set their references and player numbers.
        /// </summary>
        public void SpawnAllTanks()
        {
            var players = m_PlayerManager.GetPlayers();
            foreach (var p in players)
            {
                SpawnTank(p.number);
            }
        }

        /// <summary>
        /// Method <c>SpawnTank</c> spawns a tank and sets its references and player number.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        public void SpawnTank(int playerNumber)
        {
            var player = m_PlayerManager.GetPlayer(playerNumber);
            player.tank.playerNumber = playerNumber;
            player.tank.instance =
                Instantiate(tankPrefab, player.tank.spawnPoint.position, player.tank.spawnPoint.rotation);
            player.tank.instance.transform.SetParent(tankContainer.transform);
            player.tank.Setup();
            player.camera = m_CameraManager.AddCamera(player.number);
            m_CameraManager.AddTargetToGroupCamera(player.tank.instance);
        }
    }
}
