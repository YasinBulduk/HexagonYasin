using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Mouse Input", menuName = "Scriptable Objects/Input/Mouse")]
public class MouseInput : AbstractInput
{
    public override bool HasInput { get; protected set; }
    public override bool HasDrag { get; protected set; }
    public override Vector2 DragDirection { get; protected set; }
    public override Vector2 Position { get; protected set; }

    private Vector2 m_DragStartPosition = -Vector2.one;
    private bool m_IgnoreTap = false;

    public override void UpdateInput()
    {
        Position = Input.mousePosition;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_DragStartPosition = Input.mousePosition;
            m_IgnoreTap = false;
        }
        else
        {
            HasInput = false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!m_IgnoreTap)
            {
                HasInput = true;
                m_IgnoreTap = false;
            }

            m_DragStartPosition = -Vector2.one;
        }


        if (m_DragStartPosition != -Vector2.one)
        {
            Vector2 currentPosition = Input.mousePosition;
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
        else
        {
            DragDirection = Vector2.zero;
            HasDrag = false;
        }
    }
}