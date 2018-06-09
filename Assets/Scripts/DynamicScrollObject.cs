using System;
using pooling;
using UnityEngine;

namespace dynamicscroll
{
    //OBJECT ANCHOR MUST BE TOP LEFT
    public abstract class DynamicScrollObject<T> : PoolObject, IScrollItem, IComparable
    {
        protected Action mRefreshListAction;
		protected RectTransform mRectTransform;

        public virtual void reset() { }
        public virtual float currentHeight { get; set; }
        public virtual float currentWidth { get; set; }
        public virtual int currentIndex { get; set; }

		public RectTransform rectTransform
		{
			get
			{
				if (mRectTransform == null) mRectTransform = GetComponent<RectTransform>();
				return mRectTransform;
			}
		}

		public virtual void updateScrollObject(T item, int index)
		{
			currentIndex = index;
		}

        public virtual void SetRefreshListAction(Action action)
        {
            mRefreshListAction = action;
        }

        public int CompareTo(object obj)
        {
            if(obj is DynamicScrollObject<T>)
				return currentIndex.CompareTo(((DynamicScrollObject<T>)obj).currentIndex);

            return -1;
        }
    }
}