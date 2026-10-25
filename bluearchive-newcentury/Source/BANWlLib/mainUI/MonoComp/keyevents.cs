using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;

using BANWlLib;
using BANWlLib.mainUI.MonoComp;
using BANWlLib.mainUI.StudentManual;
using newpro;
using RimUnity;
using Verse;
//处理主界面的退出按键和鼠标点击特效。
public class keyevents : MonoBehaviour
{
   
    void Start()
    {
        
    }

    //点击位置投影到独立UI画布，粒子与界面保持同一深度。
    void Update()
    {
        if (UiMapData.uiclose)
        {
            if (Input.GetKeyDown(global::UnityEngine.KeyCode.Escape))
            {
                MonoComp_BackButton.instance.onback();
            }
            if (Input.GetMouseButtonDown(0)) // 0 = 左键
            {
                Camera camera = UiMapData.uiCamera;
                float depth = Vector3.Dot(UiMapData.mainUI.transform.position - camera.transform.position, camera.transform.forward);
                Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
                ShopEvents.RaiseRefresh();
                SpawnParticleAtPosition(worldPos);
            }
        }
    }

    //在指定的世界坐标播放粒子
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
                //随画布提交到最后一层，避免场景粒子被随后绘制的UI背景盖住。
                instGo.transform.SetParent(UiMapData.mainUI.transform, true);
                instGo.transform.SetAsLastSibling();
                BANWlLib.mainUI.Initialization.BAUiParticles.Configure(instGo);
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

