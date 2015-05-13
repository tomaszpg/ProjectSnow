using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using SharpConnect;
using System.Security.Permissions;

public class LinkSyncSCR : MonoBehaviour
{
    private int objNum = 500;
    string lastMessage;
    public Transform playerCoord;
    private const string IP_ADDRESS = "127.0.0.1";
    public GameObject cube;
    public Connector test = new Connector();
    public GameObject[] snowflakes;

    void Start()
    {
        Debug.Log("Starting client, attempting connection to " + IP_ADDRESS);
        Debug.Log(test.FnConnectResult(IP_ADDRESS, 10000, System.Environment.MachineName));
        if (test.res != "")
        {
            Debug.Log(test.res);
        }
    }

    public void initializeSnowlakes()
    {
        for (int i = 0; i < objNum; i++)
            snowflakes[i] = (GameObject)GameObject.Instantiate(Resources.Load("snowflake_simple", typeof(GameObject)), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
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
            // 1.Number, 2.size, 3.windStr, 4.windStrFluc, 5.windDir, 6.windDirFluc, 7.radius, 8.noise
            test.SendProperties(objNum, 10.0f, 5.0f, 1.0f, 0.5f, 0.1f, 40.0f, 40.0f);
            snowflakes = new GameObject[objNum];
            initializeSnowlakes();
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
                if (i < test.flakes.Length)
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