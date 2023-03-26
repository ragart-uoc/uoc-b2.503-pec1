using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace PEC1.Entities
{
    /// <summary>
    /// Class <c>TankManager</c> is used to manage various settings on a tank. It works with the GameManager class to control how the tanks behave and whether or not players have control of their tank in the different phases of the game.
    /// </summary>
    [Serializable]
    public class Tank
    {
        /// <value>Property <c>playerColor</c> represents the color this tank will be tinted.</value>
        [FormerlySerializedAs("m_PlayerColor")]
        public Color playerColor;

        /// <value>Property <c>spawnPoint</c> represents the position and direction the tank will have when it spawns.</value>
        [FormerlySerializedAs("m_SpawnPoint")]
        public Transform spawnPoint;

        /// <value>Property <c>playerNumber</c> specifies which player this the manager for.</value>
        [FormerlySerializedAs("m_PlayerNumber")]
        [HideInInspector] public int playerNumber;

        /// <value>Property <c>coloredPlayerText</c> represents the player with their number colored to match their tank.</value>
        [FormerlySerializedAs("m_ColoredPlayerText")]
        [HideInInspector] public string coloredPlayerText;

        /// <value>Property <c>instance</c> represents the instance of the tank when it is created.</value>
        [FormerlySerializedAs("m_Instance")]
        [HideInInspector] public GameObject instance;

        /// <value>Property <c>wins</c> represents the number of wins this player has so far.</value>
        [FormerlySerializedAs("m_Wins")] [HideInInspector]
        public int wins;

        /// <value>Property <c>m_Movement</c> represents the tank's movement script, used to disable and enable control.</value>
        private TankMovement m_Movement;

        /// <value>Property <c>m_Shooting</c> represents the tank's shooting script, used to disable and enable control.</value>
        private TankShooting m_Shooting;

        /// <value>Property <c>m_Health</c> represents the tank's health script, used to disable and enable control.</value>
        private TankHealth m_Health;

        /// <value>Property <c>m_CanvasGameObject</c> is used to disable the world space UI during the Starting and Ending phases of each round.</value>
        private GameObject m_CanvasGameObject;

        /// <value>Property <c>m_PlayerInput</c> represents the tank's PlayerInput component.</value>
        private PlayerInput m_PlayerInput;

        /// <summary>
        /// Method <c>Setup</c> is used to configure the tank.
        /// </summary>
        public void Setup()
        {
            // Get references to the components.
            m_Movement = instance.GetComponent<TankMovement>();
            m_Shooting = instance.GetComponent<TankShooting>();
            m_Health = instance.GetComponent<TankHealth>();
            m_PlayerInput = instance.GetComponent<PlayerInput>();
            m_CanvasGameObject = instance.GetComponentInChildren<Canvas>().gameObject;

            // Set the player numbers to be consistent across the scripts.
            m_Movement.playerNumber = playerNumber;
            m_Shooting.playerNumber = playerNumber;
            m_Health.playerNumber = playerNumber;

            // Create a string using the correct color that says 'PLAYER 1' etc based on the tank's color and the player's number.
            coloredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(playerColor) + ">PLAYER " + playerNumber + "</color>";

            // Get all of the renderers of the tank.
            var renderers = instance.GetComponentsInChildren<MeshRenderer>();

            // Go through all the renderers...
            foreach (var r in renderers)
            {
                // ... set their material color to the color specific to this tank.
                r.material.color = playerColor;
            }

            // Assign the control scheme
            AssignControlScheme();
        }

        /// <summary>
        /// Method <c>DisableControl</c> is used to disable the tank during the phases of the game where the player shouldn't be able to control it.
        /// </summary>
        public void DisableControl()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive(false);
        }

        /// <summary>
        /// Method <c>EnableControl</c> is used to enable the tank during the phases of the game where the player should be able to control it.
        /// </summary>
        public void EnableControl()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive(true);
        }

        /// <summary>
        /// Method <c>Reset</c> is used at the start of each round to put the tank into its default state.
        /// </summary>
        public void Reset()
        {
            // Reset the tank's position and rotation
            instance.transform.position = spawnPoint.position;
            instance.transform.rotation = spawnPoint.rotation;

            // Reset the tank's health
            instance.SetActive(false);
            instance.SetActive(true);

            // Reassign the control scheme
            AssignControlScheme();
        }

        private void AssignControlScheme()
        {
            m_PlayerInput.SwitchCurrentControlScheme("Keyboard", Keyboard.current);
            m_PlayerInput.SwitchCurrentActionMap("Player" + playerNumber);
        }
    }
}