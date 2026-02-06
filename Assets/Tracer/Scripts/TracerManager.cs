using UnityEngine;
using System.Collections;

public class TracerManager : MonoBehaviour
{
    [Header("Tracer")]
    public GameObject tracerPrefab;             // 궤적 프리팹
    public float lifeTime = 0.1f;               // 궤적이 머무는 시간
    public float width = 0.05f;                 // 궤적 두께
    [ColorUsage(true, true)]                    // HDR 사용 가능 설정
    public Color tracerColor = Color.cyan;      // 궤적 색상

    [Header("꺾임 설정")]
    [Range(2, 20)] public int pointsCount = 10; // 꺾이는 점 개수
    public float noiseScale = 0.3f;             // 꺾이는 정도

    [Header("Points")]
    public Transform firePoint;                 // 총구
    public Transform targetPoint;               // 목표물

    // 발사 실행 메서드
    public void ShootTracer()
    {
        if (firePoint != null && targetPoint != null)
        {
            StartCoroutine(GenerateZigzagTracer(firePoint.position, targetPoint.position));
        }
    }

    // 궤적 생성 코루틴
    IEnumerator GenerateZigzagTracer(Vector3 start, Vector3 end)
    {
        // 생성 및 초기화
        GameObject tracer = Instantiate(tracerPrefab);
        LineRenderer lr = tracer.GetComponent<LineRenderer>();

        // 인스펙터 파라미터 적용
        lr.startWidth = width;
        lr.endWidth = width;

        // 색상 설정 및 페이딩
        lr.startColor = tracerColor;
        Color endColor = tracerColor;
        endColor.a = 0f;
        lr.endColor = endColor;

        // 점 개수 설정
        lr.positionCount = pointsCount;

        // 각 점의 위치 계산 루프
        for (int i = 0; i < pointsCount; i++)
        {
            // 0.0 ~ 1.0 사이의 진행 비율 계산 (정규화)
            float t = (float)i / (pointsCount - 1);

            // 직선 경로 상의 기준점 계산 (보간)
            Vector3 basePoint = Vector3.Lerp(start, end, t);

            // 시작점, 끝점을 오프셋 없이 고정
            if (i == 0 || i == pointsCount - 1)
            {
                lr.SetPosition(i, basePoint);
                continue;
            }

            // 랜덤 오프셋 벡터 생성
            // 반경 1의 구 표면 위 랜덤한 점의 벡터 반환
            Vector3 randomOffset = Random.onUnitSphere * noiseScale;

            // 진행 방향 벡터 계산
            Vector3 direction = (end - start).normalized;

            // 진행 방향 성분을 제거하여 수직에 가깝게 만듦
            randomOffset -= Vector3.Dot(randomOffset, direction) * direction;

            // 최종 위치 = 기준점 + 랜덤 오프셋
            lr.SetPosition(i, basePoint + randomOffset);
        }

        // 대기 후 삭제
        yield return new WaitForSeconds(lifeTime);
        Destroy(tracer);
    }
}