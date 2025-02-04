﻿using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PEC1.Entities
{
    /// <summary>
    /// Class <c>TankShooting</c> is used to control the shooting of the tank.
    /// </summary>
    public class TankShooting : MonoBehaviour
    {
        /// <value>Property <c>playerNumber</c> is used to identify the different players.</value>
        [FormerlySerializedAs("m_PlayerNumber")]
        public int playerNumber = 1;

        /// <value>Property <c>shell</c> represents the prefab of the shell.</value>
        [FormerlySerializedAs("m_Shell")]
        public Rigidbody shell;

        /// <value>Property <c>altShell</c> represents the prefab of the alternative shell.</value>
        [FormerlySerializedAs("m_AltShell")]
        public Rigidbody altShell;

        /// <value>Property <c>fireTransform</c> represents the child of the tank where the shells are spawned.</value>
        [FormerlySerializedAs("m_FireTransform")]
        public Transform fireTransform;

        /// <value>Property <c>aimSlider</c> represents a child of the tank that displays the current launch force.</value>
        [FormerlySerializedAs("m_AimSlider")]
        public Slider aimSlider;

        /// <value>Property <c>shootingAudio</c> represents the audio source to play when the shell is fired.</value>
        [FormerlySerializedAs("m_ShootingAudio")]
        public AudioSource shootingAudio;

        /// <value>Property <c>chargingClip</c> represents the audio that plays when each shot is charging up.</value>
        [FormerlySerializedAs("m_ChargingClip")]
        public AudioClip chargingClip;

        /// <value>Property <c>fireClip</c> represents the audio that plays when each shot is fired.</value>
        [FormerlySerializedAs("m_FireClip")]
        public AudioClip fireClip;

        /// <value>Property <c>minLaunchForce</c> represents the force given to the shell if the fire button is not held.</value>
        [FormerlySerializedAs("m_MinLaunchForce")]
        public float minLaunchForce = 15f;

        /// <value>Property <c>maxLaunchForce</c> represents the force given to the shell if the fire button is held for the max charge time.</value>
        [FormerlySerializedAs("m_MaxLaunchForce")]
        public float maxLaunchForce = 30f;

        /// <value>Property <c>maxChargeTime</c> represents how long the shell can charge for before it is fired at max force.</value>
        [FormerlySerializedAs("m_MaxChargeTime")]
        public float maxChargeTime = 0.75f;

        /// <value>Property <c>m_CurrentLaunchForce</c> represents the force that will be given to the shell when the fire button is released.</value>
        private float m_CurrentLaunchForce;

        /// <value>Property <c>m_ChargeSpeed</c> represents how fast the launch force increases, based on the max charge time.</value>
        private float m_ChargeSpeed;

        /// <value>Property <c>m_PlayerInput</c> represents the player input.</value>
        private PlayerInput m_PlayerInput;

        /// <value>Property <c>m_Fired</c> represents if any shell has been fired.</value>
        private bool m_Fired;

        /// <value>Property <c>m_AltFired</c> represents if the alternative shell has been fired.</value>
        private bool m_AltFired;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            m_PlayerInput = GetComponent<PlayerInput>();
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = minLaunchForce;
            aimSlider.value = minLaunchForce;
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void Update()
        {
            // The slider should have a default value of the minimum launch force.
            aimSlider.value = minLaunchForce;

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= maxLaunchForce && (FireButton(1)))
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = maxLaunchForce;
                Fire();
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (FireButton(0))
            {
                // ... reset the fired flag and reset the launch force
                m_Fired = false;
                m_CurrentLaunchForce = minLaunchForce;

                // Change the clip to the charging clip and start it playing.
                shootingAudio.clip = chargingClip;
                shootingAudio.Play();
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (FireButton(1) && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
                aimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (FireButton(2) && !m_Fired)
            {
                // ... launch the shell.
                Fire();
            }
        }

        /// <summary>
        /// Method <c>Fire</c> fires the shell.
        /// </summary>
        private void Fire()
        {
            // Set the fired flag so only Fire is only called once
            m_Fired = true;

            // Create an instance of the shell and store a reference to it's rigidbody.
            var position = fireTransform.position;
            var rotation = fireTransform.rotation;
            var shellInstance = m_AltFired switch
            {
                true => Instantiate(altShell, position, rotation),
                _ => Instantiate(shell, position, rotation)
            };

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * fireTransform.forward;
            if (m_AltFired) shellInstance.velocity *= 1.5f;

            // Change the clip to the firing clip and play it.
            shootingAudio.clip = fireClip;
            shootingAudio.Play();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = minLaunchForce;

            // Reset the alt fire flag.
            m_AltFired = false;
        }

        /// <summary>
        /// Method <c>FireButton</c> returns whether or not the fire button has been pressed.
        /// </summary>
        private bool FireButton(int mode)
        {
            // Set the firing and alt firing flags.
            var firing = false;
            var altfiring = false;

            // Get the fire and alt fire actions.
            var fireAction = m_PlayerInput.actions["Fire"];
            var altFireAction = m_PlayerInput.actions["AltFire"];

            // Check if it's and alt fire.
            m_AltFired = altFireAction.IsPressed() || altFireAction.WasPressedThisFrame() || altFireAction.WasReleasedThisFrame();

            switch (mode) {
                case 0:
                    firing = fireAction.WasPressedThisFrame() && fireAction.IsPressed();
                    altfiring = altFireAction.WasPressedThisFrame() && altFireAction.IsPressed();
                    break;
                case 1:
                    firing = fireAction.IsPressed();
                    altfiring = altFireAction.IsPressed();
                    break;
                case 2:
                    firing = fireAction.WasReleasedThisFrame();
                    altfiring = altFireAction.WasReleasedThisFrame();
                    break;
            }
            return firing || altfiring;
        }
    }
}