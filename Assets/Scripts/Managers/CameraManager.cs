using System.Linq;
using UnityEngine;
using Cinemachine;

namespace PEC1.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> controls the flow of the game.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static CameraManager _instance;
        
        /// <value>Property <c>playerCameraPrefab</c> represents the prefab to use for the Cinemachine camera.</value>
        public GameObject playerCameraPrefab;

        /// <value>Property <c>m_CameraRig</c> is a reference to the camera rig.</value>
        public GameObject cameraRig;

        /// <value>Property <c>groupCamera</c> is a reference to the group camera.</value>
        public GameObject groupCamera;

        /// <value>Property <c>m_GroupTargetCamera</c> is a reference to the group camera CinemachineTargetGroup component.</value>
        private CinemachineTargetGroup m_GroupTargetCamera;

        /// <value>Property <c>worldCamera</c> is a reference to the world camera.</value>
        public GameObject worldCamera;

        /// <value> Property <c>m_SplitMode</c> represents whether or not the camera is in split mode.</value>
        private bool m_SplitMode = true;

        /// <value> Property <c>splitDistance</c> represents the distance at which the camera switches to split mode.</value>
        public float splitDistance = 25.0f;

        /// <value> Property <c>splitHysteresis</c> represents the distance at which the camera switches back to single mode.</value>
        public float splitHysteresis = 5.0f;
        
        /// <value>Property <c>m_PlayerManager</c> is a reference to the PlayerManager script.</value>
        private PlayerManager m_PlayerManager;

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
            
            // Get the group camera target
            m_GroupTargetCamera = groupCamera.transform.Find("GroupTargetCamera").GetComponent<CinemachineTargetGroup>();
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed frame-rate frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void FixedUpdate()
        {
            Split();
        }

        /// <summary>
        /// Method <c>AddCamera</c> adds a camera to a tank.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        public GameObject AddCamera(int playerNumber)
        {
            // Get the player.
            var player = m_PlayerManager.GetPlayer(playerNumber);
            
            // Create the camera.
            var playerCamera = Instantiate(playerCameraPrefab, cameraRig.transform, true);
            playerCamera.name = "PlayerCamera" + playerNumber;
            playerCamera.layer = LayerMask.NameToLayer("Player" + playerNumber);

            // Configure the Unity camera.
            var uCamera = playerCamera.transform.Find("PlayerUnityCamera").GetComponent<Camera>();
            uCamera.gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber);
            var currentCullingMask = uCamera.cullingMask;
            uCamera.cullingMask = currentCullingMask | 1 << LayerMask.NameToLayer("Player" + playerNumber);

            // Configure the Cinemachine camera and make it follow the player.
            var vCamera = playerCamera.transform.Find("PlayerVirtualCamera").GetComponent<CinemachineVirtualCamera>();
            vCamera.gameObject.layer = LayerMask.NameToLayer("Player" + playerNumber);
            vCamera.Follow = player.tank.instance.transform;
            vCamera.LookAt = player.tank.instance.transform;
            vCamera.m_Lens.OrthographicSize = 5;

            return playerCamera;
        }

        /// <summary>
        /// Method <c>AdjustCameras</c> adjusts the cameras to the number of players.
        /// </summary>
        public void AdjustCameras()
        {
            // Get the players.
            var players = m_PlayerManager.GetPlayers();
            // Get the number of alive players.
            var alivePlayers = players.FindAll(player => player.tank.instance.activeSelf);
            var alivePlayersCount = alivePlayers.Count;
            // Get the aspect ratio
            var aspectRatio = (float) Screen.width / Screen.height;

            foreach (var player in alivePlayers)
            {
                var uCamera = player.camera.transform.Find("PlayerUnityCamera").GetComponent<Camera>();

                switch (alivePlayersCount)
                {
                    case 1:
                        player.camera.SetActive(false);
                        break;
                    case 2:
                        uCamera.rect = (player.number == alivePlayers[0].number)
                            ? new Rect(0, 0.5f, 1 / aspectRatio, 0.5f)
                            : new Rect(1 - (1 / aspectRatio), 0, 1 / aspectRatio, 0.5f);
                        break;
                    case > 2:
                    {
                        var xPosition = (player.number % 2 == 0) ? 0.5f : 0;
                        var yPosition = (player.number > 2) ? 0 : 0.5f;
                        uCamera.rect = new Rect(xPosition, yPosition, 0.5f, 0.5f);
                        break;
                    }
                }
            }

            // Set the world camera
            if (alivePlayersCount == 3)
            {
                var wuCamera = worldCamera.transform.Find("WorldUnityCamera").GetComponent<Camera>();
                // Get alive players numbers
                var alivePlayersNumbers = alivePlayers.Select(player => player.number).ToList();
                // Get the missing player number
                var missingPlayerNumber = Enumerable.Range(1, m_PlayerManager.GetMaxPlayers())
                    .Except(alivePlayersNumbers).First();
                // Set the world camera rect
                var xPosition = (missingPlayerNumber % 2 == 0) ? 0.5f : 0;
                var yPosition = (missingPlayerNumber > 2) ? 0 : 0.5f;
                wuCamera.rect = new Rect(xPosition, yPosition, 0.5f, 0.5f);
                worldCamera.SetActive(true);
            }
            else if (worldCamera.activeSelf)
            {
                worldCamera.SetActive(false);
            }
        }
        
        /// <summary>
        /// Method <c>AddTargetToGroupCamera</c> adds a target to the group camera.
        /// </summary>
        /// <param name="target">The target to add.</param>
        public void AddTargetToGroupCamera(GameObject target)
        {
            m_GroupTargetCamera.AddMember(target.transform, 1, 0);
        }
        
        /// <summary>
        /// Method <c>RemoveTargetFromGroupCamera</c> removes a target from the group camera.
        /// </summary>
        /// <param name="target">The target to remove.</param>
        public void RemoveTargetFromGroupCamera(GameObject target)
        {
            m_GroupTargetCamera.RemoveMember(target.transform);
        }

        /// <summary>
        /// Method <c>Split</c> checks if the screen needs to be splitted.
        /// </summary>
        private void Split()
        {
            // Get the alive players
            var alivePlayers = m_PlayerManager.GetPlayers().FindAll(player => player.tank.instance.activeSelf);
            // Get the number of alive players.
            var playerCount = alivePlayers.Count;
            switch (playerCount)
            {
                // Return if the targets are not 2.
                case > 2:
                    m_SplitMode = true;
                    return;
                case < 2:
                    m_SplitMode = false;
                    return;
            }
            
            // Get the targets.
            var target1 = alivePlayers[0].tank.instance.transform;
            var target2 = alivePlayers[1].tank.instance.transform;

            // Get the distance between the targets.
            var distance = Vector3.Distance(target1.position, target2.position);

            switch (m_SplitMode)
            {
                case true when distance < splitDistance - splitHysteresis:
                {
                    m_SplitMode = false;
                    alivePlayers[0].camera.SetActive(false);
                    alivePlayers[1].camera.SetActive(false);
                    break;
                }
                case false when distance > splitDistance + splitHysteresis:
                {
                    m_SplitMode = true;
                    alivePlayers[0].camera.SetActive(true);
                    alivePlayers[1].camera.SetActive(true);
                    break;
                }
            }
        }
    }
}
