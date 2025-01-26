using Shared.Disposable;
using Shared.Reactive;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.SceneLoading
{
    public class SceneLoadingEntity : BaseDisposable
    {
        public struct Ctx
        {
            public IReactiveCommand<(string sceneName, float progress)> OnSceneLoading;
            public IReactiveCommand<string> OnSceneLoadingDone;

            public IReactiveCommand<(string sceneName, float progress)> OnSceneUnloading;
            public IReactiveCommand<string> OnSceneUnloadingDone;
        }

        private readonly Ctx _ctx;

        private SceneLoadingLogic _sceneLoadingLogic;

        public SceneLoadingEntity(Ctx ctx)
        {
            _ctx = ctx;

            _sceneLoadingLogic = new SceneLoadingLogic(new SceneLoadingLogic.Ctx
            {
                OnSceneLoading = _ctx.OnSceneLoading,
                OnSceneLoadingDone = _ctx.OnSceneLoadingDone,

                OnSceneUnloading = _ctx.OnSceneUnloading,
                OnSceneUnloadingDone = _ctx.OnSceneUnloadingDone,
            }).AddTo(this);
        }

        public async Task<T> LoadScene<T>(string sceneName) where T : MonoBehaviour 
        {
            return await _sceneLoadingLogic.LoadScene<T>(sceneName);
        }

        public async Task UnloadScene(string sceneName) 
        {
            await _sceneLoadingLogic.UnloadScene(sceneName);
        }
    }
}

