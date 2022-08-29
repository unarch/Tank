using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class TalkPanel : MonoBehaviour
{
    Socket socket;
    // 服务端的IP和端口
    public InputField hostInput;
    public InputField portInput;
    // 文本框
    public Text recvText;
    public string recvStr;
    public Text clientText;
    public Button linkButton;

    public Button sendButton;
    public InputField talkInput;

    // 接收缓冲区
    const int BUFFER_SIZE = 1024;
    byte[] readBuff = new byte[BUFFER_SIZE];

    // Start is called before the first frame update
    void Start()
    {
        hostInput = transform.Find("HostInput").GetComponent<InputField>();
        portInput = transform.Find("PortInput").GetComponent<InputField>();
        recvText = transform.Find("RecvText").GetComponent<Text>();
        clientText = transform.Find("ClientText").GetComponent<Text>();
        linkButton = transform.Find("LinkButton").GetComponent<Button>();
        linkButton.onClick.AddListener(Connect);
        sendButton = transform.Find("SendButton").GetComponent<Button>();
        sendButton.onClick.AddListener(Send);
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
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            if (recvStr.Length > 300) recvStr = "";
            recvStr += str + "\n";
            // 继续接收
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        } catch(Exception e) {
            recvText.text += "链接已断开" + e.Message;
            socket.Close();
        }
    }
    void Send() 
    {
        string str = talkInput.text;
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        try {
            socket.Send(bytes);
        } catch{
            
        }
    }
}
