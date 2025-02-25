using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace Shared.UI 
{
    public class ScrollRectNested : ScrollRect
    {
        private ScrollRect _parentScrollRect;

        private bool _routeToParent = false;

        protected override void Start()
        {
            if (transform.parent != null)
                _parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // Always route initialize potential drag event to parent
            if (_parentScrollRect != null)
                ((IInitializePotentialDragHandler)_parentScrollRect).OnInitializePotentialDrag(eventData);
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                if (_parentScrollRect != null)
                    ((IDragHandler)_parentScrollRect).OnDrag(eventData);
            }
            else
            {
                base.OnDrag(eventData);
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
                _routeToParent = true;
            else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
                _routeToParent = true;
            else
                _routeToParent = false;

            if (_routeToParent)
            {
                if (_parentScrollRect != null)
                    ((IBeginDragHandler)_parentScrollRect).OnBeginDrag(eventData);
            }
            else
            {
                base.OnBeginDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_routeToParent)
            {
                if (_parentScrollRect != null)
                    ((IEndDragHandler)_parentScrollRect).OnEndDrag(eventData);
            }
            else
            {
                base.OnEndDrag(eventData);
            }
            _routeToParent = false;
        }
    }
}
