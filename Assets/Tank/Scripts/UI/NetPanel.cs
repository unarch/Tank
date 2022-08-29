using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class NetPanel : MonoBehaviour
{
    Socket socket;
    // 服务端的IP和端口
    public InputField hostInput;
    public InputField portInput;
    // 文本框
    public Text recvText;
    public Text clientText;
    public Button linkButton;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Connect()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Connect
        string host = hostInput.text;
        int port = int.Parse(portInput.text);
        socket.Connect(host, port);
        clientText.text = "客户端地址: " + socket.LocalEndPoint.ToString();
        // Send
        string str = "Hello Unity!";
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);
        //Recv
        int count = socket.Receive(readBuff);
        str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
        recvText.text = str;
        socket.Close(); 

    }
}
