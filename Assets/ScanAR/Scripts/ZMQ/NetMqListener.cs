using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using UnityEngine;
using NetMQ.Sockets;

public class NetMqListener
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    public delegate void MessageFDelegate(byte[] b);
    private readonly MessageFDelegate _messageFDelegate;

    private ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<byte[]> _messageFQueue = new ConcurrentQueue<byte[]>();

    private string _topic;

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://localhost:5563");
            subSocket.Subscribe(_topic);
            while (!_listenerCancelled)
            {
                string frameString;
                if (!subSocket.TryReceiveFrameString(out frameString)) continue;
                Debug.Log(frameString);
                _messageQueue.Enqueue(frameString);

                byte[] frameByte;
                if (!subSocket.TryReceiveFrameBytes(out frameByte)) continue;
                Debug.Log(frameByte);
                _messageFQueue.Enqueue(frameByte);

            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message;
            if (_messageQueue.TryDequeue(out message))
            {
                _messageDelegate(message);
            }
            else
            {
                break;
            }
            //float f;
            byte[] b;
            if (_messageFQueue.TryDequeue(out b))
            {
                _messageFDelegate(b);
            }
            else
            {
                break;
            }
        }
    }

    public NetMqListener(MessageDelegate messageDelegate, MessageFDelegate messageFDelegate, string topic)
    {
        _messageDelegate = messageDelegate;
        _messageFDelegate = messageFDelegate;
        _listenerWorker = new Thread(ListenerWork);
        _topic = topic;
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}