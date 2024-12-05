////////////////////////////////////////
/// Copyright 2024 MinecraftSRDEV    ///
///                                  ///
/// Desained for Slime Rancher 0.2.6 ///
/// Mod loader: Saty's mod tool      ///
///                                  ///
/// Debug Bridge to SRLauncher       ///
////////////////////////////////////////

using System.Text;
using System.IO.Pipes;
using UnityEngine;

namespace SRDebugBridgeNamespace
{
    public class ComunicationBridge
    {
        public static void connectToLauncher()
        {
            SRDebugBridge.runningDebugger = false;
            client = new NamedPipeClientStream(".", "DebugBridge", PipeDirection.InOut);
            Debug.Log("Connecting to SRLauncher...");
            client.Connect(1000);
            if (client.IsConnected)
            {
                Debug.Log("Connected to SRLauncher");
                Debug.Log("Debug bridge v." + SRDebugBridge.VERSION.ToString());
                SRDebugBridge.runningDebugger = true;
            }
            else
            {
                Debug.Log("Unable to connect");
            }
        }

        public static void sendMessage(string message)
        {
            byte[] requestBytes = Encoding.UTF8.GetBytes(message);
            client.Write(requestBytes, 0, requestBytes.Length);
            client.Flush();
        }

        public static void disconnect()
        {
            try
            {
                client.Flush();
                client.Close();
                SRDebugBridge.runningDebugger = false;
            }
            catch (Exception) { }
        }

        public static NamedPipeClientStream client;
    }
    public class loggerClass : MonoBehaviour
    {
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string log = "";
            switch (type)
            {
                case LogType.Log:
                    {
                        log += "0";
                        break;
                    }
                case LogType.Warning:
                    {
                        log += "1";
                        break;
                    }
                case LogType.Error:
                    {
                        log += "2";
                        break;
                    }
                case LogType.Exception:
                    {
                        log += "3";
                        break;
                    }
                default:
                    {
                        log += "0";
                        break;
                    }
            }
            log += logString;
            ComunicationBridge.sendMessage(log);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Shutting down communication pipe");
            ComunicationBridge.disconnect();
        }
    }

    public class SRDebugBridge
    {
        public static void OnLoad()
        {
            SRDebugBridge.debugGui = new GameObject("DebugGUI");
            SRDebugBridge.debugGui.AddComponent<DBGui>();
            UnityEngine.Object.DontDestroyOnLoad(SRDebugBridge.debugGui);

            SRDebugBridge.loggingSystem = new GameObject("logger");
            SRDebugBridge.loggingSystem.AddComponent<loggerClass>();
            UnityEngine.Object.DontDestroyOnLoad(SRDebugBridge.loggingSystem);

            Debug.Log("Debug bridge loaded successfully, Version: " + VERSION.ToString());

            ComunicationBridge.connectToLauncher();
        }

        public static void OnUnload()
        {
            Debug.Log("Unloading debug bridge");

            UnityEngine.Object.DestroyObject(SRDebugBridge.debugGui);
            UnityEngine.Object.DestroyObject(SRDebugBridge.loggingSystem);
        }

        private static GameObject debugGui;
        private static GameObject loggingSystem;

        public static string VERSION = "1.1";

        public static bool runningDebugger = false;
    }

    public class DBGui : MonoBehaviour
    {
        private void OnGUI()
        {
            int screenX = Screen.width;
            int screenY = Screen.height;

            string buttonText = "Debugging disabled";

            if (level > 0)
            {
                
            }

            if (SRDebugBridge.runningDebugger == true)
            {
                buttonText = "Stop debugging";
            }

            if (GUI.Button(new Rect(10, screenY - 25, 140, 20), buttonText))
            {
                ComunicationBridge.disconnect();
            }
        }

        private void OnLevelWasLoaded(int lvl)
        {
            level = lvl;
        }

        private int level = 0;
    }
}
