using Game.City;
using Game.LoadingScreen;
using Game.MenuScreen;
using Game.Space;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Game 
{
    public class GameLogic : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;

            public Func<string, Task<LoadingScreenEntryPoint>> GetLoadingScreen;
            public Func<string, Task<MenuScreenEntryPoint>> GetMenuScreen;
            public Func<string, Task<SpaceEntryPoint>> GetSpace;
            public Func<string, Task<CityEntryPoint>> GetCity;
            public Func<string, Task> UnloadScene;
        }

        private readonly Ctx _ctx;

        public GameLogic(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async Task<LoadingScreenEntryPoint> GetLoadingScreen(LoadingScreenEntryPoint.Ctx ctx)
        {
            const string loadingScreenName = "LoadingScreen";
            var loadingScreenEntryPoint = await _ctx.GetLoadingScreen.Invoke(loadingScreenName);
            loadingScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(loadingScreenName)).AddTo(this);
            return loadingScreenEntryPoint;
        }

        public async Task<MenuScreenEntryPoint> GetMenuScreen(MenuScreenEntryPoint.Ctx ctx)
        {
            const string menuScreenName = "MenuScreen";
            var menuScreenEntryPoint = await _ctx.GetMenuScreen.Invoke(menuScreenName);
            menuScreenEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(menuScreenName)).AddTo(this);
            return menuScreenEntryPoint;
        }

        public async Task<SpaceEntryPoint> GetSpace(SpaceEntryPoint.Ctx ctx)
        {
            const string spaceName = "Space";
            var spaceEntryPoint = await _ctx.GetSpace.Invoke(spaceName);
            spaceEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(spaceName)).AddTo(this);
            return spaceEntryPoint;
        }

        private async Task UnloadSpace() 
        {
            const string spaceName = "Space";
            await UnloadScene(spaceName);
        }

        public async Task<CityEntryPoint> GetCity(CityEntryPoint.Ctx ctx)
        {
            const string cityName = "City";
            var cityEntryPoint = await _ctx.GetCity.Invoke(cityName);
            cityEntryPoint.Init(ctx);
            new DisposeObserver(async () => await UnloadScene(cityName)).AddTo(this);
            return cityEntryPoint;
        }

        private async Task UnloadCity()
        {
            const string cityName = "City";
            await UnloadScene(cityName);
        }

        public async Task UnloadAll()
        {
            await UnloadSpace();
            await UnloadCity();
        }

        private async Task UnloadScene(string sceneName) 
        {
            await _ctx.UnloadScene(sceneName);
        }
    }
}

