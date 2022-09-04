using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;



public class Walk : MonoBehaviour
{
    public GameObject prefab;

    // player
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    // self
    string playerID = "";
    // 上一次移动的时间
    public float lastMoveTime;
    // 单例
    public static Walk instance;
    void Start()
    {
        instance = this;
    }

    // 添加玩家
    void AddPlayer(string id, Vector3 pos, int score)
    {
        GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
        TextMesh textMesh = player.transform.Find("TextMesh").GetComponent<TextMesh>();
        textMesh.text = id + ":" + score;
        players.Add(id, player);
    }

    // 删除玩家
    void DelPlayer(string id) 
    {
        // 已经初始化过了
        if (players.ContainsKey(id)) {
            Destroy(players[id]);
            players.Remove(id);
        }
    }

    // 更新分数
    public void UpdateScore(string id, int score)
    {
        GameObject player = players[id];
        if (player == null) return;
        TextMesh textMesh = player.transform.Find("TextMesh").GetComponent<TextMesh>();
        textMesh.text = id + ":" + score;
    }

    // 更新信息
    public void UpdateInfo(string id, Vector3 pos, int score) 
    {
        // zhi更新自己的分数
        if (id == playerID)
        {
            UpdateScore(id, score);
            return;
        }
        // 其他人 
        // 已经初始化的
        if (players.ContainsKey(id))
        {
            players[id].transform.position = pos;
            UpdateScore(id, score);
        }
        else 
        {
            AddPlayer(id, pos, score);
        }
    }
    
    public void StartGame(string id)
    {
        playerID = id;
        //产生自己
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
        float x = 0 + UnityEngine.Random.Range(-30, 30);
        float y = 5;
        float z = 0 + UnityEngine.Random.Range(-30, 30);
        Vector3 pos = new Vector3(x, y, z);
        AddPlayer(playerID, pos, 0);
        //同步
        SendPos();
        //获取列表
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("GetList");
        NetMgr.srvConn.Send(proto, GetList);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", UpdateInfo);
        NetMgr.srvConn.msgDist.AddListener("PlayerLeave", PlayerLeave);
    }

    //发送位置
    void SendPos()
    {
        GameObject player = players[playerID];
        Vector3 pos = player.transform.position;
        //消息
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        NetMgr.srvConn.Send(proto);
    }

    //更新列表
    public void GetList(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取头部数值
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        //遍历
        for (int i = 0; i < count; i++)
        {
            string id = proto.GetString(start, ref start);
            float x = proto.GetFloat(start, ref start);
            float y = proto.GetFloat(start, ref start);
            float z = proto.GetFloat(start, ref start);
            int score = proto.GetInt(start, ref start);
            Vector3 pos = new Vector3(x, y, z);
            UpdateInfo(id, pos, score);
        }
    }

    //更新信息
    public void UpdateInfo(ProtocolBase protocol)
    {
        //获取数值
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        float x = proto.GetFloat(start, ref start);
        float y = proto.GetFloat(start, ref start);
        float z = proto.GetFloat(start, ref start);
        int score = proto.GetInt(start, ref start);
        Vector3 pos = new Vector3(x, y, z);
        UpdateInfo(id, pos, score);
    }

    //玩家离开
    public void PlayerLeave(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取数值
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        DelPlayer(id);
    }


    void Move()
    {
        if (playerID == "")
            return;
        if (players[playerID] == null)
            return;
        if (Time.time - lastMoveTime < 0.1)
            return;
        lastMoveTime = Time.time;


        GameObject player = players[playerID];
        //上
        if (Input.GetKey(KeyCode.UpArrow))
        {
            player.transform.position += new Vector3(0, 0, 1);
            SendPos();
        }
        //下
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            player.transform.position += new Vector3(0, 0, -1); ;
            SendPos();
        }
        //左
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            player.transform.position += new Vector3(-1, 0, 0);
            SendPos();
        }
        //右
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            player.transform.position += new Vector3(1, 0, 0);
            SendPos();
        }
        //分数
        else if (Input.GetKey(KeyCode.Space))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("AddScore");
            NetMgr.srvConn.Send(proto);
        }
    }

    void Update()
    {
        Move();
    }
}
