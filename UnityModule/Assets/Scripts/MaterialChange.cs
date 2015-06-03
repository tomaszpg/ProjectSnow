using UnityEngine;
using System.Collections;

public class MaterialChange : MonoBehaviour {

    float time;
    public int duration;
    private bool snowing;
    private Renderer renderer;
	void Start ()
	{
	    time = 0.0f;
	    duration = 100;
	    snowing = false;
        renderer = GetComponent<Renderer>();
        renderer.enabled = true;

	}
	

	void Update () {
	    if (snowing)
	    {
	        time += Time.deltaTime/duration;
	        renderer.material.SetFloat("_Cutoff", Mathf.Lerp(0, 1, time));
	    }
	}


    public void ItsSnowing()
    {
        snowing = true;
    }

    public void ItsNotSnowing()
    {
        snowing = false;
    }
}
