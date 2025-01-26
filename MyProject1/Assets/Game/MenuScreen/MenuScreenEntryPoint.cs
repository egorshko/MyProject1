using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.MenuScreen
{
    public enum MenuScreenType
    {
        Vertical,
        Horizontal,
    }

    public class MenuScreenEntryPoint : BaseDisposableMB
    {
        [Serializable]
        public struct Data 
        {
            [SerializeField] private CanvasGroup _group;
            [SerializeField] private TMP_Text _header;
            [SerializeField] private GridLayoutGroup _layoutGroup;
            [SerializeField] private Button _backgroundButton;
            [SerializeField] private Button _defaultButton;

            public readonly CanvasGroup Group => _group;
            public readonly TMP_Text Header => _header;
            public readonly GridLayoutGroup LayoutGroup => _layoutGroup;
            public readonly Button BackgroundButton => _backgroundButton;
            public readonly Button DefaultButton => _defaultButton;
        }

        public struct Ctx 
        {
            public IReadOnlyReactiveCommand<float> OnUpdate;
            public IReactiveValue<bool> IsShowMenu;
        }

        [SerializeField] private Data _data;

        private Ctx _ctx;

        private MenuScreenEntity _menuScreenEntity;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _menuScreenEntity = new MenuScreenEntity(new MenuScreenEntity.Ctx 
            {
                OnUpdate = _ctx.OnUpdate,
                Data = _data,
                IsShowMenu = _ctx.IsShowMenu,
            }).AddTo(this);
        }

        public void SetButtons(MenuScreenType screenType, string header, Action onBackgroundClick, params (string header, Action onClick)[] buttons)
            => _menuScreenEntity.SetButtons(screenType, header, onBackgroundClick, buttons);

        public void Hide() => _menuScreenEntity.Hide();
    }
}

