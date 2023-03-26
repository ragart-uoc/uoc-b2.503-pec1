using UnityEngine;

namespace PEC1.Entities
{
    /// <summary>
    /// Class <c>Tank</c> represents a player.
    /// </summary>
    public class Player
    {
        /// <value>Property <c>number</c> represents the player number.</value>
        public readonly int number;

        /// <value>Property <c>tank</c> represents the tank.</value>
        public readonly Tank tank;

        /// <value>Property <c>camera</c> represents the camera of the player.</value>
        public GameObject camera;

        /// <summary>
        /// Method <c>Player</c> is the constructor of the class.
        /// </summary>
        /// <param name="playerNumber">The player number.</param>
        /// <param name="tankInstance">The tank.</param>
        public Player(int playerNumber, Tank tankInstance)
        {
            number = playerNumber;
            tank = tankInstance;
        }
    }
}
