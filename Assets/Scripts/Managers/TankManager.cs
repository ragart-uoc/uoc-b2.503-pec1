using UnityEngine;
using PEC1.Entities;

namespace PEC1.Managers
{
    public class TankManager : MonoBehaviour
    {
        /// <value>Property <c>_instance</c> represents the singleton instance of the class.</value>
        private static TankManager _instance;

        /// <value>Property <c>tanks</c> is a collection of managers for enabling and disabling different aspects of the tanks.</value>
        [SerializeField]
        private Tank[] tanks;

        /// <summary>
        /// Method <c>GetTanks</c> returns the list of tanks.
        /// </summary>
        public Tank[] GetTanks()
        {
            return tanks;
        }
    }
}
