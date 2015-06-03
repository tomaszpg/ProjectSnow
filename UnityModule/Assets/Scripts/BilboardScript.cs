using UnityEngine;
using System.Collections;

public class BilboardScript : MonoBehaviour
{

    private Transform playerTransform;
	// Use this for initialization
	void Start () {
        playerTransform = GameObject.Find("FirstPersonCharacter").transform;
		transform.Rotate(Vector3.one * Random.Range(0, 360));
	}

	// Update is called once per frame
	void Update ()
	{
	    //this.transform.rotation = playerTransform.rotation;
		transform.Rotate (Vector3.one * Time.deltaTime * 45);
	}

}
