using UnityEngine;
using System.Collections;

public class BilboardScript : MonoBehaviour
{

    private Transform playerTransform;
	// Use this for initialization
	void Start () {
        playerTransform = GameObject.Find("FirstPersonCharacter").transform;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    this.transform.rotation = playerTransform.rotation;
	}
}
