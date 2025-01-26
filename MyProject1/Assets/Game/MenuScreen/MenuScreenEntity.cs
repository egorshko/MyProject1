using Shared.Disposable;
using Shared.Reactive;
using System;

namespace Game.MenuScreen 
{
    public class MenuScreenEntity : BaseDisposable
    {
        public struct Ctx
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public MenuScreenEntryPoint.Data Data;
            public IReactiveValue<bool> IsShowMenu;
        }

        private Ctx _ctx;

        private MenuScreenLogic _menuScreenLogic;

        public MenuScreenEntity(Ctx ctx)
        {
            _ctx = ctx;

            _menuScreenLogic = new MenuScreenLogic(new MenuScreenLogic.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,
                Data = _ctx.Data,
                IsShowMenu = _ctx.IsShowMenu,
            }).AddTo(this);
        }

        public void SetButtons(MenuScreenType screenType, string header, Action onBackgroundClick, params (string header, Action onClick)[] buttons)
        {
            _menuScreenLogic.SetButtons(screenType, header, onBackgroundClick, buttons);
        }

        public void Hide()
        {
            _menuScreenLogic.Hide();
        }
    }
}

