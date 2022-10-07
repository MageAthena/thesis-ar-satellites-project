using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsController : MonoBehaviour
{

    public Globals globals;

    public GameObject controlPanel;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        controlPanel.active = globals.controlsOn;
    }
}
