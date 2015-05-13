using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using SharpConnect;
using System.Security.Permissions;

public class LinkSyncSCR : MonoBehaviour
{
    string lastMessage;
    public Transform playerCoord;
    private const string IP_ADDRESS = "127.0.0.1";
    public GameObject cube;
    public Connector test;
    public GameObject[] snowflakes= new GameObject[200];

    void Start()
    {
        test = new Connector();
        Debug.Log("Starting client, attempting connection to " + IP_ADDRESS);
        Debug.Log(test.FnConnectResult(IP_ADDRESS, 10000, System.Environment.MachineName));
        if (test.res != "")
        {
            Debug.Log(test.res);
        }

        initializeSnowlakes();
    }

    public void initializeSnowlakes()
    {
        for (int i = 0; i < 200; i++)
            snowflakes[i] = (GameObject)GameObject.Instantiate(Resources.Load("snowflake", typeof(GameObject)), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
    }

    void movePiece(int num, float x, float y, float z)
    {
        // to dziala tylko dla numeru 100, uzywa wtedy jako "platka" tej kostki na scenie, ale mozna w petli puscic dla tablicy
        
        //if (num == 100)
        //    cube.transform.position = new Vector3(x, y, z);
           
        snowflakes[num].transform.position = new Vector3(x, y, z);
    }

    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            Debug.Log("Sending example properties to server");
            test.SendProperties(200, 10.0f, 5.0f, 1.0f, 0.5f, 0.1f, 40.0f, 40.0f);
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
        if (test.counter != 0)
        {
            for (int i = 0; i < test.counter; i++)
            {
                movePiece(test.flakes[i].number, test.flakes[i].x, test.flakes[i].y, test.flakes[i].z);
                //Debug.Log(test.flakes[i].number + " " + test.flakes[i].x + " " + test.flakes[i].y + " " + test.flakes[i].z);
            }
            test.counter = 0;
        }
    }

    void OnApplicationQuit()
    {
        try { test.FnDisconnect(); }
        catch { }
    }
}