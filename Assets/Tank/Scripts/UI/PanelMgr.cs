using System;
using System.Collections.Generic;

using UnityEngine;

public class PanelMgr : MonoBehaviour
{
    // 单例
    public static PanelMgr instance;
    // 画板
    private GameObject canvas;
    // 画板
    public Dictionary<string, PanelBase> dict;
    // 层级
    private Dictionary<PanelLayer, Transform> layerDict;

    
    public void Awake()
    {
        instance = this;
        InitLayer();
        dict = new Dictionary<string, PanelBase>();
    }

    // 初始化层
    private void InitLayer()
    {
        canvas = GameObject.Find("Canvas");
        if (canvas == null)
            Debug.LogError("PanelMgr.InitLayer fail , canvas is null");
        // 各个层级
        layerDict = new Dictionary<PanelLayer, Transform>();
        foreach(PanelLayer p1 in Enum.GetValues(typeof(PanelLayer)))
        {
            string name = p1.ToString();
            Transform transform = canvas.transform.Find(name);
            layerDict.Add(p1, transform);
        }
    }

    // 打开面板
    public void OpenPanel<T>(string skinPath, params object[] args ) where T : PanelBase
    {
        // 已经打开
        string name = typeof(T).ToString();
        if (dict.ContainsKey(name)) return;
        // 面板脚本
        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dict.Add(name, panel);
        // 加载皮肤
        skinPath = (skinPath != "" ? skinPath : panel.skinPath);
        GameObject skin = Resources.Load<GameObject>(skinPath);
        if (skin == null)
            Debug.LogError("panelMgr.OpenPanel fail, skin is null , skinPath = " + skinPath);
        panel.skin = (GameObject)Instantiate(skin);
        // 坐标
        Transform skinTrans = panel.skin.transform;
        PanelLayer layer = panel.layer;
        Transform parent = layerDict[layer];
        skinTrans.SetParent(parent, false);
        // 生命周期
        panel.OnShowing();
        panel.OnShowed();
    }

    // 关闭面板
    public void ClosePanel(string name)
    {
        Debug.Log("panel = "+ name);
        PanelBase panel = (PanelBase) dict[name];
        
        if (panel == null) return;
        panel.OnClosing();
        dict.Remove(name);
        panel.OnClosed();
        GameObject.Destroy(panel.skin);
        Component.Destroy(panel);
    }

}

///分层类型
public enum PanelLayer
{
    //面板
    Panel,
    //提示
    Tips,
}