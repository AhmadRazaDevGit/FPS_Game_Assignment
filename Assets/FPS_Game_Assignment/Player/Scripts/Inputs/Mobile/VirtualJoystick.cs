using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    [Tooltip("RectTransform of the joystick background (usually the parent). Used to convert screen pointer to local coordinates.")]
    [SerializeField] private RectTransform background;

    [Tooltip("RectTransform of the joystick knob (movable thumb). This RectTransform's anchoredPosition is moved as the user drags.")]
    [SerializeField] private RectTransform knob;

    [Header("Settings")]
    [Tooltip("Maximum distance in pixels the knob can move from the center. Output is clamped and normalized by this value to produce (-1..1) range.")]
    [SerializeField] private float handleRange = 50f; // px

    private Vector2 _pointerDownPos;
    private bool _dragging;
    private Vector2 _output; // cached output

    public Vector2 Output => _output; // (-1..1) in x,y

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDownPos = ScreenPointToLocal(eventData);
        _dragging = true;
        UpdateDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging) return;
        UpdateDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
        _output = Vector2.zero;
        if (knob != null) knob.anchoredPosition = Vector2.zero;
    }

    private void UpdateDrag(PointerEventData eventData)
    {
        Vector2 local = ScreenPointToLocal(eventData);
        Vector2 delta = local - _pointerDownPos;
        Vector2 clamped = Vector2.ClampMagnitude(delta, handleRange);
        if (knob != null) knob.anchoredPosition = clamped;
        _output = clamped / handleRange;
    }

    // Use the eventData's pressEventCamera so the correct camera is used (works with Screen Space - Camera / World Space canvases)
    private Vector2 ScreenPointToLocal(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        return localPoint;
    }
}
