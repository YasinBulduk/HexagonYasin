using System.Collections;
using UnityEngine;

public class SelectionVisual : MonoBehaviour
{
    public void Activate(Vector2 position, Vector2 direction)
    {
        transform.position = position;
        transform.rotation *= Quaternion.FromToRotation(transform.up, direction);
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    public IEnumerator RotateClockwiseRoutine(float rotationSpeedDegrees)
    {
        Vector3 targetDir = Quaternion.AngleAxis(-120f, Vector3.forward) * transform.up;

        while(true)
        {
            float angle = Vector2.Angle(transform.up, targetDir);
            transform.up = Vector3.RotateTowards(transform.up, targetDir, rotationSpeedDegrees * Mathf.Deg2Rad * Time.deltaTime, 0f);

            if(angle <= 0.5f)
            {
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator RotateCounterClockwiseRoutine(float rotationSpeedDegrees)
    {
        Vector3 targetDir = Quaternion.AngleAxis(120f, Vector3.forward) * transform.up;

        while (true)
        {
            float angle = Vector2.Angle(transform.up, targetDir);
            transform.up = Vector3.RotateTowards(transform.up, targetDir, rotationSpeedDegrees * Mathf.Deg2Rad * Time.deltaTime, 0f);

            if (angle <= 0.5f)
            {
                yield break;
            }

            yield return null;
        }
    }
}
