
using BANWlLib;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual;
using newpro;
using UnityEngine;
using Verse;
public class keyevents : MonoBehaviour
{
   
    void Start()
    {
        
    }

    /// <summary>
    /// 验证粒子预制体是否正确加载
    /// </summary>

    void Update()
    {
        if (UiMapData.uiclose)
        {
            if (Input.GetKeyDown(UnityEngine.KeyCode.Escape))
            {
                MonoComp_BackButton.instance.onback();
            }
            if (Input.GetMouseButtonDown(0)) // 0 = 左键
            {
                Vector3 worldPos = UiMapData.uiCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));
                ShopEvents.RaiseRefresh();
                worldPos.z = 0;
                SpawnParticleAtPosition(worldPos);
            }
        }
    }

    /// <summary>
    /// 在指定的世界坐标播放粒子
    /// </summary>
    /// <param name="worldPos">目标世界坐标</param>
    public void SpawnParticleAtPosition(Vector3 worldPos)
    {

        // 检查粒子预制体是否存在
        if (UiMapData.buyParticle == null)
        {
            return;
        }

        GameObject instGo = null;
        ParticleSystem particleSystem = null;

        try
        {
            var goPrefab = UiMapData.buyParticle as GameObject;
            if (goPrefab != null)
            {
                instGo = Object.Instantiate(goPrefab, worldPos, Quaternion.identity);
                particleSystem = instGo.GetComponent<ParticleSystem>() ?? instGo.GetComponentInChildren<ParticleSystem>();

                if (particleSystem == null)
                {
                    Object.Destroy(instGo);
                    return;
                }
            }
            else
            {
                return;
            }

            // 播放粒子
            instGo.SetActive(true);
            particleSystem.Clear(true);
            particleSystem.Play(true);

            // 计算销毁时间
            var main = particleSystem.main;
            float duration = main.duration;
            float startLifetime = main.startLifetime.constantMax;
            float destroyTime = duration + startLifetime + 0.5f;

            Object.Destroy(instGo, destroyTime);
        }
        catch (System.Exception ex)
        {
            Log.Error($"SpawnParticleAtPosition 发生错误: {ex.Message}\n{ex.StackTrace}");
            if (instGo != null)
            {
                Object.Destroy(instGo);
            }
        }
    }


}

