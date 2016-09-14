using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WeatherController : MonoBehaviour {
    public enum ViewMode
    {
        TodayCloseUp, TodayList, FiveDayForecast
    }

    public enum ViewOrientation
    {
        Landscape, Portrait
    }

    public static ViewOrientation viewOrientation = ViewOrientation.Landscape;
    public static ViewMode viewMode = ViewMode.FiveDayForecast;

    public static bool IsLoaded = false;
    public static List<WeatherSettings> WeatherData = new List<WeatherSettings>();

    public List<WorldController> worlds;
    public List<Transform> camPositions;

    public Camera cameraToMove;

    public float lerpSpeed = 1.0f;

    bool changeSize = false;
    float prevAspectRatio = 0.0f;
    float aspectR;
    float w;
    float h;
    float wHalf;
    float hHalf;

    public static void SetOrientation(ViewOrientation o)
    {
        viewOrientation = o;
    }

    public static event EventHandler AssetsLoaded;

    void Awake()
    {
        WeatherData.Add(new WeatherSettings() { IsLightClouds = true });
        WeatherData.Add(new WeatherSettings() { IsRaining = true });
        WeatherData.Add(new WeatherSettings());
        WeatherData.Add(new WeatherSettings() { IsThunderstorm = true });
        WeatherData.Add(new WeatherSettings() { IsDarkClouds = true });
    }

    void Start()
    { 
        // C# 4
        if(AssetsLoaded != null)
            AssetsLoaded(null, null);
    }

    public static void UpdateWeather(int index, WeatherSettings settings)
    {
        WeatherData[index] = settings;    
    }

    public static void SetView(ViewMode v)
    {
        viewMode = v;
    }

    void Update()
    {
        switch (viewMode)
        {
            case ViewMode.TodayCloseUp:
                {
                    ArrangeTodayCloseView();
                    break;
                }
            case ViewMode.TodayList:
                {
                    ArrangeTodayList();
                    break;
                }
            case ViewMode.FiveDayForecast:
                {
                    ArrangeFiveDayView();
                    break;
                }
        }

        for(int i = 0; i < worlds.Count; i++)
        {
            worlds[i].enableLightClouds = WeatherData[i].IsLightClouds;
            worlds[i].enableRain = WeatherData[i].IsRaining;
            worlds[i].enableDarkCloudsNoRain = WeatherData[i].IsDarkClouds;
            worlds[i].enableThunderstorm = WeatherData[i].IsThunderstorm;
        }

        CheckResize();
    }

    void CheckResize()
    {
        wHalf = Screen.width / 2.0f;
        hHalf = Screen.height / 2.0f;
        w = Screen.width; h = Screen.height;
        aspectR = (float)Screen.width / (float)Screen.height;
        if (prevAspectRatio != aspectR)
        {
            changeSize = true;
            prevAspectRatio = aspectR;
        }
    }
    
    void ArrangeFiveDayView()
    {
        if(viewOrientation == ViewOrientation.Landscape)
        { 
            float ind = -2;
            foreach (WorldController t in worlds)
            {
                Vector3 p = cameraToMove.WorldToScreenPoint(t.transform.position);
                
                float wFrac = w / 5.0f;
                
                p = cameraToMove.ScreenToWorldPoint(new Vector3(wHalf + (ind * wFrac * 0.985f), hHalf + (hHalf * 0.1f), 349.5f));
                t.transform.position = Vector3.Lerp(t.transform.position, p, 0.2f);
                Vector3 scale = new Vector3(aspectR / 4.0f, aspectR / 4.0f, aspectR / 4.0f);
                t.transform.localScale = Vector3.Lerp(t.transform.localScale, scale, 0.2f);

                ind++;
            }
        }
        else
        {
            float ind = 4;
            foreach (WorldController t in worlds)
            {
                Vector3 p = cameraToMove.WorldToScreenPoint(t.transform.position);

                float hFrac = h / 6.0f;
                float wFrac = w / 8.0f;
                
                p = cameraToMove.ScreenToWorldPoint(new Vector3(wFrac, hHalf + (-hHalf + ind * hFrac) + (hFrac / 2.0f), 349.5f));
                t.transform.position = Vector3.Lerp(t.transform.position, p, 0.2f);
                
                if (((aspectR / 2.5f) + worlds[0].sizeReducer) < (aspectR / 2.5f) && changeSize)
                {
                    worlds[0].sizeReducer += Time.deltaTime / 10.0f;
                    if(worlds[0].IsColliding)
                    {
                        changeSize = false;
                    }
                }

                float finalSize = (aspectR / 2.5f) + worlds[0].sizeReducer;
                if (finalSize <= 0.1f)
                    finalSize = 0.1f;

                Vector3 scale = new Vector3(finalSize, finalSize, finalSize);
                t.transform.localScale = Vector3.Lerp(t.transform.localScale, scale, 0.2f);

                ind--;
            }
        }
    }

    void ArrangeTodayCloseView()
    {
        if (viewOrientation == ViewOrientation.Landscape)
        {
            float ind = 0;
            foreach (WorldController t in worlds)
            {
                Vector3 p = cameraToMove.WorldToScreenPoint(t.transform.position);

                float wFrac = w / 3.0f;
                
                p = cameraToMove.ScreenToWorldPoint(new Vector3(wHalf * (ind * 5) + wFrac, hHalf, 130.5f));
                t.transform.position = Vector3.Lerp(t.transform.position, p, 0.2f);
                Vector3 scale = new Vector3(aspectR * 0.75f, aspectR * 0.75f, aspectR * 0.75f);
                t.transform.localScale = Vector3.Lerp(t.transform.localScale, scale, 0.2f);

                ind++;
            }
        }
        else
        {
            float ind = 0;
            foreach (WorldController t in worlds)
            {
                Vector3 p = cameraToMove.WorldToScreenPoint(t.transform.position);

                float wFrac = w / 3.0f;
                
                p = cameraToMove.ScreenToWorldPoint(new Vector3(wHalf * (ind * 5) + wFrac, hHalf, 130.5f));
                t.transform.position = Vector3.Lerp(t.transform.position, p, 0.2f);
                Vector3 scale = new Vector3(aspectR , aspectR , aspectR );
                t.transform.localScale = Vector3.Lerp(t.transform.localScale, scale, 0.2f);

                ind++;
            }
        }
    }

    void ArrangeTodayList()
    {
        if (viewOrientation == ViewOrientation.Landscape)
        {
            float ind = 0;
            foreach (WorldController t in worlds)
            {
                Vector3 p = cameraToMove.WorldToScreenPoint(t.transform.position);

                float wFrac = w / 6.0f;
                
                p = cameraToMove.ScreenToWorldPoint(new Vector3(wHalf + (wHalf / 1.5f + ind * wFrac * 5.0f), hHalf, 349.5f));
                t.transform.position = Vector3.Lerp(t.transform.position, p, 0.2f);
                Vector3 scale = new Vector3(aspectR * 0.5f, aspectR * 0.5f, aspectR * 0.5f);
                t.transform.localScale = Vector3.Lerp(t.transform.localScale, scale, 0.2f);

                ind++;
            }
        }
        else
        {
            float ind = 0;
            foreach (WorldController t in worlds)
            {
                Vector3 p = cameraToMove.WorldToScreenPoint(t.transform.position);

                float wFrac = w / 6.0f;
                
                p = cameraToMove.ScreenToWorldPoint(new Vector3(wHalf, hHalf + (hHalf / 1.75f + ind * wFrac * 5.0f), 349.5f));
                t.transform.position = Vector3.Lerp(t.transform.position, p, 0.2f);
                Vector3 scale = new Vector3(aspectR * 0.6f, aspectR * 0.6f, aspectR * 0.6f);
                t.transform.localScale = Vector3.Lerp(t.transform.localScale, scale, 0.2f);

                ind++;
            }
        }
    }
}

public class WeatherSettings
{
    public bool IsRaining = false;
    public bool IsLightClouds = false;
    public bool IsDarkClouds = false;
    public bool IsThunderstorm = false;
}
