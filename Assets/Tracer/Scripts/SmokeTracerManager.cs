using UnityEngine;
using System.Collections;

public class SmokeTracerManager : MonoBehaviour
{
    [Header("필수 연결")]
    public GameObject smokePrefab;   // 아까 만든 Line Renderer 프리팹
    public Transform firePoint;      // 시작점 (총구)
    public Transform targetPoint;    // 끝점 (과녁)

    [Header("연기 설정")]
    public float smokeDuration = 1.0f; // 연기가 사라지는 데 걸리는 시간
    [Tooltip("왼쪽 Alpha=255, 오른쪽 Alpha=0 으로 설정하세요")]
    public Gradient fadeGradient;      // 투명해지는 애니메이션용

    // 버튼에 연결할 함수
    public void Fire()
    {
        if (firePoint != null && targetPoint != null)
        {
            StartCoroutine(SpawnSmokeTrail(firePoint.position, targetPoint.position));
        }
    }

    IEnumerator SpawnSmokeTrail(Vector3 start, Vector3 end)
    {
        // 1. 연기 프리팹 생성
        GameObject smoke = Instantiate(smokePrefab, start, Quaternion.identity);
        LineRenderer lr = smoke.GetComponent<LineRenderer>();

        // 2. 시작점과 끝점 연결
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // 3. 서서히 사라지는 효과 (Erosion 발동)
        float timer = 0f;
        while (timer < smokeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / smokeDuration; // 0.0 ~ 1.0

            // 쉐이더의 침식 효과를 위해 Alpha를 점점 줄임
            ApplyGradientFade(lr, progress);

            yield return null;
        }

        Destroy(smoke);
    }

    // 그라데이션 알파값만 깎아내리는 함수 (변경 없음)
    void ApplyGradientFade(LineRenderer lr, float progress)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colors = fadeGradient.colorKeys;
        GradientAlphaKey[] alphas = fadeGradient.alphaKeys;

        for (int i = 0; i < alphas.Length; i++)
        {
            alphas[i].alpha *= (1.0f - progress);
        }

        gradient.SetKeys(colors, alphas);
        lr.colorGradient = gradient;
    }
}