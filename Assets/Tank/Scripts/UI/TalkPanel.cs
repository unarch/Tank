using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System.Linq;

public class TalkPanel : MonoBehaviour
{
    Socket socket;
    // 服务端的IP和端口
    public InputField hostInput;
    public InputField portInput;

    public InputField idInput;
    public InputField pwInput;

    // 文本框
    public Text recvText;
    public string recvStr;
    public Text clientText;
    public Button linkButton;

    public Button sendButton;

    public Button loginButton;

    public Button setButton;
    public Button getButton;
    public InputField talkInput;

    // 接收缓冲区
    const int BUFFER_SIZE = 1024;
    byte[] readBuff = new byte[BUFFER_SIZE];


    // 粘包分包
    int buffCount = 0;
    byte[] lenBytes = new byte[sizeof(UInt32)];
    Int32 msgLength = 0;
    // Start is called before the first frame update

    // 协议
    ProtocolBase protocol = new ProtocolBytes();

    void Start()
    {
        hostInput = transform.Find("HostInput").GetComponent<InputField>();
        portInput = transform.Find("PortInput").GetComponent<InputField>();
        idInput = transform.Find("IDInput").GetComponent<InputField>();
        pwInput = transform.Find("PWInput").GetComponent<InputField>();

        recvText = transform.Find("RecvText").GetComponent<Text>();
        clientText = transform.Find("ClientText").GetComponent<Text>();

        linkButton = transform.Find("LinkButton").GetComponent<Button>();
        linkButton.onClick.AddListener(Connect);
        sendButton = transform.Find("SendButton").GetComponent<Button>();
        sendButton.onClick.AddListener(OnSendClick);
        loginButton = transform.Find("LoginButton").GetComponent<Button>();
        loginButton.onClick.AddListener(OnLoginClick);
        setButton = transform.Find("SetButton").GetComponent<Button>();
        setButton.onClick.AddListener(OnSetClick);
        getButton = transform.Find("GetButton").GetComponent<Button>();
        getButton.onClick.AddListener(OnGetClick);



        talkInput = transform.Find("TalkInput").GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        recvText.text = recvStr;
    }


    void Connect()
    {
        recvText.text = "";
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Connect
        string host = hostInput.text;
        int port = int.Parse(portInput.text);
        socket.Connect(host, port);
        clientText.text = "客户端地址: " + socket.LocalEndPoint.ToString();
        // Recv
        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);

    }
    // 接收回调
    private void ReceiveCb(IAsyncResult ar)
    {
        try {
            int count = socket.EndReceive(ar);
            // 数据处理
            buffCount += count;
            ProcessData();

            // 继续接收
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        } catch(Exception e) {
            recvText.text += "链接已断开" + e.Message;
            socket.Close();
        }
    }

    private void ProcessData()
    {
        // 小于长度直接
        if (buffCount < sizeof(Int32)) return;
        // 消息长度
        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (buffCount < msgLength + sizeof(Int32)) return;
        // 处理消息

        ProtocolBase newProtocol = protocol.Decode(readBuff, sizeof(Int32), msgLength);
        HandleMsg(newProtocol);

        // 清除消息
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, msgLength, readBuff, 0, count);
        buffCount = count;
        if (buffCount > 0){
            ProcessData();
        } 
    }

    private void HandleMsg(ProtocolBase protocolBase)
    {
        ProtocolBytes protocolBytes = (ProtocolBytes)protocolBase;
        int start = 0;
        string protocolName = protocolBytes.GetString(start, ref start);
        int ret = protocolBytes.GetInt(start, ref start);
        // 显示

        Debug.Log("接收 " + protocolBytes.GetDesc());
        recvStr = "接收 " + protocolBytes.GetName() + " " + ret.ToString();
    }

    public void OnSendClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeatBeat");
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }

    void Send(ProtocolBase protocol) 
    {
        // string str = talkInput.text;
        byte[] bytes = protocol.Encode();
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendBuff = length.Concat(bytes).ToArray();
        socket.Send(sendBuff);
    }


    public void OnLoginClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }

    public void OnGetClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("AddScore");
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }

    public void OnSetClick()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }
}
