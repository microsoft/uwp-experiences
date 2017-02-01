using UnityEngine;
using System.Collections;

public class ShuttleExplorer : MonoBehaviour {
    private const int MAXSELECTION = 7;
    public GameObject highlightBoosterI;
    public GameObject highlightBoosterII;
    public GameObject highlightExternalTank;
    public GameObject highlightShuttle;
    public GameObject highlightDoorI;
    public GameObject highlightDoorII;
    public GameObject highlightGimballEngine;
    public Camera mainCamera;
    public static float fov;
    static int highlightPart = -1;
    public static int GetHighlightID()
    {
        return highlightPart;
    }

    public static void IncreaseSelection()
    {
        highlightPart++;
        if (highlightPart > MAXSELECTION)
            highlightPart = -1;
    }
    public static void DecreaseSelection()
    {

        highlightPart--;
        if (highlightPart < -1)
            highlightPart = MAXSELECTION;
    }

    public static void IncreaseZoom(float dt)
    {
        fov += dt;
        if (fov > 65.0f)
            fov = 65.0f;
        if (fov < 10.0f)
            fov = 10.0f;
    }
    public static void DecreaseZoom(float dt)
    {
        fov -= dt;
        if (fov > 65.0f)
            fov = 65.0f;
        if (fov < 10.0f)
            fov = 10.0f;
    }

    // Use this for initialization
    void Start () {
        fov = mainCamera.fieldOfView;
    }
	
	// Update is called once per frame
	void Update () {
        mainCamera.fieldOfView = fov;
	    switch(highlightPart)
        {
            case -1:
                {
                    HideAll();

                    break;
                }
            case 0:
                {
                    HideAll();
                    highlightBoosterI.SetActive(true);

                    break;
                }
            case 1:
                {
                    HideAll();
                    highlightBoosterII.SetActive(true);

                    break;
                }
            case 2:
                {
                    HideAll();
                    highlightExternalTank.SetActive(true);

                    break;
                }
            case 3:
                {
                    HideAll();
                    highlightShuttle.SetActive(true);

                    break;
                }
            case 4:
                {
                    HideAll();
                    highlightGimballEngine.SetActive(true);

                    break;
                }
            case 5:
                {
                    HideAll();
                    highlightDoorI.SetActive(true);

                    break;
                }
            case 6:
                {
                    HideAll();
                    highlightDoorII.SetActive(true);

                    break;
                }
        }
	}

    void HideAll()
    {
        highlightBoosterI.SetActive(false);
        highlightBoosterII.SetActive(false);
        highlightExternalTank.SetActive(false);
        highlightShuttle.SetActive(false);
        highlightDoorI.SetActive(false);
        highlightDoorII.SetActive(false);
        highlightGimballEngine.SetActive(false);
    }
}
