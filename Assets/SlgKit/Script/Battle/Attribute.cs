using System.Collections;
using System.Collections.Generic;

//加入序列化特性，才能通过U3D暴露出该结构
[System.Serializable]
public class Attribute 
{
    /// <summary>
    /// 攻击力
    /// </summary>
    public uint atk=0;

    /// <summary>
    /// 防御力
    /// </summary>
    public uint def = 0;
    /// <summary>
    /// 攻击距离
    /// Striking Range
    /// </summary>
    public uint striking_Range_Max =1 ;

    public uint striking_Range_Min = 1;
}
