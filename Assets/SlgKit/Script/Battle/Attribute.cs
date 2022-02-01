using System.Collections;
using System.Collections.Generic;


public enum AttributeKey
{
    maxHp = 0,
    hp,
    atk,
    def
}

//加入序列化特性，才能通过U3D暴露出该结构
[System.Serializable]
public class Attribute 
{
    //最大血量
    public uint maxHp = 0;
    /// <summary>
    /// 血量
    /// </summary>
    public uint hp = 0;
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
    /// 
    public uint striking_Range_Min = 1;
    public uint striking_Range_Max =1 ;


    public uint this[int index]
    {
        get
        {

            if (index == 0) return this.maxHp;
            else if (index == 1) return this.hp;
            else if (index == 2) return this.atk;
            else if (index == 3) return this.def;
            else
            {
                throw new System.IndexOutOfRangeException();
            }
        }
        set
        {
            if (index == 0) this.maxHp = value;

            else if (index == 1) this.hp = value;
            else if (index == 2) this.atk = value;
            else if (index == 3) this.def = value;
            else
            {
                throw new System.IndexOutOfRangeException();
            }
        }
    }

}
