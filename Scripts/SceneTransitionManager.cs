using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Indie
{
    [System.Serializable]
    public class SceneStatus
    {
        public string sceneName;
        public bool isLoaded;
    }

    public class SceneTransitionManager : MonoBehaviour
    {
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [SerializeField] private Image fadeImage; // Optional fade image for smoother transitions
        [SerializeField] private float fadeDuration = 1f;

        [Header("Scene Management")]
        public List<SceneStatus> scenesInBuild; // List to hold all scenes in the build and their load states

        private static SceneTransitionManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
            }
            else
            {
                Destroy(gameObject);
            }

            InitializeScenesInBuild();
        }

        private void InitializeScenesInBuild()
        {
            // Get all scenes in build settings
            scenesInBuild = new List<SceneStatus>();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

                // Add scene to the list and set initial load status
                bool isLoaded = SceneManager.GetSceneByName(sceneName).isLoaded;
                scenesInBuild.Add(new SceneStatus { sceneName = sceneName, isLoaded = isLoaded });
            }
        }

        public static async void LoadScenesByStatus()
        {
            if (instance == null)
            {
                Debug.LogError("No SceneTransitionManager instance found in the scene.");
                return;
            }

            await instance.LoadOrUnloadScenes();
        }

        private async Task LoadOrUnloadScenes()
        {
            // Fade out before transitioning scenes
            if (fadeImage != null)
            {
                await Fade(1);
            }

            // Load the loading scene temporarily
            await SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

            // List to keep track of all load/unload tasks
            List<Task> sceneTasks = new List<Task>();

            foreach (var sceneStatus in scenesInBuild)
            {
                if (sceneStatus.isLoaded && !SceneManager.GetSceneByName(sceneStatus.sceneName).isLoaded)
                {
                    sceneTasks.Add(LoadSceneAsync(sceneStatus.sceneName));
                }
                else if (!sceneStatus.isLoaded && SceneManager.GetSceneByName(sceneStatus.sceneName).isLoaded)
                {
                    sceneTasks.Add(UnloadSceneAsync(sceneStatus.sceneName));
                }
            }

            // Wait for all loading/unloading tasks to complete
            await Task.WhenAll(sceneTasks);

            // Fade back in
            if (fadeImage != null)
            {
                await Fade(0);
            }

            // Unload the loading scene after transition
            await SceneManager.UnloadSceneAsync(loadingSceneName);
        }

        private async Task LoadSceneAsync(string sceneName)
        {
            var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }
        }

        private async Task UnloadSceneAsync(string sceneName)
        {
            var asyncOp = SceneManager.UnloadSceneAsync(sceneName);
            while (!asyncOp.isDone)
            {
                await Task.Yield();
            }
        }

        private async Task Fade(float targetAlpha)
        {
            if (fadeImage == null) return;

            float startAlpha = fadeImage.color.a;
            float time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                await Task.Yield();
            }

            fadeImage.color = new Color(0, 0, 0, targetAlpha);
        }
    }
}
