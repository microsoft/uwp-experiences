using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShuttleModeHandler : MonoBehaviour {
    public Color bgErathColor;
    public GameObject shuttleSimulator;
    public GameObject shuttleExplorer;

    ShuttleSimulator shuttleSimulatorCompnent;
    ShuttleExplorer shuttleExplorerComponent;

    public Image bgImage;


    public enum ShuttleMode { Simulate, Explore };
    static ShuttleMode Mode = ShuttleMode.Simulate;

    public static void SetMode(ShuttleMode m)
    {
        Mode = m;
    }

    // Use this for initialization
    void Start () {
        shuttleSimulatorCompnent = shuttleSimulator.GetComponent<ShuttleSimulator>();
        shuttleExplorerComponent = shuttleExplorer.GetComponent<ShuttleExplorer>();

    }
	
	// Update is called once per frame
	void Update () {
	    switch(Mode)
        {
            case ShuttleMode.Simulate:
                {
                    shuttleSimulator.SetActive(true);
                    shuttleExplorer.SetActive(false);
                    break;
                }

            case ShuttleMode.Explore:
                {
                    shuttleSimulator.SetActive(false);
                    shuttleExplorer.SetActive(true);
                    break;
                }

        }

        bgImage.color = Color.Lerp(bgErathColor, Color.black, ShuttleSimulator.currentNormalizedTime);
    }
}
