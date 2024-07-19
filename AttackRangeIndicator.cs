using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AttackRangeIndicator : MonoBehaviour
{
    public float radius = 5f; // Adjust this value to set the attack range
    public int segments = 50; // Number of segments for the circle
    public Color indicatorColor = Color.red; // Color of the indicator

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        DrawCircle();
    }

    private void DrawCircle()
    {
        float angle = 0f;
        float angleStep = 360f / segments;
        for (int i = 0; i < segments + 1; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
            angle += angleStep;
        }
    }

    private void Update()
    {
        lineRenderer.startColor = indicatorColor;
        lineRenderer.endColor = indicatorColor;
    }
}
