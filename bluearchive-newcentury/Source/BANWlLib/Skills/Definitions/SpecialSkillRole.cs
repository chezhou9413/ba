namespace BANWlLib.Skills
{
    //特殊技能角色类别，负责选择对应的技能执行器。
    public enum SpecialSkillRole { Nero, Arisu, Shiroko, Wakamo, Hoshino, Kei, Rio }

    //技能按钮行为，负责区分同一角色的主动技能和形态切换。
    public enum SpecialSkillCommand { Ex, AlternateEx, SwitchForm }
}
