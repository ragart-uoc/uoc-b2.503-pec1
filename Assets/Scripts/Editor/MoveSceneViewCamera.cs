using UnityEditor;
using UnityEngine;

namespace PEC1.Editor
{
    /// <summary>
    /// Class <c>MoveSceneViewCamera</c> adds a menu item to the Window menu to move the Scene View camera to a specific position.
    /// </summary>
    public abstract class MoveSceneViewCamera
    {
        /// <summary>
        /// Method <c>PositionCamera</c> moves the Scene View camera to a specific position.
        /// </summary>
        [MenuItem("Window/Position Scene View Camera")]
        private static void PositionCamera()
        {
            SceneView.lastActiveSceneView.pivot = new Vector3(-147f, 23.5f, 237f);
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(0f, 150f, 0f);
            SceneView.lastActiveSceneView.orthographic = true;
            SceneView.lastActiveSceneView.size = 100f;
            if (Camera.main != null) Selection.activeGameObject = Camera.main.gameObject;
        }
    }
}