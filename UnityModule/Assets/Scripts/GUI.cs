using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUI : MonoBehaviour
{

    public InputField flakesNum;
    public InputField flakeSize;
    public InputField windStr;
    public InputField windStrFluc;
    public InputField windDir;
    public InputField windDirFluc;
    public InputField radius;
    public InputField noise;
    public Button start;
    public Button stop;

    public void InitializeParameteres()
    {
        LinkSyncSCR.objNum = int.Parse(flakesNum.text);
        LinkSyncSCR.objSize = float.Parse(flakeSize.text);
        LinkSyncSCR.windStr = float.Parse(windStr.text);
        LinkSyncSCR.windStrFluc = float.Parse(windStrFluc.text);
        LinkSyncSCR.windDir = float.Parse(windDir.text);
        LinkSyncSCR.windDirFluc = float.Parse(windDirFluc.text);
        LinkSyncSCR.radius = float.Parse(radius.text);
        LinkSyncSCR.noise = float.Parse(noise.text);

        LinkSyncSCR.generate = LinkSyncSCR.Phase.Start;

        flakesNum.interactable = false;
        flakeSize.interactable = false; 
        windStr.interactable = false; 
        windStrFluc.interactable = false; 
        windDir.interactable = false; 
        windDirFluc.interactable = false; 
        radius.interactable = false; 
        noise.interactable = false; 
        start.interactable = false;
        stop.interactable = true; 

    }

    public void StopGenerating()
    {
        LinkSyncSCR.generate = LinkSyncSCR.Phase.Stopping;

        flakesNum.interactable = true;
        flakeSize.interactable = true;
        windStr.interactable = true;
        windStrFluc.interactable = true;
        windDir.interactable = true;
        windDirFluc.interactable = true;
        radius.interactable = true;
        noise.interactable = true;
        start.interactable = true;
        stop.interactable = false;
    }

}
