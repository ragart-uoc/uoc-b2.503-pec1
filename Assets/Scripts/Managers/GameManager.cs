using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;
using PEC1.Entities;

namespace PEC1.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> controls the flow of the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static GameManager _instance;
        
        /// <value>Property <c>roundsToWin</c> represents the number of rounds a single player has to win to win the game.</value>
        public int roundsToWin = 5;

        /// <value>Property <c>startDelay</c> represents the delay between the start of RoundStarting and RoundPlaying phases.</value>
        [FormerlySerializedAs("m_StartDelay")]
        public float startDelay = 3f;

        /// <value>Property <c>endDelay</c> represents the delay between the end of RoundPlaying and RoundEnding phases.</value>
        [FormerlySerializedAs("m_EndDelay")]
        public float endDelay = 3f;

        /// <value>Property <c>playersInfo</c> is a reference to the text displaying the player information.</value>
        public GameObject playersInfo;

        /// <value>Property <c>messageText</c> is a reference to the overlay Text to display winning text, etc.</value>
        [FormerlySerializedAs("m_MessageText")]
        public TextMeshProUGUI messageText;

        /// <value>Property <c>m_RoundNumber</c> represents which round the game is currently on.</value>
        private int m_RoundNumber;

        /// <value>Property <c>m_StartWait</c> is used to have a delay whilst the round starts.</value>
        private WaitForSeconds m_StartWait;

        /// <value>Property <c>m_EndWait</c> is used to have a delay whilst the round or game ends.</value>
        private WaitForSeconds m_EndWait;

        /// <value>Property <c>m_RoundWinner</c> is a reference to the winner of the current round. Used to make an announcement of who won.</value>
        private Tank m_RoundWinner;

        /// <value>Property <c>m_GameWinner</c> is a reference to the winner of the game. Used to make an announcement of who won.</value>
        private Tank m_GameWinner;

        /// <value>Property <c>m_PlayerManager</c> is a reference to the PlayerManager script.</value>
        private PlayerManager m_PlayerManager;

        /// <value>Property <c>m_CameraManager</c> is a reference to the CameraManager script.</value>
        private CameraManager m_CameraManager;

        /// <value>Property <c>m_TankManager</c> is a reference to the TankManager script.</value>
        private TankManager m_TankManager;

        /// <value>Property <c>m_PlayerInput</c> is a reference to the PlayerInput script.</value>
        private PlayerInput m_PlayerInput;

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
            
            // Get the PlayerInput component.
            m_PlayerInput = GetComponent<PlayerInput>();

            // Get the managers
            m_PlayerManager = FindObjectOfType<PlayerManager>();
            m_CameraManager = FindObjectOfType<CameraManager>();
            m_TankManager = FindObjectOfType<TankManager>();
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Create the players
            CreatePlayers();
            UpdatePlayerInfo();

            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(startDelay);
            m_EndWait = new WaitForSeconds(endDelay);

            // Spawn all the tanks
            m_TankManager.SpawnAllTanks();
            m_CameraManager.AdjustCameras();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Method <c>CreatePlayers</c> creates the players.
        /// </summary>
        private void CreatePlayers()
        {
            var basePlayerNumber = m_PlayerManager.GetBasePlayers();
            var tanks = m_TankManager.GetTanks();
            for (var playerNumber = 1; playerNumber <= basePlayerNumber; playerNumber++)
            {
                m_PlayerManager.AddPlayer(playerNumber, tanks[playerNumber - 1]);
            }
        }

        /// <summary>
        /// Method <c>GameLoop</c> is the coroutine that controls the flow of the game. It's called from start and will run each phase of the game one after another
        /// </summary>
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level.
                SceneManager.LoadScene("Game");
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues.
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine(GameLoop());
            }
        }

        /// <summary>
        /// Method <c>RoundStarting</c> is called before each round to set up the round.
        /// </summary>
        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks();
            DisableControls();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            messageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }

        /// <summary>
        /// Method <c>RoundPlaying</c> is called from GameLoop each time a round is playing.
        /// </summary>
        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableControls();

            // Clear the text from the screen.
            messageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }

        /// <summary>
        /// Method <c>RoundEnding</c> is called after each round to clean up things and determine if there is a winner of the game.
        /// </summary>
        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            DisableControls();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.wins++;

            // Update the player information text.
            UpdatePlayerInfo();

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            var message = EndMessage();
            messageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }

        /// <summary>
        /// Method <c>OneTankLeft</c> is used to check if there is one or fewer tanks remaining and thus the round should end.
        /// </summary>
        private bool OneTankLeft()
        {
            var players = m_PlayerManager.GetPlayers();
            var numTanksLeft = players.Count(player => player.tank.instance.activeSelf);
            return numTanksLeft <= 1;
        }

        /// <summary>
        /// Method <c>GetRoundWinner</c> is called at the end of a round to determine which tank (if any) won the round.
        /// </summary>
        private Tank GetRoundWinner()
        {
            var players = m_PlayerManager.GetPlayers();
            return players.FirstOrDefault(p => p.tank.instance.activeSelf)?.tank;
        }

        /// <summary>
        /// Method <c>GetGameWinner</c> is called at the end of a round to determine which tank (if any) won the game.
        /// </summary>
        private Tank GetGameWinner()
        {
            var players = m_PlayerManager.GetPlayers();
            return players.FirstOrDefault(p => p.tank.wins == roundsToWin)?.tank;
        }

        /// <summary>
        /// Method <c>EndMessage</c> is used to construct a string message to display at the end of each round.
        /// </summary>
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            var message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.coloredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            var players = m_PlayerManager.GetPlayers();
            message = players.Aggregate(message, (current, p) => current + (p.tank.coloredPlayerText + ": " + p.tank.wins + " WINS\n"));

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.coloredPlayerText + " WINS THE GAME!";

            return message;
        }

        /// <summary>
        /// Method <c>ResetAllTanks</c> is used to reset all the tanks and put them back into their starting positions.
        /// </summary>
        private void ResetAllTanks()
        {
            var players = m_PlayerManager.GetPlayers();
            foreach (var p in players)
            {
                p.tank.Reset();
                p.camera.SetActive(true);
            }
            m_CameraManager.AdjustCameras();
        }

        /// <summary>
        /// Method <c>EnableControls</c> is used to turn all the tanks back on so that the players can control them.
        /// </summary>
        private void EnableControls()
        {
            m_PlayerInput.enabled = true;
            m_PlayerInput.SwitchCurrentControlScheme("Keyboard", Keyboard.current);
            m_PlayerInput.SwitchCurrentActionMap("PlayerManagement");
            var players = m_PlayerManager.GetPlayers();
            foreach (var p in players)
            {
                p.tank.EnableControl();
            }
        }

        /// <summary>
        /// Method <c>DisableControls</c> is used to turn all the tanks off so that the players can't control them.
        /// </summary>
        private void DisableControls()
        {
            m_PlayerInput.enabled = false;
            var players = m_PlayerManager.GetPlayers();
            foreach (var p in players)
            {
                p.tank.DisableControl();
            }
        }

        /// <summary>
        /// Method <c>OnPlayer3Join</c> is called when the player 3 join button is pressed.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        private void OnPlayer3Join(InputValue inputValue)
        {
            Join(3);
        }

        /// <summary>
        /// Method <c>OnPlayer4Join</c> is called when the player 4 join button is pressed.
        /// </summary>
        /// <param name="inputValue">The input value.</param>
        private void OnPlayer4Join(InputValue inputValue)
        {
            Join(4);
        }

        /// <summary>
        /// Method <c>Join</c> is called when a player joins the game.
        /// </summary>
        private void Join(int playerNumber)
        {
            if (m_PlayerManager.GetPlayer(playerNumber) != null)
                return;
            var tanks = m_TankManager.GetTanks();
            m_PlayerManager.AddPlayer(playerNumber, tanks[playerNumber - 1]);
            UpdatePlayerInfo();
            m_TankManager.SpawnTank(playerNumber);
            m_CameraManager.AdjustCameras();
            StartCoroutine(JoinMessage());
        }
        
        /// <summary>
        /// Method <c>JoinMessage</c> is used to display a message when a player joins the game.
        /// </summary>
        private IEnumerator JoinMessage()
        {
            messageText.text = "HERE COMES A NEW CHALLENGER!";
            yield return new WaitForSeconds(2);
            messageText.text = String.Empty;
        }

        /// <summary>
        /// Method <c>UpdatePlayerInfo</c> updates the value of the player info text.
        /// </summary>
        private void UpdatePlayerInfo()
        {
            var maxPlayers = m_PlayerManager.GetMaxPlayers();
            for (var i = 1; i <= maxPlayers; i++)
            {
                // Get the UI element for the player.
                var playerInfo = playersInfo.transform.Find("Player" + i + "Info").gameObject;
                var playerInfoTitle = playerInfo.transform.Find("Player" + i + "Title").gameObject
                    .GetComponent<TextMeshProUGUI>();
                var playerInfoText = playerInfo.transform.Find("Player" + i + "Wins").gameObject
                    .GetComponent<TextMeshProUGUI>();

                var player = m_PlayerManager.GetPlayer(i);
                if (player != null)
                {
                    var color = player.tank.playerColor;
                    playerInfoTitle.color = new Color(color.r, color.g, color.b, 1);
                }
                playerInfoText.text = (player == null) ? "JOIN" : "WINS: " + player.tank.wins;
            }
        }
    }
}