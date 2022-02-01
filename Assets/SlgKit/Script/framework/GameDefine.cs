﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDefine
{
    public enum AstartTag : uint { basicGround = 0x00, playerTag = 0x01, Obstacel = 0x02, Water = 0x03 }


    static string[] mTagNames;
    public static string[] GetTagNames()
    {

        if (mTagNames == null)
        {
            mTagNames = new string[32];
            for (int i = 0; i < mTagNames.Length; i++)
            {
                mTagNames[i] = "" + i;
            }
        }

        var EoftagNames = System.Enum.GetNames(typeof(AstartTag));
        for (int i = 0; i < EoftagNames.Length; i++)
        {
            mTagNames[i] = EoftagNames[i];
        }



        return mTagNames;
    }

    //天地劫中两个闹得最僵的门派
    //天玄门 和 四象门
    //他们是敌对关系
    public enum Sect : uint { TianXuanMen,SiXiangMen }


    // idel 更改为 idle
    public enum PlayerSate : uint { idle, moveAttack, wait }
}
