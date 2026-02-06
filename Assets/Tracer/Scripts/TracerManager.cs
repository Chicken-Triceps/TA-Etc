using UnityEngine;
using System.Collections;

public class TracerManager : MonoBehaviour
{
    [Header("Tracer")]
    public GameObject tracerPrefab;             // 궤적 프리팹
    public float lifeTime = 0.2f;               // 궤적이 머무는 시간
    public float width = 0.05f;                 // 궤적 두께
    public Color tracerColor = Color.yellow;    // 궤적 색상

    [Header("Points")]
    public Transform firePoint;                 // 총구
    public Transform targetPoint;               // 목표물

    // 발사 실행 메서드
    public void ShootTracer()
    {
        if (firePoint != null && targetPoint != null)
        {
            StartCoroutine(GenerateTracer(firePoint.position, targetPoint.position));
        }
    }

    IEnumerator GenerateTracer(Vector3 start, Vector3 end)
    {
        // 생성 및 초기화
        GameObject tracer = Instantiate(tracerPrefab);
        LineRenderer lr = tracer.GetComponent<LineRenderer>();

        // 인스펙터 파라미터 적용
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = tracerColor;

        // 페이딩
        Color endColor = tracerColor;
        endColor.a = 0f;
        lr.endColor = endColor;

        // 시작-끝 위치 지정
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // 대기 후 삭제
        yield return new WaitForSeconds(lifeTime);
        Destroy(tracer);
    }
}