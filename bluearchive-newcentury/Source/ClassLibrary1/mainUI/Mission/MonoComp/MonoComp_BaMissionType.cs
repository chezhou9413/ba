using BANWlLib.BaDef;
using BANWlLib.mainUI.MonoComp;
using newpro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.Mission.MonoComp
{
    /// <summary>
    /// 负责处理任务类型按钮点击，并切换任务节点列表与背景图。
    /// </summary>
    public class MonoComp_BaMissionType:MonoBehaviour
    {
        public BaMissionType baMissionType;
        public Button button;

        /// <summary>
        /// 负责绑定按钮点击事件，并在任务界面未完整初始化时跳过本次点击。
        /// </summary>
        void Start()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Log.Warning("[BAUI] 任务类型按钮缺少 Button 组件。");
                return;
            }

            button.onClick.AddListener(() =>
            {
                if (baMissionType == null)
                {
                    Log.Warning("[BAUI] 任务类型按钮缺少 BaMissionType 数据。");
                    return;
                }

                if (MissionMapData.back2 == null || MissionMapData.mianImage == null)
                {
                    Log.Warning("[BAUI] 任务界面未完成初始化，无法打开任务类型：" + baMissionType.defName);
                    return;
                }

                MissionMapData.back2.SetActive(true);
                if (MonoComp_BackButton.instance != null)
                {
                    MonoComp_BackButton.instance.setNewObj(MissionMapData.back2, "bgm5");
                }

                Image mainImage = MissionMapData.mianImage.GetComponent<Image>();
                if (mainImage != null && !string.IsNullOrEmpty(baMissionType.UIShowImagePath))
                {
                    mainImage.sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(baMissionType.UIShowImagePath));
                }

                showSelfTypeNode(baMissionType);
            });
        }

        /// <summary>
        /// 负责通知全部任务节点按当前任务类型刷新显示状态。
        /// </summary>
        void showSelfTypeNode(BaMissionType type)
        {
            if (MissionMapData.AllBaMissionNode == null)
            {
                return;
            }

            foreach(MonoComp_BaMissionNode monoComp in MissionMapData.AllBaMissionNode)
            {
                if (monoComp != null)
                {
                    monoComp.showSelf(type);
                }
            }
        }  
    }
}
