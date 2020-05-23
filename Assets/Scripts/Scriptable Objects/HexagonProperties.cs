using UnityEngine;

[CreateAssetMenu(fileName = "new HexProperties", menuName = "Scriptable Objects/Hex/Create Hex Property")]
public class HexagonProperties : ScriptableObject
{
    public float rotationSpeed = 360f;
    public float moveSpeed = HexMetrics.innerRadius * 20f;
}
