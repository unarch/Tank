using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TitlePanel : PanelBase
{
    private Button startBtn;
    private Button infoBtn;

    #region 生命周期

    public override void Init (params object[] args)
    {
        base.Init(args);
        skinPath = "TitlePanel";
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        startBtn = skinTrans.Find("startBtn").GetComponent<Button>();
        infoBtn = skinTrans.Find("infoBtn").GetComponent<Button>();
        startBtn.onClick.AddListener(OnStartClick);
        infoBtn.onClick.AddListener(OnInfoClick);
    }
    #endregion
    
    public void OnStartClick()
    {
        // Battle.instance.StartTwoCampBattle(2, 2);
        PanelMgr.instance.OpenPanel<OptionPanel>("");
    }

    public void OnInfoClick()
    {
        PanelMgr.instance.OpenPanel<InfoPanel>("");
    }

}
