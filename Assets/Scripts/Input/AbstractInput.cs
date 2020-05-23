using UnityEngine;

public abstract class AbstractInput : ScriptableObject
{
    [Tooltip("How many times screen resolution is divided." + 
        " Ex. Number:3 Screen Width:1080 Screen Height:1920." +
        " Calculation: 1080/3=360  1920/3=640  (360+640)/2= 500 pixel is Drag Threshold.")]
    [SerializeField] protected float dragDetectThreshold = 3;
    [Tooltip("How many times screen resolution is divided." +
        " Ex. Number:4 Screen Width:1080 Screen Height:1920." +
        " Calculation: 1080/4=270  1920/4=480  (270+480)/2= 375 pixel is Tap Ignore Threshold." + 
        " This must be more than Drag Detect Threshold.")]
    [SerializeField] protected float tapIgnoreThreshold = 4;

    public abstract bool HasInput { get; protected set; }
    public abstract bool HasDrag { get; protected set; }
    public abstract Vector2 Position { get; protected set; }
    public abstract Vector2 DragDirection { get; protected set; }

    protected float m_DragDetectThreshold;
    protected float m_TapIgnoreThreshold;

    public void InitializeInput()
    {
        m_DragDetectThreshold = (Screen.width / dragDetectThreshold + Screen.height / dragDetectThreshold) / 2;
        m_TapIgnoreThreshold = (Screen.width / tapIgnoreThreshold + Screen.height / tapIgnoreThreshold) / 2;
    }

    public abstract void UpdateInput();
}
