using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using SharpConnect;
using System.Security.Permissions;

public class LinkSyncSCR : MonoBehaviour
{
    public Connector test = new Connector();
    string lastMessage;
    public Transform playerCoord;
    private const string IP_ADDRESS = "127.0.0.1";

    void Start()
    {
        Debug.Log("Starting client, attempting connection to " + IP_ADDRESS);
        Debug.Log(test.FnConnectResult(IP_ADDRESS, 10000, System.Environment.MachineName));
        if (test.res != "")
        {
            Debug.Log(test.res);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            Debug.Log("Sending example properties to server");
            test.SendProperties(200, 10.0f, 15.0f, 20.0f, 25.0f, 30.0f, 35.0f, 40.0f);
        }
        if (Input.GetKeyDown("o"))
        {
            Debug.Log("Sending player position: " + playerCoord.position.x + ", " + playerCoord.position.z);
            test.SendPlayerPos(playerCoord.position);
        }

        if (Input.GetKeyDown("i"))
        {
            Debug.Log("Received positions: " + test.counter);
        }
        if (test.res != lastMessage && test.res != "")
        {
            Debug.Log(test.res);
            lastMessage = test.res;
        }
    }

    void OnApplicationQuit()
    {
        try { test.FnDisconnect(); }
        catch { }
    }
}