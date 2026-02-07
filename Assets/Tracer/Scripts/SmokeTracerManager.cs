using UnityEngine;
using System.Collections;

public class SmokeTracerManager : MonoBehaviour
{
    [Header("---- 프리팹 연결 ----")]
    public GameObject smokePrefab;   // (기존) 구겨지는 연기 프리팹
    public GameObject tracerPrefab;  // (필수) 빛나는 직선 탄도 프리팹
    public Transform firePoint;
    public Transform targetPoint;

    [Header("---- 연기 설정 (Smoke) ----")]
    public float smokeDuration = 0.5f;   // 연기 수명
    public float smokeWidth = 0.1f;      // 연기 두께
    [Range(10, 100)] public int resolution = 50; // 연기 품질 (점 개수)

    [Header("---- 탄도 설정 (Tracer) ----")]
    public float tracerDuration = 0.1f;  // 탄도 수명 (짧을수록 빠름, 0.05 ~ 0.1 추천)
    public float tracerWidth = 0.02f;    // 탄도 두께 (아주 얇게 추천)
    [ColorUsage(true, true)]
    public Color tracerColor = Color.yellow; // 탄도 색상 (HDR 지원, 인스펙터에서 밝기 조절 가능)


    public void Fire()
    {
        if (firePoint != null && targetPoint != null)
        {
            // 1. 연기 발생 (천천히, 구겨짐)
            StartCoroutine(SpawnSmoke(firePoint.position, targetPoint.position));

            // 2. 탄도 발생 (빠르게, 직선, 빛남)
            if (tracerPrefab != null)
            {
                StartCoroutine(SpawnBulletTracer(firePoint.position, targetPoint.position));
            }
        }
    }

    // ==================================================================================
    // 1. 연기 생성 로직 (기존 쉐이더용 점 배치)
    // ==================================================================================
    IEnumerator SpawnSmoke(Vector3 start, Vector3 end)
    {
        GameObject smoke = Instantiate(smokePrefab, start, Quaternion.identity);
        LineRenderer lr = smoke.GetComponent<LineRenderer>();

        lr.positionCount = resolution;
        lr.widthMultiplier = smokeWidth;
        lr.useWorldSpace = true;

        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            lr.SetPosition(i, Vector3.Lerp(start, end, t));
        }

        float timer = 0f;
        while (timer < smokeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / smokeDuration;

            // 투명도 1 -> 0
            float alpha = Mathf.Lerp(1f, 0f, progress);
            SetLineColorAlpha(lr, Color.white, alpha); // 연기는 쉐이더 색상을 따름 (White)

            yield return null;
        }
        Destroy(smoke);
    }

    // ==================================================================================
    // 2. 탄도 생성 로직 (서서히 사라지는 직선)
    // ==================================================================================
    IEnumerator SpawnBulletTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(tracerPrefab, start, Quaternion.identity);
        LineRenderer lr = tracer.GetComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, start); // 시작점 (총구)
        lr.SetPosition(1, end);   // 끝점 (과녁)

        lr.widthMultiplier = tracerWidth;
        lr.useWorldSpace = true;

        // [핵심] 복잡한 Alpha 계산 루프 삭제!
        // 탄도가 잠깐 번쩍였다가 알아서 사라지게 기다리기만 합니다.
        yield return new WaitForSeconds(tracerDuration);

        Destroy(tracer);
    }

    // 색상과 투명도를 한 번에 적용하는 함수
    void SetLineColorAlpha(LineRenderer lr, Color color, float alpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) }
        );
        lr.colorGradient = gradient;
    }
}