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

namespace BANWlLib.mainUI.Mission.MonoComp
{
    public class MonoComp_BaMissionType:MonoBehaviour
    {
        public BaMissionType baMissionType;
        public Button button;
        void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                MissionMapData.back2.SetActive(true);
                MonoComp_BackButton.instance.setNewObj(MissionMapData.back2, "bgm5");
                MissionMapData.mianImage.GetComponent<Image>().sprite = imgcvT2d.LoadSpriteFromFile(imgcvT2d.getRimWorldImgPath(baMissionType.UIShowImagePath));
                showSelfTypeNode(baMissionType);
            });
        }
        void showSelfTypeNode(BaMissionType type)
        {
            foreach(MonoComp_BaMissionNode monoComp in MissionMapData.AllBaMissionNode)
            {
                monoComp.showSelf(type);
            }
        }  
    }
}
