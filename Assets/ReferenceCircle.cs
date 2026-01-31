using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ReferenceCircle : MonoBehaviour
{
    [Header("Circle Settings")]
    public int points = 120;
    public float radius = 2f;
    public float lineWidth = 0.05f;

    [HideInInspector]
    public List<Vector2> referencePoints = new List<Vector2>();

    void update()
    {
        LineRenderer lr = GetComponent<LineRenderer>();

        // LineRenderer ayarlarý
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = points;

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.startColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        lr.endColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        referencePoints.Clear();

        // Çember çizimi + referans noktalarýný kaydet
        for (int i = 0; i < points; i++)
        {
            float angle = i * Mathf.PI * 2f / points;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            Vector2 point = new Vector2(x, y);

            lr.SetPosition(i, new Vector3(x, y, 0f));
            referencePoints.Add(point);
        }
    }
}