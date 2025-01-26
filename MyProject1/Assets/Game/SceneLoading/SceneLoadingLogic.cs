using Shared.Disposable;
using Shared.Reactive;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.SceneLoading 
{
    public class SceneLoadingLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReactiveCommand<(string sceneName, float progress)> OnSceneLoading;
            public IReactiveCommand<string> OnSceneLoadingDone;

            public IReactiveCommand<(string sceneName, float progress)> OnSceneUnloading;
            public IReactiveCommand<string> OnSceneUnloadingDone;
        }

        private const int AwaitTicks = 100;

        private readonly Ctx _ctx;

        public SceneLoadingLogic(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async Task<T> LoadScene<T>(string sceneName) where T : MonoBehaviour
        {
            var sceneLoading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (sceneLoading != null && !sceneLoading.isDone)
            {
                await Task.Delay(AwaitTicks);
                _ctx.OnSceneLoading.Execute((sceneName, sceneLoading.progress));
            }
            _ctx.OnSceneLoadingDone.Execute(sceneName);

            var scene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(scene);

            T result = null;
            foreach (var rootObject in scene.GetRootGameObjects()) 
            {
                result = rootObject.GetComponentInChildren<T>(true);
                if (result != null) break;
            }

            return result;
        }

        public async Task UnloadScene(string sceneName)
        {
            var currentScene = SceneManager.GetSceneByName(sceneName);
            if (currentScene == null || !currentScene.isLoaded) 
            {
                _ctx.OnSceneUnloading.Execute((sceneName, 1f));
                _ctx.OnSceneUnloadingDone.Execute(sceneName);
                return;
            }

            var sceneUnloading = SceneManager.UnloadSceneAsync(sceneName);
            while (sceneUnloading != null && !sceneUnloading.isDone)
            {
                await Task.Delay(AwaitTicks);
                _ctx.OnSceneUnloading.Execute((sceneName, sceneUnloading.progress));
            }
            _ctx.OnSceneUnloadingDone.Execute(sceneName);
        }
    }
}

