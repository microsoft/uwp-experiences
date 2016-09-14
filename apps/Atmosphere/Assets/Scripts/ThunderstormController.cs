using UnityEngine;
using System.Collections;

public class ThunderstormController : MonoBehaviour {
    public GameObject lightningStrike;

    float timer = 0;


	// Use this for initialization
	void Start () {
        timer = 2.0f;

    }
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;

        if (timer < 0.0f)
        {
            lightningStrike.SetActive(true);
        }
        if(timer < -0.15f)
        {
            lightningStrike.SetActive(false);
            timer = Random.Range(1.0f, 6.0f);
        }
	}
}
