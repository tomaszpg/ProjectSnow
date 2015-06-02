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

        LinkSyncSCR.generate = true;
    }
}
