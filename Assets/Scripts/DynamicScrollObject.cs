using System;
using pooling;

namespace dynamicscroll
{
    //OBJECT ANCHOR MUST BE TOP LEFT
    public abstract class DynamicScrollObject<T> : PoolObject, IScrollItem, IComparable
    {
        protected Action mRefreshListAction;

        public virtual void reset() { }
        public virtual float currentHeight { get; set; }
        public virtual float currentWidth { get; set; }
        public virtual int currentIndex { get; set; }

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