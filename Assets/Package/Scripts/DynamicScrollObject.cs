using System;
using pooling;
using UnityEngine;

namespace dynamicscroll
{
    //OBJECT ANCHOR MUST BE MID MID
    public abstract class DynamicScrollObject<T> : PoolingObject, IScrollItem, IComparable
    {
        protected Action refreshListAction;
        protected RectTransform rectTransform;

        public virtual float CurrentHeight
        {
            get => RectTransform.sizeDelta.y;
            set => RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, value);
        }

        public virtual float CurrentWidth
        {
            get => RectTransform.sizeDelta.x;
            set => RectTransform.sizeDelta = new Vector2(value, RectTransform.sizeDelta.y);
        }

        public virtual int CurrentIndex { get; set; }
        public bool IsCentralized { get; private set; }
        public Vector2 PositionInViewport { get; private set; }
        public Vector2 DistanceFromCenter { get; private set; }

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }

        public virtual void Reset() { }

        public virtual void UpdateScrollObject(T item, int index)
        {
            CurrentIndex = index;
            OnObjectIsNotCentralized();
        }

        public virtual void SetRefreshListAction(Action action)
        {
            refreshListAction = action;
        }

        public virtual void SetPositionInViewport(Vector2 position, Vector2 distanceFromCenter)
        {
            PositionInViewport = position;
            DistanceFromCenter = distanceFromCenter;
        }

        public virtual void OnObjectIsCentralized()
        {
            IsCentralized = true;
        }

        public virtual void OnObjectIsNotCentralized()
        {
            IsCentralized = false;
        }

        public int CompareTo(object obj)
        {
            if (obj is DynamicScrollObject<T> scrollObject)
                return CurrentIndex.CompareTo(scrollObject.CurrentIndex);

            return -1;
        }
    }
}