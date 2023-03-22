using UnityEngine;

namespace PEC1.Cameras
{
    /// <summary>
    /// Class <c>CameraFollow</c> is used to make the camera follow the player.
    /// </summary>
    public class CameraFollow : MonoBehaviour {
        
        /// <value>Property <c>m_Camera</c> represents the camera reference.</value>
        private Camera m_Camera;
        
        /// <value>Property <c>smooth</c> represents the smoothness of the camera movement.</value>
        public float smooth = 0.5f;
        
        /// <value>Property <c>limitDist</c> represents the distance limit.</value>
        public float limitDist = 20.0f;
        
        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed frame-rate frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void FixedUpdate ()
        {
            if (m_Camera == null) {
                m_Camera = GetComponentInChildren<Camera>();
                return;
            }

            Follow();
        }
        
        /// <summary>
        /// Method <c>Follow</c> is used to make the camera follow the player.
        /// </summary>
        private void Follow()
        {
            var currentDist = Vector3.Distance (transform.position, m_Camera.transform.position);

            if (currentDist > limitDist)
            {
                m_Camera.transform.position = Vector3.Lerp (m_Camera.transform.position, transform.position, Time.deltaTime * smooth);
            }
        }
    }
}