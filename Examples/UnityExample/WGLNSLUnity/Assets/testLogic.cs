using NSL.BuilderExtensions.SocketCore;
using NSL.BuilderExtensions.SocketCore.Unity;
using NSL.BuilderExtensions.WebSocketsClient;
using NSL.BuilderExtensions.WebSocketsClient.Unity;
using NSL.BuilderExtensions.WebSocketsServer;
using NSL.SocketCore.Utils.Buffer;
using NSL.WebSockets.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class testLogic : MonoBehaviour
{
    public TMP_InputField countInput;

    public Button sendLoginBtn;

    public Button startWSTestBtn;

    // Start is called before the first frame update
    void Start()
    {
        sendLoginBtn.onClick.AddListener(clickLoginHandle);

        startWSTestBtn.onClick.AddListener(clickStartWS);
    }

    void clickLoginHandle()
    {
        if (!int.TryParse(countInput.text, out int count))
            throw new Exception("Input must have number value");

        repo.APILogin(new LoginViewModel()
        {
            Email = "tt"
        }, result =>
        {
            Debug.LogWarning($"{nameof(repo.APILogin)}_result = {result.MessageResponse.StatusCode}");
        });
    }

     async void clickStartWS()
    {


        var wsclient = WebSocketsClientEndPointBuilder
            .Create()
            .WithClientProcessor<wsclient>()
            .WithOptions<WSClientOptions<wsclient>>()
            .WithUrl(new Uri("ws://127.0.0.1:20006"))
            .WithCode(builder => {

                builder.AddConnectHandleForUnity(client =>
                {
                    Debug.Log($"[Client] Success connected");
                });

                builder.AddDisconnectHandleForUnity(client =>
                {
                    Debug.Log($"[Client] Client disconnected");
                });

                builder.AddExceptionHandleForUnity((ex, client) =>
                {
                    Debug.Log($"[Client] Exception error handle - {ex}");
                });

                builder.AddSendHandleForUnity((client, pid, packet, stackTrace) =>
                {
                    //Console.WriteLine($"[Server] Send packet({pid}) to {client.GetRemotePoint()} from\r\n{stackTrace}");
                    Debug.Log($"[Client] Send packet({pid}) to {client.GetRemotePoint()}");
                });

                builder.AddReceiveHandleForUnity((client, pid, packet) =>
                {
                    Debug.Log($"[Client] Receive packet({pid}) from {client.GetRemotePoint()}");
                });
            })
            .WithCode(builder => {
                builder.AddPacketHandle(1, (client, data) => {
                    Debug.Log($"received : {data.ReadString16()}");
                });
                builder.AddPacketHandle(2, (client, data) => {
                    Debug.Log($"received : {data.ReadString16()}");
                });
                builder.AddPacketHandle(3, (client, data) => {
                    Debug.Log($"received : {data.ReadString16()}");
                });
            })
//#if UNITY_EDITOR || !PLATFORM_WEBGL
//            .Build();
//#elif PLATFORM_WEBGL
            .BuildForWGLPlatform();
//#endif

        var result = await wsclient.ConnectAsync();

        if (!result)
            throw new Exception("Cannot connect to destination host");

        var packet = new OutputPacketBuffer();

        packet.PacketId = 1;

        packet.WriteString16(DateTime.Now.ToString());

        wsclient.Send(packet);
    }







    // Update is called once per frame
    void Update()
    {

    }
}
