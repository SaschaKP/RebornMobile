using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileJoystick : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private RectTransform background;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private RectTransform handle;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    public float deadZone;
    [SerializeField]
    private float handleRange = 1f;
    [SerializeField]
    private bool blockVerticalInput;
    [SerializeField]
    private bool blockHorizontalInput;
    
    public Vector2 Input { get; private set; }
    
    private int pointerId = -1;
    
    public void SetSize(float size)
    {
        background.sizeDelta = Vector2.one * size;
        handle.sizeDelta = Vector2.one * size * 0.5f;
    }

    private static bool _notWorking = false;
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId)
        {
            return;
        }

        Vector2 pointerPosition;
        if (_notWorking)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, null, out pointerPosition);
        }
        else
        {
            var backgroundPosition = background.position;
            var eventDataPosition = eventData.position;
            pointerPosition = new Vector2(eventDataPosition.x - backgroundPosition.x, eventDataPosition.y - backgroundPosition.y);
            if (canvas == null || pointerPosition == null)
            {
                _notWorking = true;
                OnDrag(eventData);
                return;
            }
            pointerPosition /= canvas.scaleFactor;
        }

        var extent = background.rect.size * 0.5f * handleRange;
        
        var horizontalInput = pointerPosition.x / extent.x;
        var verticalInput = pointerPosition.y / extent.y;
        Input = new Vector2(blockHorizontalInput ? 0f : horizontalInput, blockVerticalInput ? 0f : verticalInput);

        var magnitude = Input.magnitude;
        Input = magnitude > 1.0f ? Input.normalized : Input;
        handle.localPosition = new Vector2(Input.x * extent.x, Input.y * extent.y);
        if (magnitude < deadZone)
        {
            Input = Vector2.zero;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId)
        {
            return;
        }
        
        ResetPosition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter != gameObject)
        {
            return;
        }

        pointerId = eventData.pointerId;
        
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != pointerId)
        {
            return;
        }
        
        OnEndDrag(eventData);
    }

    private void ResetPosition()
    {
        Input = Vector2.zero;
        handle.position = background.position;
    }
    
    private void OnEnable()
    {
        backgroundImage.raycastTarget = true;
    }

    private void OnDisable()
    {
        backgroundImage.raycastTarget = false;
        ResetPosition();
    }
}