using UnityEditor;
using UnityEngine;

namespace Indie
{
    [CustomEditor(typeof(SceneTransitionManager))]
    public class SceneTransitionManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SceneTransitionManager manager = (SceneTransitionManager)target;

            GUILayout.Space(10);
            GUILayout.Label("Scene Management", EditorStyles.boldLabel);

            // Display each scene with a checkbox for load/unload
            foreach (var sceneStatus in manager.scenesInBuild)
            {
                bool newStatus = EditorGUILayout.Toggle(sceneStatus.sceneName, sceneStatus.isLoaded);
                if (newStatus != sceneStatus.isLoaded)
                {
                    sceneStatus.isLoaded = newStatus;
                    EditorUtility.SetDirty(manager); // Mark the manager as changed
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Apply Scene Load/Unload"))
            {
                SceneTransitionManager.LoadScenesByStatus();
            }
        }
    }
}
