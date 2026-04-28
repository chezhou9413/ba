using BANWlLib.BaDef;
using BANWlLib.mainUI.Mission.GameComp;
using BANWlLib.mainUI.MonoComp;
using newpro;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.Mission.MonoComp
{
    public class MonoComp_BaMissionNode:MonoBehaviour
    {
        public BaMissionType type;
        public BaMissionNode selfMissionInfo;
        public Button StartMisson;
        public GameComp_TaskQuest component;
        public GameObject lockMask;
        void Start() 
        { 
            lockMask = this.transform.Find("lockMask").gameObject;
            StartMisson = this.transform.Find("StartMisson").gameObject.GetComponent<Button>();
            StartMisson.onClick.AddListener(() => 
            {
                MissionMapData.missionInfo.ShowMissInfo(selfMissionInfo);
                UiMapData.isLocKBack = true;
            });
            LockMission();
        }

        public void showSelf(BaMissionType showtype)
        {
            if(showtype == type)
            {
                this.gameObject.SetActive(true);
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            LockMission();
        }

        void LockMission()
        {
            if (component == null)
            {
                component = Current.Game.GetComponent<GameComp_TaskQuest>();
            }
            if (selfMissionInfo.UnlockedOn != null)
            {
                if (component.MissionQuest.Contains(selfMissionInfo.UnlockedOn))
                {
                    lockMask.SetActive(false);
                }
                else
                {
                    lockMask.SetActive(true);
                }
            }
        }
    }
}
