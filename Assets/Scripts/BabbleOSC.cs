using Rug.Osc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using static Babble.BabbleExpressions;

public class BabbleOSC
{
    public Dictionary<string, float> MouthShapes { get; private set; }

    private OscReceiver _receiver;

    private bool _loop = true;

    private readonly Thread _thread;

    private readonly int _resolvedPort;

    private readonly IPAddress _resolvedHost;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

    public BabbleOSC(string? host = null, int? port = null)
    {
        if (_receiver != null)
        {
            Debug.LogError("BabbleOSC connection already exists.");
            return;
        }

        _resolvedHost = IPAddress.Parse(host ?? DEFAULT_HOST);
        _resolvedPort = port ?? DEFAULT_PORT;

        MouthShapes = new Dictionary<string, float>();
        for (int i = 0; i < ExpressionToAddress.Length; i++)
        {
            MouthShapes.Add(ExpressionToAddress[i], 0f);
        }

        Debug.Log($"Started BabbleOSC with Host: {_resolvedHost} and Port {_resolvedPort}");
        ConfigureReceiver();
        _loop = true;
        _thread = new Thread(new ThreadStart(ListenLoop));
        _thread.Start();
    }

    private void ConfigureReceiver()
    {
        // Create the OSC receiver with specified host and port
        _receiver = new OscReceiver(_resolvedHost, _resolvedPort);
        _receiver.Connect();
    }

    private void ListenLoop()
    {
        while (_loop)
        {
            try
            {
                if (_receiver.State == OscSocketState.Connected)
                {
                    // Check for a new OSC message
                    if (_receiver.TryReceive(out OscPacket packet))
                    {
                        if (packet is OscMessage oscMessage && oscMessage.Count > 0 && oscMessage[0] is float value)
                        {
                            ProcessMessage(oscMessage, value);
                        }
                    }
                }
                else
                {
                    _receiver.Close();
                    ConfigureReceiver();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"OSC Listener Error: {ex.Message}");
            }
        }
    }

    private void ProcessMessage(OscMessage oscMessage, float value)
    {
        if (oscMessage.Address == "/mouthFunnel" || oscMessage.Address == "/mouthPucker")
        {
            MouthShapes[oscMessage.Address] = value * 4f;
        }

        var trimmed = oscMessage.Address.TrimStart('/');
        if (MouthShapes.ContainsKey(trimmed))
        {
            MouthShapes[trimmed] = value;
        }
    }

    public void Teardown()
    {
        _loop = false;
        _receiver.Close();
        _thread.Join();
    }
}
