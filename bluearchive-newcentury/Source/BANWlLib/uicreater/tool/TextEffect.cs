#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using System.Collections;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using RimUnity.UI;
#else
using UnityEngine.UI;
#endif

public class TextEffect : MonoBehaviour
{
    private Text targetText;
    private Color originalColor;
    private Vector3 originalPos;
    private bool isShaking = false;

    public void PlayShakeEffect()
    {
        if (isShaking) return; // 防止重复播放

        targetText = GetComponent<Text>();
        if (targetText == null)
        {
            Debug.LogWarning("TextEffect: 未找到 Text 组件！");
            return;
        }

        originalColor = targetText.color;
        originalPos = transform.localPosition;

        StartCoroutine(ShakeAndRestore());
    }

    private IEnumerator ShakeAndRestore()
    {
        isShaking = true;

        float shakeDuration = 0.5f;
        float shakeMagnitude = 5f;

        targetText.color = Color.red;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        targetText.color = originalColor;

        isShaking = false;
    }
}
