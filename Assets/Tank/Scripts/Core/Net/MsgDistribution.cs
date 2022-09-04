using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// 消息分发
public class MsgDistribution
{
    // 每帧处理消息的数量
    public int num = 15;
    // 消息列表
    public List<ProtocolBase> msgList = new List<ProtocolBase>();
    // 委托类型
    public delegate void Delegate(ProtocolBase protocol);
    // 事件监听表
    private Dictionary<string, Delegate> eventDict = new Dictionary<string, Delegate>();
    private Dictionary<string, Delegate> onceDict = new Dictionary<string, Delegate>();

    // Update
    public void Update()
    {
        for (int i = 0; i < num; i++)
        {
            if (msgList.Count <= 0) break;
            DispatchMsgEvent(msgList[0]);
            lock(msgList){
                msgList.RemoveAt(0);
            }
        }
    }

    // 消息分发
    public void DispatchMsgEvent(ProtocolBase protocol) 
    {
        string name = protocol.GetName();
        Debug.Log("分发处理消息 " + name);
        if (eventDict.ContainsKey(name))
        {
            eventDict[name](protocol);
        }
        if (onceDict.ContainsKey(name))
        {
            onceDict[name](protocol);
            onceDict[name] = null;
            onceDict.Remove(name);
        }
    }

    // 添加监听事件
    public void AddListener(string name, Delegate cb) 
    {
        if (eventDict.ContainsKey(name)) eventDict[name] += cb;
        else eventDict[name] = cb;
    }

    // 添加单次监听事件
    public void AddOnceListener(string name, Delegate cb)
    {
        if (onceDict.ContainsKey(name)) onceDict[name] += cb;
        else onceDict[name] = cb;
    }

    // 删除监听事件
    public void DelListener(string name, Delegate cb) 
    {
        if (eventDict.ContainsKey(name))
        {
            eventDict[name] -= cb;
            if (eventDict[name] == null)
                eventDict.Remove(name);
        }
    }

    // 删除单次监听事件
    public void DelOnceListener(string name, Delegate cb) 
    {
        if (onceDict.ContainsKey(name))
        {
            onceDict[name] -= cb;
            if (onceDict[name] == null)
                onceDict.Remove(name);
        }
    }

}
