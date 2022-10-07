using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{

    public float lat = -33;
    public float lng = 151;

    public DateTime fixedTime;
    public DateTime currentTime = DateTime.Now;
    public float timePassed = 0.0f;

    public float timeScale = 1.0f;

    public bool external = false;
    public float externalScale = 30000.0f;

    public bool controlsOn = false;

    public bool updateAllSats = false;

    public bool satSubset = false;
    
    // Start is called before the first frame update
    void Start()
    {
        fixTimeNow();
        timeScale = 1.0f;
        external=false;
        
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime * timeScale;
        currentTime = fixedTime.AddSeconds(timePassed);
    }

    public void setTimeScale(float newTimeScale){
        timeScale = newTimeScale;
        satSubset = timeScale > 120;
    }

    public void fixTimeNow(){
        fixedTime = DateTime.Now;
        timePassed = 0.0f;
        timeScale = 1.0f;
        updateAllSats = true;
    }

    public void setViewExternal(){
        external=true;
    }

    public void setViewInternal(){
        external=false;
    }

    public void toggleControls(){
        controlsOn=!controlsOn;
    }
}
