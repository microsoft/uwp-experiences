using UnityEngine;
using System.Collections;

public class ShuttleSimulator : MonoBehaviour {
    public ParticleSystem srbIThrust;
    public ParticleSystem srbIIThrust;
    public ParticleSystem ShuttleMainThrust;
    public ParticleSystem Shuttle2ndThrust;
    public ParticleSystem Shuttle3rdThrust;

    public static bool fireSRBThrusters = false;
    public static bool fireShuttleThrusters = false;

    public static void FireShuttleThrusters(bool f)
    {
        fireShuttleThrusters = f;
    }

    public static void FireSLBThrusters(bool f)
    {
        fireSRBThrusters = f;
    }

    public Animator timelineAnimation;
    public static float animationTime;
    public static float currentNormalizedTime;

    static bool restartAnimation = false;

    static bool scrubAnimationMode = false;
    public static void ScrubAnimationMode(bool mode)
    {
        scrubAnimationMode = mode;
    }
    public static void RestartAnimation()
    {
        restartAnimation = true;
    }

    static bool continueAnumation;
    public static void PlayAnimationFrom(float time)
    {
        animationTime = time;
    }

    public static void SetTime(float dt)
    {
        animationTime += dt;
    }

    public void TriggerRestartAnimation()
    {
        timelineAnimation.Play("ShuttleAnimation", -1, 0.0f);
        restartAnimation = false;
        scrubAnimationMode = false;
    }

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {

        if(continueAnumation)
        {
            scrubAnimationMode = false;
            continueAnumation = false;
            timelineAnimation.speed = 1.0f;
            timelineAnimation.Play("ShuttleAnimation", -1, animationTime);
        }

        if(restartAnimation)
            TriggerRestartAnimation();

        if (scrubAnimationMode)
        {
            timelineAnimation.speed = 0.0f;
            timelineAnimation.Play("ShuttleAnimation", -1, animationTime);
        }
        else
        {
            timelineAnimation.StartPlayback();
            timelineAnimation.speed = 1.0f;
        }

        if (fireSRBThrusters)
        {
            srbIThrust.gameObject.SetActive(true);
            srbIIThrust.gameObject.SetActive(true);
        }
        else
        {
            srbIThrust.gameObject.SetActive(false);
            srbIIThrust.gameObject.SetActive(false);
        }

        if (fireShuttleThrusters)
        {
            ShuttleMainThrust.gameObject.SetActive(true);
            Shuttle2ndThrust.gameObject.SetActive(true);
            Shuttle3rdThrust.gameObject.SetActive(true);
        }
        else
        {
            ShuttleMainThrust.gameObject.SetActive(false);
            Shuttle2ndThrust.gameObject.SetActive(false);
            Shuttle3rdThrust.gameObject.SetActive(false);
        }


        if (Input.GetKeyDown(KeyCode.Space))
            TriggerRestartAnimation();

        currentNormalizedTime = timelineAnimation.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    
}
