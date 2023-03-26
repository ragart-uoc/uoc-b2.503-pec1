using UnityEngine;
using UnityEngine.Serialization;

namespace PEC1.Cameras
{
    /// <summary>
    /// Class <c>CameraControl</c> is used to control the camera's movement and zoom.
    /// </summary>
    public class CameraControl : MonoBehaviour
    {
        /// <value> Property <c>dampTime</c> represents the approximate time for the camera to refocus.</value>
        [FormerlySerializedAs("m_DampTime")]
        public float dampTime = 0.2f;

        /// <value> Property <c>screenEdgeBuffer</c> represents the space between the top/bottom most target and the screen edge.</value>
        [FormerlySerializedAs("m_ScreenEdgeBuffer")]
        public float screenEdgeBuffer = 4f;

        /// <value> Property <c>minSize</c> represents the smallest orthographic size the camera can be.</value>
        [FormerlySerializedAs("m_MinSize")]
        public float minSize = 6.5f;

        /// <value> Property <c>targets</c> represents all the targets the camera needs to encompass.</value>
        [FormerlySerializedAs("m_Targets")]
        [HideInInspector] public Transform[] targets;

        /// <value> Property <c>m_Camera</c> is used for referencing the camera.</value>
        private Camera m_Camera;

        /// <value> Property <c>m_ZoomSpeed</c> represents the reference speed of the smooth damping of the orthographic size.</value>
        private float m_ZoomSpeed;

        /// <value> Property <c>m_MoveVelocity</c> represents the reference velocity for the smooth damping of the position.</value>
        private Vector3 m_MoveVelocity;

        /// <value> Property <c>m_DesiredPosition</c> represents the position the camera is moving towards.</value>
        private Vector3 m_DesiredPosition;

        /// <value> Property <c>m_SplitMode</c> represents whether or not the camera is in split mode.</value>
        private bool m_SplitMode = true;

        /// <value> Property <c>splitDistance</c> represents the distance at which the camera switches to split mode.</value>
        [FormerlySerializedAs("limitDistance")]
        public float splitDistance = 25.0f;

        /// <value> Property <c>splitHysteresis</c> represents the distance at which the camera switches back to single mode.</value>
        [FormerlySerializedAs("hysteresis")]
        public float splitHysteresis = 5.0f;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            //m_SplitMode = false;
            //m_Camera = GetComponentInChildren<Camera>();
        }

        /// <summary>
        /// Method <c>FixedUpdate</c> is called every fixed frame-rate frame, if the MonoBehaviour is enabled.
        /// </summary>
        private void FixedUpdate()
        {
            // Move the camera towards a desired position.
            //Move();

            // Change the size of the camera based.
            //if (!m_SplitMode) Zoom();

            // Switch between single and split mode.
            //Split();
        }

