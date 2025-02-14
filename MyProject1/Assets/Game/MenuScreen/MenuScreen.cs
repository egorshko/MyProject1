using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Game.MenuScreen
{
    public class MenuScreen : BaseDisposableMB
    {
        private class Entity : BaseDisposable
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;
                    public MenuScreen.Data Data;
                    public IReactiveValue<bool> IsShowMenu;
                }

                private Ctx _ctx;

                private Stack<GameObject> _currentButtons = new();

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _ctx.Data.DefaultButton.gameObject.SetActive(false);
                }

                public void SetButtons(MenuScreenType screenType, string header, Action onBackgroundClick, params (string header, Action onClick)[] buttons)
                {
                    while (_currentButtons.Count > 0)
                        UnityEngine.Object.Destroy(_currentButtons.Pop());

                    switch (screenType)
                    {
                        case MenuScreenType.Horizontal:
                            _ctx.Data.LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                            break;
                        case MenuScreenType.Vertical:
                            _ctx.Data.LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                            break;
                    }

                    _ctx.Data.Header.text = header;

                    _ctx.Data.Group.blocksRaycasts = true;
                    _ctx.Data.Group.alpha = 1f;

                    _ctx.Data.BackgroundButton.onClick.RemoveAllListeners();
                    _ctx.Data.BackgroundButton.onClick.AddListener(() => onBackgroundClick?.Invoke());

                    foreach (var button in buttons)
                    {
                        var currentButton = UnityEngine.Object.Instantiate(_ctx.Data.DefaultButton, _ctx.Data.DefaultButton.transform.parent);

                        var currentButtonTextArea = currentButton.GetComponentInChildren<TMP_Text>(true);
                        currentButtonTextArea.text = button.header;

                        currentButton.onClick.RemoveAllListeners();
                        currentButton.onClick.AddListener(() => button.onClick?.Invoke());

                        currentButton.gameObject.SetActive(true);

                        _currentButtons.Push(currentButton.gameObject);
                    }

                    _ctx.IsShowMenu.Value = true;

                    Time.timeScale = 0f;
                }

                public void Hide()
                {
                    while (_currentButtons.Count > 0)
                        UnityEngine.Object.Destroy(_currentButtons.Pop());

                    _ctx.Data.Group.blocksRaycasts = false;
                    _ctx.Data.Group.alpha = 0f;

                    _ctx.IsShowMenu.Value = false;

                    Time.timeScale = 1f;
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public MenuScreen.Data Data;
                public IReactiveValue<bool> IsShowMenu;
            }

            private Ctx _ctx;

            private Logic _menuScreenLogic;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _menuScreenLogic = new Logic(new Logic.Ctx
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

        public enum MenuScreenType
        {
            Vertical,
            Horizontal,
        }

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

        private Entity _menuScreenEntity;

        public void Init(Ctx ctx)
        {
            _ctx = ctx;

            _menuScreenEntity = new Entity(new Entity.Ctx
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

