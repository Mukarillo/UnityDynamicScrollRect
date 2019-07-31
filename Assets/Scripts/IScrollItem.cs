using UnityEngine;

namespace dynamicscroll
{
    public interface IScrollItem
    {
        void Reset();
        int CurrentIndex { get; set; }
        RectTransform RectTransform { get; }
    }
}