using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SimilarityManager : MonoBehaviour
{
    [Header("References")]
    public ReferenceCircle referenceCircle;
    public DrawLine drawLine;
    public TextMeshProUGUI percentText;

    [Header("Scoring")]
    public float perfectThreshold = 90f;
    public float maxErrorForZeroPercent = 0.5f;

    [Header("Visual Smoothness")]
    [Tooltip("Puanýn týrmanma hýzý. Hala hýzlý gelirse 1-2 yap.")]
    public float lerpSpeed = 3f;

    private float targetPercent = 0f;
    private float currentVisualPercent = 0f;

    void Start() => ResetProgress();

    public void ResetProgress()
    {
        targetPercent = 0f;
        currentVisualPercent = 0f;
        UpdateUI(0);
    }

    void Update()
    {
        // Puanýn "uçmasýný" engelleyen ve rakamlarýn akýþýný saðlayan kýsým
        if (currentVisualPercent < targetPercent)
        {
            // Puaný saniyede belirli bir hýzla (lerpSpeed * 15) artýrarak izlenebilir kýlar
            currentVisualPercent = Mathf.MoveTowards(currentVisualPercent, targetPercent, Time.deltaTime * lerpSpeed * 15f);

            if (currentVisualPercent < perfectThreshold)
            {
                UpdateUI(currentVisualPercent);
            }
        }
    }

    public void Recalculate()
    {
        if (referenceCircle == null || drawLine == null) return;

        List<Vector2> pts = drawLine.drawnPoints;
        int n = pts.Count;

        // Hassasiyet freni: Ýlk 5 nokta oluþmadan hesaplama yapýp puan fýrlatmasýn
        if (n < 5) return;

        // 1. Çizilen toplam mesafe (Çizdiðinle eþdeðer artýþýn anahtarý)
        float totalDist = 0f;
        for (int i = 1; i < n; i++) totalDist += Vector2.Distance(pts[i], pts[i - 1]);

        // 2. Referans çemberin toplam çevresi
        float refRadius = referenceCircle.radius;
        float targetDist = 2f * Mathf.PI * refRadius;

        // 3. Ýlerleme oraný (Mesafe bazlý)
        float progress = Mathf.Clamp01(totalDist / targetDist);

        // 4. Çizim kalitesi
        Vector2 center;
        if (!FitCircleKasa(pts, out center)) center = referenceCircle.transform.position;

        float totalErr = 0f;
        foreach (var p in pts) totalErr += Mathf.Abs(Vector2.Distance(p, center) - refRadius);
        float quality = Mathf.Clamp01(1f - ((totalErr / n) / maxErrorForZeroPercent));

        // 5. Hesaplanan skor: (Yüzde kaç yol gittin?) * (Ne kadar doðru çizdin?)
        float finalScore = (progress * 100f) * quality;

        // Sadece artýþa izin ver
        targetPercent = Mathf.Max(targetPercent, finalScore);

        // PERFECT Kontrolü
        if (targetPercent >= perfectThreshold && progress > 0.8f)
        {
            percentText.text = "PERFECT!";
            percentText.color = Color.green;
        }
    }

    void UpdateUI(float p)
    {
        int val = Mathf.RoundToInt(p);
        percentText.text = val + "%";

        // Renk geçiþi: Kýrmýzý -> Sarý -> Yeþil
        if (p < 50f)
            percentText.color = Color.Lerp(Color.red, Color.yellow, p / 50f);
        else
            percentText.color = Color.Lerp(Color.yellow, Color.green, (p - 50f) / 50f);
    }

    bool FitCircleKasa(List<Vector2> pts, out Vector2 center)
    {
        center = Vector2.zero; int n = pts.Count;
        double sx = 0, sy = 0, sx2 = 0, sy2 = 0, sxy = 0, sx3 = 0, sy3 = 0, sx1y2 = 0, sx2y1 = 0;
        foreach (var p in pts)
        {
            double x = p.x, y = p.y; sx += x; sy += y; sx2 += x * x; sy2 += y * y; sxy += x * y;
            sx3 += x * x * x; sy3 += y * y * y; sx1y2 += x * y * y; sx2y1 += x * x * y;
        }
        double den = (n * sx2 - sx * sx) * (n * sy2 - sy * sy) - (n * sxy - sx * sy) * (n * sxy - sx * sy);
        if (System.Math.Abs(den) < 1e-12) return false;
        center = new Vector2((float)(((n * sx3 + n * sx1y2 - (sx2 + sy2) * sx) * (n * sy2 - sy * sy) - (n * sx2y1 + n * sy3 - (sx2 + sy2) * sy) * (n * sxy - sx * sy)) / (2 * den)),
                             (float)(((n * sx2 - sx * sx) * (n * sx2y1 + n * sy3 - (sx2 + sy2) * sy) - (n * sxy - sx * sy) * (n * sx3 + n * sx1y2 - (sx2 + sy2) * sx)) / (2 * den)));
        return true;
    }
}