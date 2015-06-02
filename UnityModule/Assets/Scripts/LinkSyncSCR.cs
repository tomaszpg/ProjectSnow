using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using SharpConnect;
using System.Security.Permissions;
using UnityEngineInternal;

public class LinkSyncSCR : MonoBehaviour
{
    public enum Phase { Start, Stopping, Stop }
    public static int objNum { get; set; }
    public static float objSize { get; set; }
    public static float windStr { get; set; }
    public static float windStrFluc { get; set; }
    public static float windDir { get; set; }
    public static float windDirFluc { get; set; }
    public static float radius { get; set; }
    public static float noise { get; set; }

    public static Phase generate { get; set; }

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

        generate = Phase.Stop;
    }

    public void initializeSnowlakes()
    {
        for (int i = 0; i < objNum; i++)
        {
            GameObject newSnowflake = (GameObject)Instantiate(Resources.Load("snowflake_simple", typeof (GameObject)), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            newSnowflake.tag = "Flake";
            snowflakes[i] = newSnowflake;
        }
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
        if (generate == Phase.Start)
        {
            Debug.Log("Sending example properties to server");
            // 1.Number, 2.size, 3.windStr, 4.windStrFluc, 5.windDir, 6.windDirFluc, 7.radius, 8.noise
            test.SendProperties(objNum, objSize, windStr, windStrFluc, windDir, windDirFluc, radius, noise);
            snowflakes = new GameObject[objNum];
            initializeSnowlakes();
        }

        if (generate == Phase.Stopping)
        {
            GameObject[] toDestroy = GameObject.FindGameObjectsWithTag("Flake");
            foreach (GameObject flake in toDestroy)
                Destroy(flake);


            generate = Phase.Stop;
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
        if (test.counter != 0 && generate == Phase.Start)
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