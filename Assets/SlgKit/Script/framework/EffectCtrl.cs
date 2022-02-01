using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCtrl 
{
    public static EffectCtrl instance;
    private SpawnPool spawnPool;

    internal static void Init()
    {
        instance = new EffectCtrl();
        EventDispatcher.instance.Regist<PlayerController,Vector3>(GameEventType.showHitEffect, instance.showHitEffect);
       // EventDispatcherDemo.instance.showHitEffect += instance.showHitEffect;


    }

    private void showHitEffect(PlayerController player,Vector3  worldPos)
    {
        //throw new NotImplementedException();

        //优化前
        //var i = ResourcesExt.Load("effect/hit-blue-1");
        //var go = GameObject.Instantiate(i);
        //go.transform.position = worldPos;

        // GameObject.Destroy(go,2F);

        return;
        // prefab有点问题
        //优化后
        if (this.spawnPool == null)
        {
            var i = ResourcesExt.Load("effect/hit-blue-1");

            var poolGo = new GameObject("hitEffect Pool");

            this.spawnPool = poolGo.AddComponent<SpawnPool>();

            var prefabPool = new PrefabPool(i.transform);
            this.spawnPool.CreatePrefabPool(prefabPool);

        }
        var go = spawnPool.Spawn("hit-blue-1");
        go.transform.position = worldPos;

        spawnPool.Despawn(go, 2F);


    }
}
