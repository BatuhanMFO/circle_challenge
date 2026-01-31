using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class DrawLine : MonoBehaviour
{
    public SimilarityManager similarityManager;

    [Header("Hassasiyet")]
    [Tooltip("Uçuyorsa bunu 0.1 veya 0.15 yap kanka.")]
    public float minDistance = 0.08f;

    [HideInInspector] public List<Vector2> drawnPoints = new List<Vector2>();
    private LineRenderer lr;
    private Camera cam;
    private bool isDrawing;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        cam = Camera.main;
        lr.positionCount = 0;
        lr.useWorldSpace = true;
    }

    void Update()
    {
        if (Pointer.current == null || cam == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            isDrawing = true;
            lr.positionCount = 0;
            drawnPoints.Clear();
            if (similarityManager != null) similarityManager.ResetProgress();
            AddPoint();
        }

        if (isDrawing && Pointer.current.press.isPressed)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0f;
            Vector2 p2 = new Vector2(worldPos.x, worldPos.y);

            // Sadece parmak belirli bir mesafe kat ettiðinde yeni nokta ekle (Hassasiyet freni)
            if (drawnPoints.Count == 0 || Vector2.Distance(drawnPoints[^1], p2) > minDistance)
            {
                drawnPoints.Add(p2);
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, worldPos);

                if (similarityManager != null) similarityManager.Recalculate();
            }
        }

        if (Pointer.current.press.wasReleasedThisFrame) isDrawing = false;
    }

    void AddPoint()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        worldPos.z = 0f;
        drawnPoints.Add(new Vector2(worldPos.x, worldPos.y));
        lr.positionCount = 1;
        lr.SetPosition(0, worldPos);
    }
}