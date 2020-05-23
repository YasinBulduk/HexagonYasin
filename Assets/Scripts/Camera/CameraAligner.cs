using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAligner : MonoBehaviour
{
    public HexGrid grid;
    public Vector3 centerOffset;
    [Range(0.5f, 1f)] public float edgeOffset;

    private Camera m_Cam;

    private void Awake()
    {
        m_Cam = GetComponent<Camera>();
    }

    public void Initialize(HexGrid grid)
    {
#if UNITY_EDITOR
        if (!m_Cam.orthographic)
        {
            Debug.LogError(gameObject.name + " Camera is not orthographic.");
            return;
        }
#endif

        Vector2 bottomLeft;
        Vector2 upperRight;

        grid.GetCorners(out bottomLeft, out upperRight);

        Bounds b = new Bounds();
        upperRight.x += edgeOffset;
        bottomLeft.x -= edgeOffset;

        b.Encapsulate(bottomLeft);
        b.Encapsulate(upperRight);

        float screenRatio = (float) Screen.width / (float) Screen.height;
        float targetRatio = b.size.x / b.size.y;

        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = b.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            Camera.main.orthographicSize = b.size.y / 2 * differenceInSize;
        }

        transform.position = new Vector3(b.center.x + centerOffset.x, b.center.y + centerOffset.y, -10f);
    }
}
