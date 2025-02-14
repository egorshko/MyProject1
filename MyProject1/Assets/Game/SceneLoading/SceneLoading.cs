using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.SceneLoading 
{
    public class SceneLoading
    {
        public class Entity : BaseDisposable
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReactiveCommand<(string sceneName, float progress)> OnSceneLoading;
                    public IReactiveCommand<string> OnSceneLoadingDone;

                    public IReactiveCommand<(string sceneName, float progress)> OnSceneUnloading;
                    public IReactiveCommand<string> OnSceneUnloadingDone;
                }

                private readonly Ctx _ctx;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;
                }

                public async UniTask<T> LoadScene<T>(string sceneName) where T : MonoBehaviour
                {
                    var sceneLoading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    while (sceneLoading != null && !sceneLoading.isDone)
                    {
                        var delayMs = 50;
                        await UniTask.Delay(delayMs, true);
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

                public async UniTask UnloadScene(string sceneName)
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
                        var delayMs = 50;
                        await UniTask.Delay(delayMs, true);
                        _ctx.OnSceneUnloading.Execute((sceneName, sceneUnloading.progress));
                    }
                    _ctx.OnSceneUnloadingDone.Execute(sceneName);
                }
            }

            public struct Ctx
            {
                public IReactiveCommand<(string sceneName, float progress)> OnSceneLoading;
                public IReactiveCommand<string> OnSceneLoadingDone;

                public IReactiveCommand<(string sceneName, float progress)> OnSceneUnloading;
                public IReactiveCommand<string> OnSceneUnloadingDone;
            }

            private readonly Ctx _ctx;

            private Logic _sceneLoadingLogic;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _sceneLoadingLogic = new Logic(new Logic.Ctx
                {
                    OnSceneLoading = _ctx.OnSceneLoading,
                    OnSceneLoadingDone = _ctx.OnSceneLoadingDone,

                    OnSceneUnloading = _ctx.OnSceneUnloading,
                    OnSceneUnloadingDone = _ctx.OnSceneUnloadingDone,
                }).AddTo(this);
            }

            public async UniTask<T> LoadScene<T>(string sceneName) where T : MonoBehaviour
            {
                return await _sceneLoadingLogic.LoadScene<T>(sceneName);
            }

            public async UniTask UnloadScene(string sceneName)
            {
                await _sceneLoadingLogic.UnloadScene(sceneName);
            }
        }
    }
}

