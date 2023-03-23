using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using PEC1.Cameras;

namespace PEC1.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> controls the flow of the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <value>Property <c>roundsToWin</c> represents the number of rounds a single player has to win to win the game.</value>
        public int roundsToWin = 5;

        /// <value>Property <c>startDelay</c> represents the delay between the start of RoundStarting and RoundPlaying phases.</value>
        [FormerlySerializedAs("m_StartDelay")]
        public float startDelay = 3f;

        /// <value>Property <c>endDelay</c> represents the delay between the end of RoundPlaying and RoundEnding phases.</value>
        [FormerlySerializedAs("m_EndDelay")]
        public float endDelay = 3f;

        /// <value>Property <c>cameraControl</c> is a reference to the CameraControl script for control during different phases.</value>
        [FormerlySerializedAs("m_CameraControl")]
        public CameraControl cameraControl;

        /// <value>Property <c>messageText</c> is a reference to the overlay Text to display winning text, etc.</value>
        [FormerlySerializedAs("m_MessageText")]
        public TextMeshProUGUI messageText;

        /// <value>Property <c>tankPrefab</c> represents the prefab to use for the tanks.</value>
        [FormerlySerializedAs("m_TankPrefab")]
        public GameObject tankPrefab;

        /// <value>Property <c>tanks</c> is a collection of managers for enabling and disabling different aspects of the tanks.</value>
        [FormerlySerializedAs("m_Tanks")]
        public TankManager[] tanks;

        /// <value>Property <c>m_RoundNumber</c> represents which round the game is currently on.</value>
        private int m_RoundNumber;

        /// <value>Property <c>m_StartWait</c> is used to have a delay whilst the round starts.</value>
        private WaitForSeconds m_StartWait;

        /// <value>Property <c>m_EndWait</c> is used to have a delay whilst the round or game ends.</value>
        private WaitForSeconds m_EndWait;

        /// <value>Property <c>m_RoundWinner</c> is a reference to the winner of the current round. Used to make an announcement of who won.</value>
        private TankManager m_RoundWinner;

        /// <value>Property <c>m_GameWinner</c> is a reference to the winner of the game. Used to make an announcement of who won.</value>
        private TankManager m_GameWinner;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(startDelay);
            m_EndWait = new WaitForSeconds(endDelay);

            SpawnAllTanks();
            SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Method <c>SpawnAllTanks</c> spanws all the tanks and set their references and player numbers.
        /// </summary>
        private void SpawnAllTanks()
        {
            var mainCam = GameObject.Find("Main Camera").GetComponent<Camera>();

            // For all the tanks...
            for (var i = 0; i < tanks.Length; i++)
            {
                // ... create them, set their player number and references needed for control.
                var playerNumber = i + 1;
                tanks[i].instance =
                    Instantiate(tankPrefab, tanks[i].spawnPoint.position, tanks[i].spawnPoint.rotation);
                tanks[i].playerNumber = playerNumber;
                tanks[i].Setup();
                AddCamera(i, mainCam);
            }
        }

        /// <summary>
        /// Method <c>SetCameraTargets</c> creates a collection of targets for the camera to follow.
        /// </summary>
        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            var targets = new Transform[tanks.Length];

            // For each of these transforms...
            for (var i = 0; i < targets.Length; i++)
            {
                // ... set it to the appropriate tank transform.
                targets[i] = tanks[i].instance.transform;
            }

            // These are the targets the camera should follow.
            cameraControl.targets = targets;
        }

        /// <summary>
        /// Method <c>AddCamera</c> adds a camera to a tank.
        /// </summary>
        private void AddCamera(int i, Camera mainCam) {
            var childCam = new GameObject( "Camera"+(i+1) );
            var newCam = childCam.AddComponent<Camera>();
            newCam.CopyFrom(mainCam);

            childCam.transform.parent = tanks[i].instance.transform;
            newCam.rect = i==0
                ? new Rect(0.0f, 0.5f, 0.89f, 0.5f)
                : new Rect(0.11f, 0.0f, 0.89f, 0.5f);
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
            DisableTankControl();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            cameraControl.SetStartPositionAndSize();

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
            EnableTankControl();

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
            DisableTankControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.wins++;

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
            var numTanksLeft = tanks.Count(tank => tank.instance.activeSelf);
            return numTanksLeft <= 1;
        }

        /// <summary>
        /// Method <c>GetRoundWinner</c> is called at the end of a round to determine which tank (if any) won the round.
        /// </summary>
        private TankManager GetRoundWinner()
        {
            return tanks.FirstOrDefault(t => t.instance.activeSelf);
        }

        /// <summary>
        /// Method <c>GetGameWinner</c> is called at the end of a round to determine which tank (if any) won the game.
        /// </summary>
        private TankManager GetGameWinner()
        {
            return tanks.FirstOrDefault(t => t.wins == roundsToWin);
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
            message = tanks.Aggregate(message, (current, t) => current + (t.coloredPlayerText + ": " + t.wins + " WINS\n"));

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
            foreach (var t in tanks)
            {
                t.Reset();
            }
        }

        /// <summary>
        /// Method <c>EnableTankControl</c> is used to turn all the tanks back on so that the players can control them.
        /// </summary>
        private void EnableTankControl()
        {
            foreach (var t in tanks)
            {
                t.EnableControl();
            }
        }

        /// <summary>
        /// Method <c>DisableTankControl</c> is used to turn all the tanks off so that the players can't control them.
        /// </summary>
        private void DisableTankControl()
        {
            foreach (var t in tanks)
            {
                t.DisableControl();
            }
        }
    }
}