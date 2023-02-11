using System;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class SocketManager : MonoBehaviour
{
    private SocketIOUnity _socket;

    public InputField eventNameTxt;
    public InputField dataTxt;
    public Text receivedText;  
    
    // Start is called before the first frame update
    private void Start()
    {
        var uri = new Uri("http://localhost:3333/chat");
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        _socket.JsonSerializer = new NewtonsoftJsonSerializer();

        ///// reserved socket.io events
        _socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };
        
        _socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        
        _socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        
        _socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        
        _socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        ////

        Debug.Log("Connecting...");
        _socket.Connect();
        
        receivedText.text = "";
        _socket.OnAnyInUnityThread((name, response) =>
        {
            if (receivedText.text.Length > 200)
            {
                receivedText.text = "";
            }
            receivedText.text += "Received On " + name + " : " + response + "\n";
        });
    }

    public void EmitTest()
    {
        var eventName = eventNameTxt.text.Trim().Length < 1 ? "ping" : eventNameTxt.text;
        var txt = dataTxt.text;
        
        if (!IsJSON(txt))
        {
            _socket.Emit(eventName, txt);
        }
        else
        {
            _socket.EmitStringAsJSON(eventName, txt);
        }
    }

    private static bool IsJSON(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) { return false; }
        str = str.Trim();
        if ((str.StartsWith("{") && str.EndsWith("}")) || //For object
            (str.StartsWith("[") && str.EndsWith("]"))) //For array
        {
            try
            {
                var obj = JToken.Parse(str);
                return true;
            }catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void JoinRoom()
    {
        var eventName = eventNameTxt.text.Trim().Length < 1 ? "room:join" : eventNameTxt.text.Trim();
        var txt = dataTxt.text.Trim().Length < 1 ? "{\"room\":\"123\",\"name\":\"groot\"}" : dataTxt.text.Trim();
        
        _socket.EmitStringAsJSON(eventName, txt);
    }

    public void SendChat()
    {
        var eventName = eventNameTxt.text.Trim().Length < 1 ? "room:send" : eventNameTxt.text.Trim();
        var txt = dataTxt.text.Trim().Length < 1 ? "{\"room\":\"123\",\"name\":\"groot\",\"message\":\"test\"}" : dataTxt.text.Trim();
        
        _socket.EmitStringAsJSON(eventName, txt);
    }
}