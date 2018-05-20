using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class client : MonoBehaviour {

    public string msg;
    public float[] cmd,fm, test;

    private NetMqListener _netMqListener;

    public string topic;

    private void HandleMessage(string message)
    {
        //         var splittedStrings = message.Split(' ');
        //         if (splittedStrings.Length != 3) return;
        //         var x = float.Parse(splittedStrings[0]);
        //         var y = float.Parse(splittedStrings[1]);
        //         var z = float.Parse(splittedStrings[2]);
        //         transform.position = new Vector3(x, y, z);
        msg = message;
    }

    private void HandleFMessage(byte[] b)
    {
        //         var splittedStrings = message.Split(' ');
        //         if (splittedStrings.Length != 3) return;
        //         var x = float.Parse(splittedStrings[0]);
        //         var y = float.Parse(splittedStrings[1]);
        //         var z = float.Parse(splittedStrings[2]);
        //         transform.position = new Vector3(x, y, z);
        if (msg[0].Equals('s'))
        {
            Buffer.BlockCopy(b, 0, cmd, 0, 4);
        }
        else if (msg[0].Equals('m'))
        {
            int len = int.Parse(msg.Substring(1));
            fm = new float[len];
            Buffer.BlockCopy(b, 0, fm, 0, 4 * len);
        }
        else
        {
            Buffer.BlockCopy(b, 0, test, 0, 4);
        }
    }

    private void Start()
    {
        _netMqListener = new NetMqListener(HandleMessage, HandleFMessage, topic);
        _netMqListener.Start();

        cmd = new float[1];
        fm = new float[64];
        test = new float[1];
    }

    private void Update()
    {
        _netMqListener.Update();
    }

    private void OnDestroy()
    {
        _netMqListener.Stop();
    }
}
