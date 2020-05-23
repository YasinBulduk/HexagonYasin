using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Touch Input", menuName = "Scriptable Objects/Input/Touch")]
public class TouchInput : AbstractInput
{
    public override bool HasInput { get; protected set; }
    public override bool HasDrag { get; protected set; }
    public override Vector2 Position { get; protected set; }
    public override Vector2 DragDirection { get; protected set; }

    private Vector2 m_DragStartPosition = -Vector2.one;
    private bool m_IgnoreTap = false;

    public override void UpdateInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Position = touch.position;

            if (touch.phase == TouchPhase.Began)
            {
                m_DragStartPosition = touch.position;
                m_IgnoreTap = false;
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    m_DragStartPosition = -Vector2.one;
                    m_IgnoreTap = true;
                    return;
                }
            }
            else if (touch.phase == TouchPhase.Moved && m_DragStartPosition != -Vector2.one)
            {
                Vector2 currentPosition = touch.position;
                Vector2 dragDirection = currentPosition - m_DragStartPosition;
                float dragSqrMag = dragDirection.sqrMagnitude;

                if (dragSqrMag > m_TapIgnoreThreshold * m_TapIgnoreThreshold)
                {
                    m_IgnoreTap = true;
                }

                if (dragSqrMag > m_DragDetectThreshold * m_DragDetectThreshold)
                {
                    DragDirection = dragDirection.normalized;
                    HasDrag = true;
                    m_DragStartPosition = -Vector2.one;
                }
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                if (!m_IgnoreTap)
                {
                    HasInput = true;
                    m_IgnoreTap = false;
                }

                m_DragStartPosition = -Vector2.one;
            }
            else
            {
                HasDrag = false;
                DragDirection = Vector2.zero;
            }
        }
        else
        {
            HasInput = false;
            HasDrag = false;
            DragDirection = Vector2.zero;
        }
    }
}
