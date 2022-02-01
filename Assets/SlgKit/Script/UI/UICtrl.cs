using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICtrl : MonoBehaviour
{
    private Canvas canvas;
    private CanvasScaler _canvasScaler;
    private CanvasRenderer hudCanvas;
    private SpawnPool spawnPool;
    private bool _updateHpImage;
    public GameObject actionPanel;

    public static UICtrl instance { get; private set; }
    private void Awake()
    {
        instance = this;
        EventDispatcher.instance.Regist<int, Vector3>(GameEventType.showHudDamage, this.showHudDamage);

        canvas = this.GetComponent<Canvas>();
        _canvasScaler = this.GetComponent<CanvasScaler>();

        this.hudCanvas = this.transform.Find("hudCanvas").GetComponent<CanvasRenderer>();
        //创建对象池 伤害字体

        var i = ResourcesExt.Load("ui/hudItem");

        //var poolGo = new GameObject("HudText Pool");

        this.spawnPool = hudCanvas.gameObject.AddComponent<SpawnPool>();

        var prefabPool = new PrefabPool(i.transform);


        this.spawnPool.CreatePrefabPool(prefabPool);
    }

    private void showHudDamage(int damage, Vector3 worldPos)
    {
        //throw new NotImplementedException();

        var hudItem = spawnPool.Spawn("hudItem", this.hudCanvas.transform);

        var screenPos = getScreenPos(Camera.main, worldPos);
        hudItem.position = screenPos;


        StartCoroutine(FloatUI(hudItem.gameObject));

        var text = hudItem.Find("Text").GetComponent<Text>();
        text.text = damage.ToString();
        spawnPool.Despawn(hudItem, 1.3f);


    }

    IEnumerator FloatUI(GameObject go)
    {
        //1.2秒 升高180 米
        var duration = 1.2f;
        var startTime = Time.time;

        var startPos = go.transform.position;
        var y_offset = 180;
        float t1 = 0;
        while (t1 < 1)
        {
            t1 = (Time.time - startTime) / duration;

            if (t1 >= 1f) t1 = 1;

            yield return new WaitForEndOfFrame();

            var y = Mathf.Lerp(0, y_offset, t1);

            go.transform.position = startPos + new Vector3(0, y, 0);
        }
    }


    //// 将精灵的世界坐标转换成屏幕坐标
    private Vector3 getScreenPos(Camera cam, Vector3 worldPos)
    {
        // throw new NotImplementedException();
        var resolutionX = this._canvasScaler.referenceResolution.x;
        var resolutionY = this._canvasScaler.referenceResolution.y;
        var offset = (Screen.width / this._canvasScaler.referenceResolution.x) * (1 - this._canvasScaler.matchWidthOrHeight) + (Screen.height / this._canvasScaler.referenceResolution.y) * this._canvasScaler.matchWidthOrHeight;
        var screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        return new Vector3(screenPos.x, screenPos.y, 0);

    }


    // Start is called before the first frame update
    void Start()
    {


        actionPanel = this.transform.Find("ActionPanel").gameObject;
        var waitBtn = actionPanel.transform.Find("wait").GetComponent<Button>();
        waitBtn.onClick.AddListener(this.onWaitBtnClick);
        actionPanel.SetActive(false);
    }

    private void onWaitBtnClick()
    {
        // throw new NotImplementedException();
        actionPanel.SetActive(false);
        GameCtrl.instance.Wait();

    }



    // Update is called once per frame
    void Update()
    {
        //每帧更新血条位置
        if (_updateHpImage)
        {
            foreach (var player in GameCtrl.instance.players)
            {

                var screenPos = getScreenPos(Camera.main, player.transform.position + new Vector3(0, 4, 0));
                player.hpImageTrs.position = screenPos;


            }
        }

    }

    public void Init_HpImage()
    {
        //初始化血条预制体，利用对象池优化
        var i = ResourcesExt.Load("ui/hpImage_green");
        var prefabPool = new PrefabPool(i.transform);
        this.spawnPool.CreatePrefabPool(prefabPool);

        i = ResourcesExt.Load("ui/hpImage_red");
        prefabPool = new PrefabPool(i.transform);
        this.spawnPool.CreatePrefabPool(prefabPool);


        foreach (var player in GameCtrl.instance.players)
        {
            Transform hpImageTrs = null;
            if (player.sect == GameCtrl.instance.mySect)
                hpImageTrs = spawnPool.Spawn("hpImage_green", this.hudCanvas.transform);
            else
                hpImageTrs = spawnPool.Spawn("hpImage_red", this.hudCanvas.transform);

            var screenPos = getScreenPos(Camera.main, player.transform.position + new Vector3(0, 3, 0));
            hpImageTrs.position = screenPos;
            player.hpImageTrs = hpImageTrs;
            player.hpImage = hpImageTrs.Find("hp_front").GetComponent<Image>();

            player.viewHp = (int)player.attribute.hp;
            UpdateHp(player);
        }

        _updateHpImage = true;
    }

    public void UpdateHp(PlayerController player)
    {
        // throw new NotImplementedException();
        player.hpImage.fillAmount = (float)player.viewHp / player.attribute.maxHp;
    }

    
    GameObject fastAttackImageGob;
    internal void ShowFastAttack(PlayerController playerController)
    {
        //throw new NotImplementedException();
        if (fastAttackImageGob == null)
        {
             var i = ResourcesExt.Load("ui/fastAttackImage");

            fastAttackImageGob = MonoBehaviour.Instantiate(i);

            fastAttackImageGob.transform.SetParent(this.hudCanvas.transform, false);
        }

      


        fastAttackImageGob.SetActive(false);
        fastAttackImageGob.SetActive(true);


        var screenPos = getScreenPos(Camera.main, playerController.transform.position + Vector3.up * 5);
        fastAttackImageGob.transform.position = screenPos;

    }
}
