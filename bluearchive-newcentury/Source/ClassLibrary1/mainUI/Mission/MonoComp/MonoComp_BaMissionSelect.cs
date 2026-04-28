using BANWlLib.BaDef;
using BANWlLib.BANWMap;
using BANWlLib.mainUI.Mission.GameComp;
using BANWlLib.mainUI.pojo;
using BANWlLib.mainUI.StudentManual;
using BANWlLib.MissionRunTime;
using BANWlLib.Tool;
using BANWlLib.uicreater.tool;
using newpro;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.Mission.MonoComp
{
    internal static class MissionSpriteSizes
    {
        public const int QueuePortrait = 256;
        public const int ListHead = 128;
        public const int CachedHead = 128;
    }

    public class selectDataQue
    {
        public GameObject queOBJ;
        public selectData selectData;
    }
    public class selectData : IExposable
    {
        public string lable;
        public Pawn Pawn;
        public BaStudentRaceDef baDef;
        public StudentData studentData;
        public void ExposeData()
        {
            Scribe_Values.Look(ref lable, "lable", string.Empty);
            Scribe_Defs.Look(ref baDef, "baDef");
            Scribe_Deep.Look(ref studentData, "studentData", null);
            Scribe_References.Look(ref Pawn, "Pawn");
        }
    }
    public class MonoComp_BaMissionSelect : MonoBehaviour
    {
        public GameObject selectPal;
        public GameObject ListContent;
        public GameObject ShowContent;
        public ManualDataGameComp ManualDataGameComp;
        public GameComp_TaskQuest quest;
        public Dictionary<GameObject, selectData> objmap = new Dictionary<GameObject, selectData>();
        public List<GameObject> pawnlist = new List<GameObject>();
        public List<selectDataQue> queSelect = new List<selectDataQue>();
        public BaMissionNode baMissionNode;
        void Awake()
        {
            selectPal = this.transform.Find("Mask").gameObject;
            foreach (Transform child in transform.Find("SelectGroup"))
            {
                MonoComp_BaLongPressDraggable monoComp_BaLongPressDraggable = child.Find("buttom").gameObject.AddComponent<MonoComp_BaLongPressDraggable>();
                monoComp_BaLongPressDraggable.targetToMove = child.Find("SelectPawn").GetComponent<RectTransform>();
            }
            selectPal.transform.Find("SelectPal/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                selectPal.SetActive(false);
                UiMapData.isLocKBack = false;
            });
            this.transform.Find("them").GetComponent<Button>().onClick.AddListener(() =>
            {
                UiMapData.isLocKBack = true;
                setSelectStudent();
                selectPal.SetActive(true);
            });
            this.transform.Find("Start").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (quest.selectDataList.Count < 1)
                {
                    BamessageUI.ShowBaMessageUI("出击失败", "任务编队为空，请至少选择一名学生和殖民者再进行出击", "了解");
                    return;
                }
                UICoreStart.fanhuiyouxi();
                Map missionMap = CreateMap.CreateSandPocketMap(baMissionNode.missionMapDef);
                Type classToSpawn = baMissionNode.missionRunTimeDef.missionClass;
                BaMissionRunTimeAction actionInstance = (BaMissionRunTimeAction)Activator.CreateInstance(classToSpawn);
                actionInstance.missionPawns = JumpPawn(missionMap);
                actionInstance.map = missionMap;
                actionInstance.def = baMissionNode;
                quest.NoDie = actionInstance.missionPawns;
                var manager = Find.World.GetComponent<BaMissionManager>();
                manager.AddMission(actionInstance);
                quest.isStarMission = true;
            });
            ListContent = selectPal.transform.Find("SelectPal/selectShow/Viewport/Content").gameObject;
            ShowContent = selectPal.transform.Find("SelectPal/selectList/Viewport/Content").gameObject;
            ManualDataGameComp = Current.Game.GetComponent<ManualDataGameComp>();
            quest = Current.Game.GetComponent<GameComp_TaskQuest>();
            selectPal.transform.Find("SelectPal/quek").GetComponent<Button>().onClick.AddListener(() =>
            {
                quest.selectDataList.Clear();
                foreach (selectDataQue selectDataQue in queSelect)
                {
                    quest.selectDataList.Add(selectDataQue.selectData);
                }
                selectPal.SetActive(false);
                UiMapData.isLocKBack = false;
                SetBigSprite();
            });
        }

        List<Pawn> JumpPawn(Map map)
        {
            List<Pawn> jumpedPawns = new List<Pawn>();
            List<IntVec3> spawnCells = BuildMissionSpawnCells(map, baMissionNode.missionRunTimeDef.Spawnposition, quest.selectDataList.Count);
            for (int i = 0; i < quest.selectDataList.Count; i++)
            {
                selectData selectData = quest.selectDataList[i];
                IntVec3 spawnCell = spawnCells.ElementAtOrDefault(i);
                if (!spawnCell.IsValid)
                {
                    spawnCell = baMissionNode.missionRunTimeDef.Spawnposition;
                }

                if (selectData.Pawn != null)
                {
                    Pawn pawn = PawnDropHelper.JumpForPawnOfBaEff(map, selectData.Pawn, spawnCell, false);
                    jumpedPawns.Add(pawn);
                }
                else
                {
                    Pawn pawn = PawnDropHelper.JumpForRaceOfBaEff(map, selectData.baDef, spawnCell, selectData.studentData, selectData, false);
                    jumpedPawns.Add(pawn);
                }
            }
            return jumpedPawns;
        }

        private List<IntVec3> BuildMissionSpawnCells(Map map, IntVec3 center, int count)
        {
            List<IntVec3> result = new List<IntVec3>();
            if (map == null || count <= 0)
            {
                return result;
            }

            if (center.IsValid && center.InBounds(map) && center.Standable(map))
            {
                result.Add(center);
            }

            int searchRadius = Mathf.Max(6, count + 2);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, searchRadius, true))
            {
                if (result.Count >= count)
                {
                    break;
                }
                if (!cell.InBounds(map) || !cell.Standable(map))
                {
                    continue;
                }
                if (result.Contains(cell))
                {
                    continue;
                }
                result.Add(cell);
            }

            while (result.Count < count)
            {
                IntVec3 fallback = CellFinder.RandomClosewalkCellNear(center, map, searchRadius);
                if (!fallback.IsValid)
                {
                    fallback = DropCellFinder.RandomDropSpot(map);
                }
                if (!fallback.IsValid)
                {
                    fallback = center;
                }
                result.Add(fallback);
            }

            return result;
        }
        void OnDisable()
        {
            UiMapData.mainUI.transform.Find("daohang/fanhuiyouxi").gameObject.SetActive(true);
        }

        public void SetBigSprite()
        {
            setSelectStudent();
            foreach (Transform child in transform.Find("SelectGroup"))
            {
                selectData selectData = quest.selectDataList.ElementAtOrDefault(child.GetSiblingIndex());
                if (selectData != null)
                {
                    child.Find("None").gameObject.SetActive(false);
                    child.Find("buttom").gameObject.GetComponent<MonoComp_BaLongPressDraggable>().studentRaceDef = selectData.baDef;
                    GameObject SelectPawn = child.Find("SelectPawn").gameObject;
                    SelectPawn.SetActive(true);
                    if (selectData.Pawn != null)
                    {
                        SelectPawn.GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetSpriteFromPawn(selectData.Pawn, MissionSpriteSizes.QueuePortrait);
                    }
                    else
                    {
                        PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(k => k.race != null && k.race.defName == selectData.baDef.defName);
                        SelectPawn.GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetSpriteFromKind(kindDef, MissionSpriteSizes.QueuePortrait);
                    }
                    if (selectData.baDef != null)
                    {
                        if (selectData.baDef.baStudentData != null)
                        {
                            child.Find("infoBack").gameObject.SetActive(true);
                            child.Find("infoBack/Info/Attack").GetComponent<Image>().sprite = MissionMapData.MissionSprite[selectData.baDef.baStudentData.DamageType.ToString() + "_main"];
                            child.Find("infoBack/Info/Defense").GetComponent<Image>().sprite = MissionMapData.MissionSprite[selectData.baDef.baStudentData.DefenseType.ToString() + "_def"];
                            Log.Message(selectData.baDef.baStudentData.DefenseType.ToString() + "_def");
                            child.Find("infoBack/Info").GetComponent<Image>().sprite = MissionMapData.MissionSprite[selectData.baDef.baStudentData.PosType.ToString() + "_big"];
                            child.Find("infoBack/StarCont").GetComponent<UnityEngine.UI.Text>().text = selectData.baDef.baStudentData.StarCont.ToString();
                            if (selectData.studentData != null)
                            {
                                child.Find("infoBack/Leve").GetComponent<UnityEngine.UI.Text>().text = "Lv." + selectData.studentData.StudentLv;
                            }
                            else
                            {
                                child.Find("infoBack/Leve").GetComponent<UnityEngine.UI.Text>().text = "Lv.1";
                            }
                        }
                        else
                        {
                            child.Find("infoBack").gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        child.Find("infoBack").gameObject.SetActive(false);
                    }
                }
                else
                {
                    child.Find("None").gameObject.SetActive(true);
                    GameObject SelectPawn = child.Find("SelectPawn").gameObject;
                    SelectPawn.SetActive(false);
                    child.Find("infoBack").gameObject.SetActive(false);
                }
            }
        }
        void SyncSelectionWithSave()
        {
            lordSaveSelect();
            for (int i = queSelect.Count - 1; i >= 0; i--)
            {
                var item = queSelect[i];
                if (item.queOBJ != null) Destroy(item.queOBJ);
                foreach (var pair in objmap)
                {
                    if (pair.Value == item.selectData)
                    {
                        pair.Key.transform.Find("select").gameObject.SetActive(false);
                        break;
                    }
                }
            }
            queSelect.Clear();
            foreach (var savedData in quest.selectDataList)
            {
                GameObject targetBtnObj = null;
                foreach (var pair in objmap)
                {
                    selectData uiData = pair.Value;

                    // 匹配逻辑：比较 Pawn 实例或者 DefName
                    bool isMatch = false;
                    if (savedData.Pawn != null && uiData.Pawn != null)
                    {
                        isMatch = (savedData.Pawn == uiData.Pawn);
                    }
                    else if (savedData.baDef != null && uiData.baDef != null)
                    {
                        isMatch = (savedData.baDef.defName == uiData.baDef.defName);
                    }

                    if (isMatch)
                    {
                        targetBtnObj = pair.Key;
                        break; // 找到了就跳出内部循环
                    }
                }
                if (targetBtnObj != null)
                {
                    SelectThisPawn(targetBtnObj);
                }
            }
        }
        public void setSelectStudent()
        {
            StudentRosterUtility.SyncAllStudentRuntimeState(ManualDataGameComp);

            if (ListContent.transform.childCount != ManualDataGameComp.HaveStudent.Count)
            {
                objmap.Clear();
                foreach (Transform child in ListContent.transform)
                {
                    Destroy(child.gameObject);
                }
                foreach (StudentData studentData in ManualDataGameComp.HaveStudent)
                {
                    if (studentData == null || string.IsNullOrEmpty(studentData.DefName))
                    {
                        continue;
                    }

                    BaStudentRaceDef def = DefDatabase<BaStudentRaceDef>.GetNamed(studentData.DefName);
                    if (def == null)
                    {
                        continue;
                    }

                    GameObject MissionTargetObj = GameObject.Instantiate(MissionMapData.selectList, ListContent.transform);
                    MissionTargetObj.transform.Find("avt").GetComponent<Image>().sprite = MissionMapData.pawnBigHardSprite[studentData.DefName];
                    MissionTargetObj.transform.Find("Name").GetComponent<UnityEngine.UI.Text>().text = def.label;
                    if (def.baStudentData != null)
                    {
                        MissionTargetObj.transform.Find("box").GetComponent<Image>().sprite = MissionMapData.MissionSprite[def.baStudentData.DamageType + "_box"];
                        MissionTargetObj.transform.Find("pos").GetComponent<Image>().sprite = MissionMapData.MissionSprite[def.baStudentData.PosType + "_min"];
                        MissionTargetObj.transform.Find("Start/StarCont").GetComponent<UnityEngine.UI.Text>().text = def.baStudentData.StarCont.ToString();
                    }
                    else
                    {
                        MissionTargetObj.transform.Find("box").GetComponent<Image>().sprite = MissionMapData.MissionSprite["None_Box"];
                        MissionTargetObj.transform.Find("pos").gameObject.SetActive(false);
                    }
                    MissionTargetObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SelectThisPawn(MissionTargetObj);
                    });
                    selectData selectData = new selectData();
                    selectData.Pawn = studentData.StudentPawn;
                    selectData.baDef = def;
                    selectData.lable = def.label;
                    selectData.studentData = studentData;
                    objmap.Add(MissionTargetObj, selectData);
                }
            }
            setSelectOutPawn();
            SyncSelectionWithSave();
        }

        public void setSelectOutPawn()
        {
            HashSet<Pawn> studentPawns = StudentRosterUtility.GetRuntimeStudentPawnSet(ManualDataGameComp);
            HashSet<string> studentDefNames = new HashSet<string>(
                ManualDataGameComp.HaveStudent
                    .Where(s => s != null && !string.IsNullOrEmpty(s.DefName))
                    .Select(s => s.DefName)
            );

            foreach (GameObject obj in pawnlist)
            {
                Destroy(obj);
                objmap.Remove(obj);
            }
            pawnlist.Clear();
            List<Pawn> allColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
            foreach (Pawn p in allColonists)
            {
                if (p.kindDef.defName == "BANW_Sensei")
                {
                    continue;
                }
                if (p.Faction != Faction.OfPlayer)
                {
                    continue;
                }
                if (StudentRosterUtility.IsStudentPawn(ManualDataGameComp, p) || studentPawns.Contains(p))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(p.def?.defName) && studentDefNames.Contains(p.def.defName))
                {
                    continue;
                }
                GameObject MissionTargetObj = GameObject.Instantiate(MissionMapData.selectList, ListContent.transform);
                pawnlist.Add(MissionTargetObj);
                MissionTargetObj.transform.Find("avt").GetComponent<Image>().sprite = RimWorldUISpriteUtil.GetHeadShotSpriteFromPawn(p, MissionSpriteSizes.ListHead);
                MissionTargetObj.transform.Find("Name").GetComponent<UnityEngine.UI.Text>().text = p.LabelShort;
                MissionTargetObj.transform.Find("box").GetComponent<Image>().sprite = MissionMapData.MissionSprite["None_Box"];
                MissionTargetObj.transform.Find("pos").gameObject.SetActive(false);
                MissionTargetObj.transform.Find("Start").gameObject.SetActive(false);

                MissionTargetObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectThisPawn(MissionTargetObj);
                });
                selectData selectData = new selectData();
                selectData.Pawn = p;
                selectData.baDef = null;
                selectData.lable = p.LabelShort;
                selectData.studentData = null;
                objmap.Add(MissionTargetObj, selectData);
            }
        }

        void lordSaveSelect()
        {
            quest.selectDataList.RemoveAll(selectData =>
            {
                if (selectData.Pawn == null)
                {
                    if (selectData.baDef == null)
                    {
                        return true;
                    }
                    bool hasStudent = StudentRosterUtility.IsStudentDef(ManualDataGameComp, selectData.baDef.defName);
                    if (!hasStudent)
                    {
                        return true;
                    }
                }
                return false;
            });

        }

        void SelectThisPawn(GameObject gameObject)
        {
            selectData currentData = objmap[gameObject];
            if (queSelect.Any(x => x.selectData == objmap[gameObject]))
            {
                gameObject.transform.Find("select").gameObject.SetActive(false);
                selectDataQue selectData = queSelect.Find(x => x.selectData == objmap[gameObject]);
                Destroy(selectData.queOBJ);
                queSelect.Remove(selectData);
            }
            else
            {
                if (queSelect.Count < 6)
                {
                    bool duplicatePawnSelected = currentData.Pawn != null &&
                        queSelect.Any(x => x.selectData?.Pawn == currentData.Pawn);
                    if (duplicatePawnSelected)
                    {
                        return;
                    }

                    bool duplicateStudentSelected = currentData.Pawn == null &&
                        currentData.baDef != null &&
                        queSelect.Any(x => x.selectData?.baDef == currentData.baDef);
                    if (duplicateStudentSelected)
                    {
                        return;
                    }

                    gameObject.transform.Find("select").gameObject.SetActive(true);
                    selectDataQue selectData = new selectDataQue();
                    selectData.selectData = currentData;
                    GameObject QueTargetObj = GameObject.Instantiate(MissionMapData.selectQue, ShowContent.transform);
                    QueTargetObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        gameObject.transform.Find("select").gameObject.SetActive(false);
                        Destroy(selectData.queOBJ);
                        queSelect.Remove(selectData);
                    });
                    QueTargetObj.transform.Find("Avt").GetComponent<Image>().sprite = gameObject.transform.Find("avt").GetComponent<Image>().sprite;
                    QueTargetObj.transform.Find("box").GetComponent<Image>().sprite = gameObject.transform.Find("box").GetComponent<Image>().sprite;
                    if (selectData.selectData.baDef != null)
                    {
                        if (selectData.selectData.baDef.baStudentData != null)
                        {
                            QueTargetObj.transform.Find("Star/StarCont").GetComponent<UnityEngine.UI.Text>().text = selectData.selectData.baDef.baStudentData.StarCont.ToString();
                            QueTargetObj.transform.Find("Pos").GetComponent<Image>().sprite = MissionMapData.MissionSprite[selectData.selectData.baDef.baStudentData.PosType + "_min"];
                        }
                    }
                    else
                    {
                        QueTargetObj.transform.Find("Star").gameObject.SetActive(false);
                        QueTargetObj.transform.Find("Pos").gameObject.SetActive(false);
                    }
                    selectData.queOBJ = QueTargetObj;
                    queSelect.Add(selectData);
                }
            }
        }
    }
}
