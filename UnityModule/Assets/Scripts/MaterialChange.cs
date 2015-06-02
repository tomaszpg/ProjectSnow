using UnityEngine;
using System.Collections;

public class MaterialChange : MonoBehaviour {

    float time = 0.0f;
    public int duration = 80;
    private bool snowing = false;
    private Renderer renderer;
	void Start ()
	{
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
