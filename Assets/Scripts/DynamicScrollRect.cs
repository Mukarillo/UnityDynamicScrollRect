using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine;

namespace dynamicscroll
{
	public class DynamicScrollRectEvent : UnityEvent<PointerEventData> { }
	public class DynamicScrollRect : ScrollRect
    {
		public DynamicScrollRectEvent onEndDrag = new DynamicScrollRectEvent();
		public DynamicScrollRectEvent onBeginDrag = new DynamicScrollRectEvent();

		public MovementType realMovementType;
		public bool needElasticReturn;
		public Vector2 clampedPosition;

		private bool mDragging = false;
		private Vector2 mPointerStartLocalCursor = Vector2.zero;
        
		public override void OnBeginDrag(PointerEventData eventData)
        {
			if (onBeginDrag != null)
                onBeginDrag.Invoke(eventData);
			
			if(realMovementType != MovementType.Elastic)
			{
				base.OnBeginDrag(eventData);
				return;
			}

			mDragging = true;

			if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

			mPointerStartLocalCursor = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out mPointerStartLocalCursor);
			m_ContentStartPosition = content.anchoredPosition;

			base.OnBeginDrag(eventData);
        }

		public override void OnEndDrag(PointerEventData eventData)
		{
			mDragging = false;
			base.OnEndDrag(eventData);
			if (onEndDrag != null)
				onEndDrag.Invoke(eventData);
		}

		public override void OnDrag(PointerEventData eventData)
        {
			if (realMovementType != MovementType.Elastic)
            {
				base.OnDrag(eventData);
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

			var pointerDelta = localCursor - mPointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

			Vector2 offset = CalculateOffset(position - content.anchoredPosition);
            position += offset;
			var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            
			if (needElasticReturn)
            {
                if (offset.x != 0)
					position.x = position.x - RubberDelta(offset.x, viewBounds.size.x);
                if (offset.y != 0)
					position.y = position.y - RubberDelta(offset.y, viewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

		private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

		protected override void LateUpdate()
		{
			if (realMovementType != MovementType.Elastic)
            {
				base.LateUpdate();
                return;
            }

			if (!content)
				return;

			EnsureLayoutHasRebuilt();
			UpdateBounds();
			float deltaTime = Time.unscaledDeltaTime;
			Vector2 offset = CalculateOffset(Vector2.zero);
			if (!mDragging && (offset != Vector2.zero || velocity != Vector2.zero))
			{
				Vector2 position = content.anchoredPosition;
				Vector2 vel = velocity;

				for (int axis = 0; axis < 2; axis++)
				{
					if (offset[axis] != 0)
					{
						float speed = velocity[axis];
						position[axis] = Mathf.SmoothDamp(content.anchoredPosition[axis], content.anchoredPosition[axis] + offset[axis], ref speed, elasticity, Mathf.Infinity, deltaTime);
						if (Mathf.Abs(speed) < 1)
							speed = 0;
						vel[axis] = speed;
					}
					else if (inertia)
					{
						vel[axis] *= Mathf.Pow(decelerationRate, deltaTime);
						if (Mathf.Abs(velocity[axis]) < 1)
							vel[axis] = 0;
						position[axis] += velocity[axis] * deltaTime;
					}
					else
					{
						vel[axis] = 0;
					}
				}

				velocity = vel;

				SetContentAnchoredPosition(position);
			}

			base.LateUpdate();
		}

		private void EnsureLayoutHasRebuilt()
        {
            if (!CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

		private Vector2 CalculateOffset(Vector2 delta)
        {
			var mViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
			return InternalCalculateOffset(ref mViewBounds, ref delta);
        }

        internal Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
			if(!needElasticReturn)
                return offset;
            
			Vector2 min = new Vector2(content.anchoredPosition.x - content.rect.width / 2, (content.anchoredPosition.y - clampedPosition.y) - content.rect.height / 2);
			Vector2 max = new Vector2((content.anchoredPosition.x - clampedPosition.x) + content.rect.width / 2, content.anchoredPosition.y + content.rect.height / 2);

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
				if (min.x > viewBounds.min.x)
					offset.x = viewBounds.min.x - min.x;
				else if (max.x < viewBounds.max.x)
					offset.x = viewBounds.max.x - max.x;
            }
            
            if (vertical)
            {            
                min.y += delta.y;
                max.y += delta.y;
            
				if (max.y < viewBounds.max.y)
					offset.y = viewBounds.max.y - max.y;
				else if (min.y > viewBounds.min.y)
					offset.y = viewBounds.min.y - min.y;
            }

            return offset;
        }
	}
}
