using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PEC1.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        /// <value>Property <c>m_PlayerManager</c> is a reference to the PlayerManager script.</value>
        private PlayerManager m_PlayerManager;

        /// <value>Property <c>currentPlayerNumber</c> represents the element of the UI that displays the current number of players.</value>
        public TextMeshProUGUI playersText;

        /// <value>Property <c>returnAfterCloseButton</c> represents the element of the UI that returns to the main menu after closing any floating screen.</value>
        private Button m_ReturnAfterCloseButton;

        /// <value>Property <c>controlsScreen</c> represents the element of the UI that displays the controls screen.</value>
        public GameObject controlsScreen;

        /// <value>Property <c>ControlsCloseButton</c> represents the element of the UI that closes the controls screen.</value>
        public Button controlsCloseButton;

        /// <value>Property <c>creditsScreen</c> represents the element of the UI that displays the credits screen.</value>
        public GameObject creditsScreen;

        /// <value>Property <c>CreditsCloseButton</c> represents the element of the UI that closes the credits screen.</value>
        public Button creditsCloseButton;

        /// <value>Property <c>inputAction</c> represents the input action that is used in-game.</value>
        public InputActionAsset inputAction;

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Get the PlayerManager script
            m_PlayerManager = FindObjectOfType<PlayerManager>();

            // Desactivate both the controls and the credits screens
            if (controlsScreen.activeSelf) controlsScreen.SetActive(false);
            if (creditsScreen.activeSelf) creditsScreen.SetActive(false);

            // Print the number of players on the player selection UI element
            PrintNumberOfPlayers();

            // Print the controls on the controls screen
            PrintPlayerControls();
        }

        /// <summary>
        /// Method <c>NewGame</c> loads the game scene removing any player progress.
        /// </summary>
        public void StartGame()
        {
            SceneManager.LoadScene("Game");
        }

        /// <summary>
        /// Method <c>ToggleControls</c> toggles the visibility of the controls screen.
        /// </summary>
        public void ToggleControls()
        {
            if (creditsScreen.activeSelf) return;
            if (!controlsScreen.activeSelf)
                m_ReturnAfterCloseButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            controlsScreen.SetActive(!controlsScreen.activeSelf);
            if (controlsScreen.activeSelf)
                controlsCloseButton.Select();
            else
                m_ReturnAfterCloseButton.Select();
        }

        /// <summary>
        /// Method <c>ToggleCredits</c> toggles the visibility of the credits screen.
        /// </summary>
        public void ToggleCredits()
        {
            if (controlsScreen.activeSelf) return;
            if (!creditsScreen.activeSelf)
                m_ReturnAfterCloseButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            creditsScreen.SetActive(!creditsScreen.activeSelf);
            if (creditsScreen.activeSelf)
                creditsCloseButton.Select();
            else
                m_ReturnAfterCloseButton.Select();
        }

        /// <summary>
        /// Method <c>QuitGame</c> quits the game.
        /// </summary>
        public void ExitGame()
        {
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        /// <summary>
        /// Method <c>ChangeNumberOfPlayers</c> changes the number of players.
        /// </summary>
        public void ChangeNumberOfPlayers(bool increase)
        {
            var minPlayers = m_PlayerManager.GetMinPlayers();
            var maxPlayers = m_PlayerManager.GetMaxPlayers();
            var playerCount = m_PlayerManager.GetBasePlayers();
                playerCount += (increase) ? 1 : -1;
            if (playerCount < minPlayers)
            {
                playerCount = maxPlayers;
            } else if (playerCount > maxPlayers)
            {
                playerCount = minPlayers;
            }
            m_PlayerManager.SetBasePlayers(playerCount);
            PrintNumberOfPlayers();
        }

        /// <summary>
        /// Method <c>PrintNumberOfPlayers</c> prints the number of players.
        /// </summary>
        private void PrintNumberOfPlayers()
        {
            var playerCount = m_PlayerManager.GetBasePlayers();
            playersText.text = "Number of players: " + playerCount.ToString();
        }

        /// <summary>
        /// Method <c>PrintPlayerControls</c> prints the controls of each player.
        /// </summary>
        private void PrintPlayerControls()
        {
            var maxPlayers = m_PlayerManager.GetMaxPlayers();
            for (var i = 1; i <= maxPlayers; i++)
            {
                // Get the player controls UI element
                var playerControls = controlsScreen.transform.Find("Player" + i + "Controls").gameObject;
                var playerControlsText = playerControls.transform.Find("Player" + i + "ControlsText")
                    .gameObject.GetComponent<TextMeshProUGUI>();

                // Get the player controls
                var playerInput = inputAction.FindActionMap("Player" + i);
                var move = playerInput.FindAction("Move");
                var jump = playerInput.FindAction("Fire");
                var altFire = playerInput.FindAction("AltFire");
                var join = playerInput.FindAction("Join");

                // Print the player controls
                playerControlsText.text = "Player " + i + " controls:\n" +
                                          "Move: " + move.GetBindingDisplayString(0) + "\n" +
                                          "Jump: " + jump.GetBindingDisplayString(0) + "\n" +
                                          "Alt Fire: " + altFire.GetBindingDisplayString(0) + "\n";
                if (join.bindings.Count > 0)
                    playerControlsText.text += "Join: " + join.GetBindingDisplayString(0);
                else
                    playerControlsText.text += "Join: -";
            }

        }
    }
}
