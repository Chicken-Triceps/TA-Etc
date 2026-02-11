using UnityEngine;
using System.Collections;

public class SmokeTracerManager : MonoBehaviour
{
    [Header("---- 프리팹 연결 ----")]
    public GameObject smokePrefab;
    public GameObject tracerPrefab;
    public Transform firePoint;
    public Transform targetPoint;

    [Header("---- [1단계] 발사 (길이 증가) ----")]
    [Tooltip("총알이 날아가는 속도")]
    public float flySpeed = 150f;

    [Header("---- [1.5단계] 유지 (박힌 후 대기) ----")]
    [Tooltip("목표물에 도달한 후 사라지기 전까지 머무는 시간 (초)")]
    public float lingerDuration = 0.2f; // [추가됨] 잠시 멈춰있는 시간

    [Header("---- [2단계] 소멸 (꼬리부터 사라짐) ----")]
    [Tooltip("Start에서 End까지 완전히 지워지는 데 걸리는 시간")]
    public float dissolveDuration = 0.5f;
    [Tooltip("연기가 사라질 때 경계면의 부드러움")]
    public float dissolveSmoothness = 0.1f;

    [Header("---- 개별 설정 ----")]
    [Range(10, 100)] public int smokeResolution = 50;
    public float smokeWidth = 0.5f;
    public float tracerWidth = 0.05f;

    [Header("---- 색상 설정 ----")]
    [ColorUsage(true, true)] public Color smokeColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
    [ColorUsage(true, true)] public Color tracerColor = Color.yellow;

    public void Fire()
    {
        if (firePoint != null && targetPoint != null)
        {
            StartCoroutine(ProcessShot(firePoint.position, targetPoint.position));
        }
    }

    IEnumerator ProcessShot(Vector3 start, Vector3 end)
    {
        // 1. 생성
        GameObject smokeObj = Instantiate(smokePrefab, start, Quaternion.identity);
        GameObject tracerObj = Instantiate(tracerPrefab, start, Quaternion.identity);

        LineRenderer smokeLr = smokeObj.GetComponent<LineRenderer>();
        LineRenderer tracerLr = tracerObj.GetComponent<LineRenderer>();

        // 2. 초기화
        SetupLine(smokeLr, smokeWidth, smokeResolution);
        SetupLine(tracerLr, tracerWidth, 2); // 탄도는 점 2개

        // 색상 및 초기 상태 설정
        smokeLr.material.SetColor("_GlowColor", smokeColor);
        UpdateSmokeDissolve(smokeLr, Color.white, 0f); // 연기 꽉 찬 상태

        tracerLr.startColor = tracerColor;
        tracerLr.endColor = tracerColor;
        tracerLr.SetPosition(0, start);
        tracerLr.SetPosition(1, start);


        // -----------------------------------------------------------------------
        // [Phase 1] 발사: 총알이 날아가며 길이가 늘어남
        // -----------------------------------------------------------------------
        float distance = Vector3.Distance(start, end);
        float currentDist = 0f;

        while (currentDist < distance)
        {
            currentDist += flySpeed * Time.deltaTime;
            Vector3 tipPosition = Vector3.MoveTowards(start, end, currentDist);

            tracerLr.SetPosition(1, tipPosition); // 탄도 머리 이동
            UpdateLinePosition(smokeLr, start, tipPosition, smokeResolution); // 연기 전체 갱신

            yield return null;
        }

        // 도착 보정 (확실하게 끝점에 고정)
        tracerLr.SetPosition(1, end);
        UpdateLinePosition(smokeLr, start, end, smokeResolution);


        // -----------------------------------------------------------------------
        // [Phase 1.5] 유지: 잠시 머무름 (Linger)
        // -----------------------------------------------------------------------
        if (lingerDuration > 0f)
        {
            yield return new WaitForSeconds(lingerDuration);
        }


        // -----------------------------------------------------------------------
        // [Phase 2] 소멸: 꼬리가 머리 쪽으로 따라가며 사라짐
        // -----------------------------------------------------------------------
        float timer = 0f;
        while (timer < dissolveDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / dissolveDuration; // 0.0 -> 1.0

            // 탄도 소멸: 시작점(0)을 끝점 방향으로 이동 (물리적 길이 감소)
            Vector3 tailPos = Vector3.Lerp(start, end, progress);
            tracerLr.SetPosition(0, tailPos);

            // 연기 소멸: 그라디언트 투명도 조절 (시각적 소멸)
            UpdateSmokeDissolve(smokeLr, Color.white, progress);

            yield return null;
        }

        // 4. 삭제
        Destroy(smokeObj);
        Destroy(tracerObj);
    }

    // ==================================================================================
    // 헬퍼 함수들 (기존과 동일)
    // ==================================================================================
    void UpdateSmokeDissolve(LineRenderer lr, Color baseColor, float cutProgress)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(baseColor, 0f), new GradientColorKey(baseColor, 1f) },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0f, Mathf.Clamp(cutProgress, 0f, 0.99f)),
                new GradientAlphaKey(1f, Mathf.Clamp(cutProgress + dissolveSmoothness, 0f, 1f)),
                new GradientAlphaKey(1f, 1f)
            }
        );
        lr.colorGradient = gradient;
    }

    void SetupLine(LineRenderer lr, float width, int count)
    {
        lr.positionCount = count;
        lr.widthMultiplier = width;
        lr.useWorldSpace = true;
        lr.textureMode = LineTextureMode.Tile;
        lr.numCornerVertices = 5;
        lr.numCapVertices = 5;
    }

    void UpdateLinePosition(LineRenderer lr, Vector3 start, Vector3 end, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            lr.SetPosition(i, Vector3.Lerp(start, end, t));
        }
    }
}