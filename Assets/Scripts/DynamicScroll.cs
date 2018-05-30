using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using pooling;

namespace dynamicscroll
{
    public class DynamicScroll<T, T1> 
        where T : class
        where T1 : DynamicScrollObject<T>
    {
        public const float CONTENT_OFFSET_FIXER_LIMIT = 1000f;
        public float spacing = 15f;
        public readonly Pooling<T1> objectPool = new Pooling<T1>();
        public Action<Vector2> OnDragEvent;

        private ScrollRect mScrollRect;
        private VerticalLayoutGroup mVerticalLayoutGroup;
        private HorizontalLayoutGroup mHorizontalLayoutGroup;
        private GridLayoutGroup mGridLayoutGroup;
        private ContentSizeFitter mContentSizeFitter;

        private bool mIsVertical = false;
        private bool mIsHorizontal = false;
            
        private Vector2 mNewAnchoredPosition = Vector2.zero;
		private T[] mInfoList;

        private int mInitialAmount;

        public void Initiate(ScrollRect scrollRect, T[] infoList, int startIndex, GameObject objReference, int initialAmount, bool createMoreIfNeeded = true)
        {
            mScrollRect = scrollRect;
            if (mScrollRect == null)
                throw new Exception("No scroll rect in gameObject.");

            if (objReference == null)
                throw new Exception("No Reference GameObject setted.");
            
            mInitialAmount = initialAmount;

            mInfoList = infoList;
            
            mScrollRect.onValueChanged.AddListener(OnScroll);
            mScrollRect.movementType = ScrollRect.MovementType.Unrestricted;

            if (mScrollRect.content.GetComponent<VerticalLayoutGroup>() != null)
            {
                mVerticalLayoutGroup = mScrollRect.content.GetComponent<VerticalLayoutGroup>();
                mVerticalLayoutGroup.spacing = spacing;
            }

            if (mScrollRect.content.GetComponent<HorizontalLayoutGroup>() != null)
            {
                mHorizontalLayoutGroup = mScrollRect.content.GetComponent<HorizontalLayoutGroup>();
                mHorizontalLayoutGroup.spacing = spacing;
            }

            if (mScrollRect.content.GetComponent<GridLayoutGroup>() != null)
            {
                mGridLayoutGroup = mScrollRect.content.GetComponent<GridLayoutGroup>();
                mGridLayoutGroup.spacing = new Vector2(spacing, spacing);
            }

            if (mScrollRect.content.GetComponent<ContentSizeFitter>() != null)
                mContentSizeFitter = mScrollRect.content.GetComponent<ContentSizeFitter>();
         
            mIsHorizontal = mScrollRect.horizontal;
            mIsVertical = mScrollRect.vertical;

            objectPool.createMoreIfNeeded = createMoreIfNeeded;
			objectPool.Initialize(Mathf.Min(initialAmount, infoList.Length), objReference, mScrollRect.content);

			CreateList(mInfoList, startIndex);
            
			DisableGridComponents();

            objectPool.ForEach(x => x.SetRefreshListAction(RefreshPosition));

            if (!mIsHorizontal || !mIsVertical) return;
            Debug.LogError("DynamicScroll doesn't support scrolling in both directions, please choose one direction (horizontal or vertical)");
            mIsHorizontal = false;
        }

        //if startIndex = -1, it will keep the same position
		public void ChangeList(T[] infoList, int startIndex = -1, bool resetContentPosition = false)
        {
            if (startIndex == -1)
                startIndex = GetHighest().currentIndex;

            mScrollRect.StopMovement();
			mScrollRect.content.anchoredPosition = Vector2.zero;

            var objs = objectPool.GetAllWithState(true);
            objs.ForEach(x => objectPool.Release(x));
            if(resetContentPosition)
                mScrollRect.content.anchoredPosition = new Vector2((mIsHorizontal ? spacing : 0), (mIsVertical ? spacing : 0));
            
            mInfoList = infoList;

			CreateList(infoList, startIndex);
        }

        private void CreateList(T[] infoList, int startIndex)
		{
			float totalSize = 0f;
			int toCreate = Mathf.Min(mInitialAmount, infoList.Length);
            var lastObjectPosition = Vector2.zero;
            for (var i = 0; i < toCreate; i++)
            {
				if (startIndex + i >= infoList.Length)
					break;
                var obj = objectPool.Collect();
				obj.updateScrollObject(mInfoList[startIndex + i], startIndex + i);
                var rect = obj.GetComponent<RectTransform>();
                var posX = i > 0 ? lastObjectPosition.x + (mIsHorizontal ? +spacing : 0) : 0;
                var posY = i > 0 ? lastObjectPosition.y - (mIsVertical ? spacing : 0) : 0;
                rect.anchoredPosition = new Vector2(posX, posY);
                lastObjectPosition = new Vector2(posX + (mIsHorizontal ? obj.currentWidth : 0), posY - (mIsVertical ? obj.currentHeight : 0));

                totalSize += (mIsVertical) ? obj.currentHeight : obj.currentWidth;
            }

            totalSize = (totalSize / (float)toCreate) * infoList.Length;
            bool canDrag = (mIsHorizontal && totalSize > mScrollRect.content.rect.width) || (mIsVertical && totalSize > mScrollRect.content.rect.height);
            ToggleScroll(canDrag);
		}

        public void RefreshPosition()
        {
            var lastObject = GetHighest();
            var objs = objectPool.GetAllWithState(true);
            var index = lastObject.currentIndex;
			float totalSize = 0f;

            for (var i = 0; i < objs.Count; i++)
            {
                var currentObject = objectPool.Find(x => x.currentIndex == index);
                if (currentObject != null && currentObject.isUsing && currentObject.CompareTo(lastObject) != 0)
                {
                    var no = currentObject.GetComponent<RectTransform>();
                    var lo = lastObject.GetComponent<RectTransform>();
                    var x = (mIsHorizontal ? lo.anchoredPosition.x + lastObject.currentWidth + spacing : no.anchoredPosition.x);
                    var y = (mIsVertical ? lo.anchoredPosition.y - lastObject.currentHeight - spacing : no.anchoredPosition.y);
                    no.anchoredPosition = new Vector2(x, y);
					totalSize += mIsHorizontal ? lastObject.currentWidth : lastObject.currentHeight;
                    lastObject = currentObject;
                }

                index++;
            }

            bool canDrag = (mIsHorizontal && totalSize > mScrollRect.viewport.rect.width) || (mIsVertical && totalSize > mScrollRect.viewport.rect.height);
			ToggleScroll(canDrag);
        }

        public void ToggleScroll(bool active)
        {
            mScrollRect.enabled = active;
            mScrollRect.viewport.anchorMin = new Vector2(0, 0);
            mScrollRect.viewport.anchorMax = new Vector2(1, 1);
            mScrollRect.viewport.offsetMin = new Vector2(0, 0);
            mScrollRect.viewport.offsetMax = new Vector2(0, 0);
            mScrollRect.viewport.pivot = new Vector2(0.5f, 0.5f);

            if (!active)
				mScrollRect.content.anchoredPosition = Vector2.zero;
        }

        private void DisableGridComponents()
        {
            if (mVerticalLayoutGroup != null)
                mVerticalLayoutGroup.enabled = false;

            if (mHorizontalLayoutGroup != null)
                mHorizontalLayoutGroup.enabled = false;

            if (mContentSizeFitter != null)
                mContentSizeFitter.enabled = false;

            if (mGridLayoutGroup != null)
                mGridLayoutGroup.enabled = false;
        }

        private void OnScroll(Vector2 pos)
        {
			if(OnDragEvent != null)
                OnDragEvent.Invoke(mScrollRect.velocity);
			
			if (mIsVertical && Mathf.Abs(mScrollRect.content.anchoredPosition.y) > CONTENT_OFFSET_FIXER_LIMIT)
            {
                var v = (mScrollRect.content.anchoredPosition.y > 0 ? -CONTENT_OFFSET_FIXER_LIMIT : CONTENT_OFFSET_FIXER_LIMIT);
                mScrollRect.content.anchoredPosition = new Vector2(mScrollRect.content.anchoredPosition.x, mScrollRect.content.anchoredPosition.y + v);
                RectTransform objRectTransform;
                Vector2 objAnchoredPos;
                objectPool.GetAllWithState(true).ForEach(x =>
                {
                    objRectTransform = x.GetComponent<RectTransform>();
                    objAnchoredPos.x = objRectTransform.anchoredPosition.x;
                    objAnchoredPos.y = objRectTransform.anchoredPosition.y - v;
                    objRectTransform.anchoredPosition = objAnchoredPos;
                });
            }

            if (mIsHorizontal && Mathf.Abs(mScrollRect.content.anchoredPosition.x) > CONTENT_OFFSET_FIXER_LIMIT)
            {
                var v = (mScrollRect.content.anchoredPosition.x > 0 ? -CONTENT_OFFSET_FIXER_LIMIT : CONTENT_OFFSET_FIXER_LIMIT);
                mScrollRect.content.anchoredPosition = new Vector2(mScrollRect.content.anchoredPosition.x + v, mScrollRect.content.anchoredPosition.y);
                RectTransform objRectTransform;
                Vector2 objAnchoredPos;
                objectPool.GetAllWithState(true).ForEach(x =>
                {
                    objRectTransform = x.GetComponent<RectTransform>();
                    objAnchoredPos.x = objRectTransform.anchoredPosition.x - v;
                    objAnchoredPos.y = objRectTransform.anchoredPosition.y;
                    objRectTransform.anchoredPosition = objAnchoredPos;
                });
            }

            for (var i = 0; i < objectPool.Count; i++)
            {
				if(!objectPool[i].isUsing) continue;

                var rectTransform = objectPool[i].GetComponent<RectTransform>();
                
                if (mIsHorizontal)
                {
                    var objPosX = rectTransform.anchoredPosition.x + mScrollRect.content.anchoredPosition.x;

					if (mScrollRect.velocity.x > 0 && objPosX > mScrollRect.content.rect.width + objectPool[i].currentWidth * 0.1f)
                    {
                        var lowestObj = GetLowest();
                        var nextIndex = lowestObj.currentIndex - 1;

						if (nextIndex < 0)
                            continue;

                        objectPool.Release(objectPool[i]);

						mNewAnchoredPosition = rectTransform.anchoredPosition;

                        var obj = objectPool.Collect();
                        obj.updateScrollObject(mInfoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();
                        
                        mNewAnchoredPosition.x = lowestObj.GetComponent<RectTransform>().anchoredPosition.x - lowestObj.currentWidth - spacing;
						rectTransform.anchoredPosition = mNewAnchoredPosition;
                    }
					else if (mScrollRect.velocity.x < 0 && objPosX < objectPool[i].currentWidth * -1.1f)
                    {
                        var highestObject = GetHighest();
                        var nextIndex = highestObject.currentIndex + 1;

						if (nextIndex >= mInfoList.Length)
                            continue;

                        objectPool.Release(objectPool[i]);
						mNewAnchoredPosition = rectTransform.anchoredPosition;

                        var obj = objectPool.Collect();
                        obj.updateScrollObject(mInfoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();
                        
                        mNewAnchoredPosition.x = highestObject.GetComponent<RectTransform>().anchoredPosition.x + obj.currentWidth + spacing;                  
						rectTransform.anchoredPosition = mNewAnchoredPosition;
                    }
                }

                if (mIsVertical)
                {
                    var objPosY = rectTransform.anchoredPosition.y + mScrollRect.content.anchoredPosition.y;

                    if (mScrollRect.velocity.y > 0 && objPosY - objectPool[i].currentHeight > objectPool[i].currentHeight * 0.1f)
                    {
                        var lowestObj = GetLowest();
                        var nextIndex = lowestObj.currentIndex + 1;

						if (nextIndex >= mInfoList.Length)
                            continue;
                  
                        objectPool.Release(objectPool[i]);

						mNewAnchoredPosition = rectTransform.anchoredPosition;

                        var obj = objectPool.Collect();
                        obj.updateScrollObject(mInfoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();
                        
                        mNewAnchoredPosition.y = lowestObj.GetComponent<RectTransform>().anchoredPosition.y - lowestObj.currentHeight - spacing;
                        obj.GetComponent<RectTransform>().anchoredPosition = mNewAnchoredPosition;
                    }
					else if (mScrollRect.velocity.y < 0 && objPosY < -(mScrollRect.content.rect.height + objectPool[i].currentHeight * 0.1f))
                    {
                        var highestObject = GetHighest();
                        var nextIndex = highestObject.currentIndex - 1;

                        if (nextIndex < 0)
                            continue;
                  
                        objectPool.Release(objectPool[i]);

						mNewAnchoredPosition = rectTransform.anchoredPosition;

                        var obj = objectPool.Collect();
                        obj.updateScrollObject(mInfoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();
                        
                        mNewAnchoredPosition.y = highestObject.GetComponent<RectTransform>().anchoredPosition.y + obj.currentHeight + spacing;
                        obj.GetComponent<RectTransform>().anchoredPosition = mNewAnchoredPosition;
                    }
                }
            }
            
            LimitScroll();
        }

        private void LimitScroll()
        {
            if (mIsVertical)
            {
                if (mScrollRect.velocity.y < 0)
                {
                    //Going Down
                    var obj = GetHighest();
                    var objRect = obj.GetComponent<RectTransform>().anchoredPosition;
                    var objPosY = mScrollRect.content.anchoredPosition.y + objRect.y + spacing + mScrollRect.viewport.rect.height;
                    var diff = 0f;
                    var limit = mScrollRect.viewport.rect.height - spacing;

                    if (objPosY < limit)
                    {
                        diff = limit - objPosY;
                        mScrollRect.StopMovement();
                    }

                    mScrollRect.content.anchoredPosition = new Vector2(mScrollRect.content.anchoredPosition.x, mScrollRect.content.anchoredPosition.y + diff);
                }
                else if(mScrollRect.velocity.y > 0)
                {
                    //Going Up
                    var obj = GetLowest();
                    var objRect = obj.GetComponent<RectTransform>().anchoredPosition;
                    var objPosY = mScrollRect.content.anchoredPosition.y + objRect.y + mScrollRect.viewport.rect.height - spacing;
                    var limit = obj.currentHeight + spacing;
                    var diff = 0f;

                    if (objPosY > limit)
                    {
                        diff = limit - objPosY;
                        mScrollRect.StopMovement();
                    }

                    mScrollRect.content.anchoredPosition = new Vector2(mScrollRect.content.anchoredPosition.x, mScrollRect.content.anchoredPosition.y + diff);
                }
            }
            else if (mIsHorizontal)
            {
                if (mScrollRect.velocity.x < 0)
                {
                    //Going Left
                    var obj = GetHighest();
                    var objRect = obj.GetComponent<RectTransform>().anchoredPosition;
                    var objPosX = mScrollRect.content.anchoredPosition.x + objRect.x + spacing + mScrollRect.viewport.rect.width;
                    var diff = 0f;
                    var limit = mScrollRect.viewport.rect.width - spacing;

                    if (objPosX < limit)
                    {
                        diff = limit - objPosX;
                        mScrollRect.StopMovement();
                    }

                    mScrollRect.content.anchoredPosition = new Vector2(mScrollRect.content.anchoredPosition.x + diff, mScrollRect.content.anchoredPosition.y);
                }
                else if (mScrollRect.velocity.x > 0)
                {
                    //Going Right
                    var obj = GetLowest();
                    var objRect = obj.GetComponent<RectTransform>().anchoredPosition;
                    var objPosX = mScrollRect.content.anchoredPosition.x + objRect.x + mScrollRect.viewport.rect.width - spacing;
                    var limit = obj.currentWidth + spacing;
                    var diff = 0f;

                    if (objPosX > limit)
                    {
                        diff = limit - objPosX;
                        mScrollRect.StopMovement();
                    }

                    mScrollRect.content.anchoredPosition = new Vector2(mScrollRect.content.anchoredPosition.x + diff, mScrollRect.content.anchoredPosition.y);
                }
            }
        }

        private T1 GetLowest()
        {
            var min = float.MaxValue;
            T1 lowestObj = null;
            var objs = objectPool.GetAllWithState(true);
            foreach (var t in objs)
            {
                if (mIsVertical)
                {
                    if (t.transform.localPosition.y < min)
                    {
                        min = t.transform.localPosition.y;
                        lowestObj = t;
                    }
                }
                else if (mIsHorizontal)
                {
                    if (t.transform.localPosition.x < min)
                    {
                        min = t.transform.localPosition.x;
                        lowestObj = t;
                    }
                }
            }

            return lowestObj;
        }

        private T1 GetHighest()
        {
            var max = float.MinValue;
            T1 highestObj = null;
            var objs = objectPool.GetAllWithState(true);
            foreach (var t in objs)
            {
                if (mIsVertical)
                {
                    if (t.transform.localPosition.y > max)
                    {
                        max = t.transform.localPosition.y;
                        highestObj = t;
                    }
                }
                else if (mIsHorizontal)
                {
                    if (t.transform.localPosition.x > max)
                    {
                        max = t.transform.localPosition.x;
                        highestObj = t;
                    }
                }
            }

            return highestObj;
        }
    }
}