        /// <summary>
        /// Method <c>Move</c> is used to move the camera towards a desired target.
        /// </summary>
        private void Move()
        {
            // Find the average position of the targets.
            FindAveragePosition();

            // Smoothly transition to that position.
            transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, dampTime);
        }

        /// <summary>
        /// Method <c>FindAveragePosition</c> is used to find the average position of the targets.
        /// </summary>
        private void FindAveragePosition()
        {
            var averagePos = new Vector3();
            var numTargets = 0;

            // Go through all the targets and add their positions together.
            foreach (var target in targets)
            {
                // If the target isn't active, go on to the next one.
                if (!target.gameObject.activeSelf)
                    continue;

                // Add to the average and increment the number of targets in the average.
                averagePos += target.position;
                numTargets++;
            }

            // If there are targets divide the sum of the positions by the number of them to find the average.
            if (numTargets > 0)
                averagePos /= numTargets;

            // Keep the same y value.
            averagePos.y = transform.position.y;

            // The desired position is the average position;
            m_DesiredPosition = averagePos;
        }

        /// <summary>
        /// Method <c>Zoom</c> is used to change the size of the camera based on the targets position.
        /// </summary>
        private void Zoom()
        {
            // Find the required size based on the desired position and smoothly transition to that size.
            var requiredSize = FindRequiredSize();
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, dampTime);
        }

        /// <summary>
        /// Method <c>FindRequiredSize</c> is used to find the required size of the camera based on the targets position.
        /// </summary>
        private float FindRequiredSize()
        {
            // Find the position the camera rig is moving towards in its local space.
            var desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // Start the camera's size calculation at zero.
            var size = 0f;

            // Go through all the targets...
            foreach (var target in targets)
            {
                // ... and if they aren't active continue on to the next target.
                if (!target.gameObject.activeSelf)
                    continue;

                // Otherwise, find the position of the target in the camera's local space.
                var targetLocalPos = transform.InverseTransformPoint(target.position);

                // Find the position of the target from the desired position of the camera's local space.
                var desiredPosToTarget = targetLocalPos - desiredLocalPos;

                // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

                // Choose the largest out of the current size and the calculated size based on the tank being to the left or right of the camera.
                size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
            }

            // Add the edge buffer to the size.
            size += screenEdgeBuffer;

            // Make sure the camera's size isn't below the minimum.
            size = Mathf.Max(size, minSize);

            return size;
        }

        /// <summary>
        /// Method <c>SetStartPositionAndSize</c> is used to set the starting position and size of the camera.
        /// </summary>
        public void SetStartPositionAndSize()
        {
            // Find the desired position.
            FindAveragePosition();

            // Set the camera's position to the desired position without damping.
            transform.position = m_DesiredPosition;

            // Find and set the required size of the camera.
            m_Camera.orthographicSize = FindRequiredSize();
        }

        /// <summary>
        /// Method <c>Split</c> is used to switch between single and split mode.
        /// </summary>
        private void Split()
        {
			var distance = Vector3.Distance(targets[0].position, targets[1].position);

            // Check if last frame's update set the cullingMask to zero. If this is the case, then disable the m_Camera in the next frame (this one). More explanations below.
            if (m_Camera.enabled && m_Camera.cullingMask == 0)
            {
                m_Camera.enabled = false;
                m_Camera.gameObject.SetActive(false);
            }

            switch (m_SplitMode)
            {
                case true when distance < splitDistance - splitHysteresis:
                {
                    m_SplitMode = false;

                    // Set m_Camera's culling mask to DEFAULT to render all objects again
                    m_Camera.cullingMask = 1;
                    m_Camera.enabled = true;
                    m_Camera.gameObject.SetActive(true);

                    // Disable all other tank cameras
                    foreach (var tankCamera in Resources.FindObjectsOfTypeAll<Camera>())
                    {
                        if (!tankCamera.name.StartsWith("Camera")) continue;
                        tankCamera.enabled = false;
                        tankCamera.gameObject.SetActive(false);
                    }

                    break;
                }
                case false when distance > splitDistance + splitHysteresis:
                {
                    m_SplitMode = true;

                    // To prevent m_Camera's last frame from being shown when switching to tank cameras
                    // we set m_Camera.cullingMask to NOTHING so the camera will render no objects on the screen thus
                    // showing the default BLACK background color.

                    // We, however, cannot disable m_Camera in this FixedUpdate() frame, because doing so would still render the last frame remains
                    // on both sides of the screen. To prevent this effect, we have to disable m_Camera on the next FixedUpdate() invocation (next frame).
                    m_Camera.cullingMask = 0;

                    // Enable all other tank cameras
                    foreach (var tankCamera in Resources.FindObjectsOfTypeAll<Camera>())
                    {
                        if (!tankCamera.name.StartsWith("Camera")) continue;
                        tankCamera.enabled = true;
                        tankCamera.gameObject.SetActive(true);
                    }

                    break;
                }
            }
        }
    }
}