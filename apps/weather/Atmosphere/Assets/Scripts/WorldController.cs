using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour {
    public GameObject rainGO;
    public GameObject darkCloudsGO;
    public GameObject thunderstormGO;
    public GameObject lightCloudsGO;
    public GameObject planetsGO;
    public GameObject sunblock;

    public float sunPosition;
    public bool enableRain = false;
    public bool enableLightClouds = false;
    public bool enableDarkCloudsNoRain = false;
    public bool enableThunderstorm = false;

    public Material clearSkyMaterial;
    public Material darkSkyMaterial;
    public MeshRenderer internalSky;

    public bool IsColliding = false;
    public float sizeReducer = 0.0f;
    public float sizeWhenEnter = 0.0f;
    public float sizeWhenExit = 0.0f;

    void OnCollisionEnter(Collision other)
    {
        sizeWhenEnter = sizeReducer;
    }
    void OnCollisionExit(Collision other)
    {
        IsColliding = false;
        sizeWhenExit = sizeReducer;
    }
    void OnCollisionStay(Collision other)
    {
        IsColliding = true;
        sizeReducer -= Time.deltaTime;
    }


    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        SetRain(enableRain);
        SetLightClouds(enableLightClouds);
        SetSunPosition(sunPosition);
        SetDarkClouds(enableDarkCloudsNoRain);
        SetThunderstorm(enableThunderstorm);
        
        SetSun(!(enableDarkCloudsNoRain || enableRain || enableThunderstorm));
        SetSkybox((enableDarkCloudsNoRain || enableRain || enableThunderstorm));
        SetSPF((enableDarkCloudsNoRain || enableRain || enableThunderstorm));
    }

    public void SetSunPosition(float p)
    {
        float d = p % 360;
        planetsGO.transform.rotation = Quaternion.Euler(d - 90.0f, -30, 0.0f);
    }

    public void SetSun(bool s)
    {
        planetsGO.SetActive(s);
    }

    public void SetRain(bool r)
    {
        rainGO.SetActive(r);
    }
    
    public void SetSPF(bool r)
    {
        sunblock.SetActive(r);
    }

    public void SetThunderstorm(bool r)
    {
        thunderstormGO.SetActive(r);
    }

    public void SetDarkClouds(bool r)
    {
        darkCloudsGO.SetActive(r);
    }

    public void SetLightClouds(bool lc)
    {
        lightCloudsGO.SetActive(lc);
    }

    public void SetSkybox(bool c)
    {
        if(c)
        {
            internalSky.material = darkSkyMaterial;
            sunblock.GetComponent<MeshRenderer>().material = darkSkyMaterial;
        } else
        {
            internalSky.material = clearSkyMaterial;
            sunblock.GetComponent<MeshRenderer>().material = clearSkyMaterial;
        }
    }
}
