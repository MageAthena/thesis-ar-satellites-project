using One_Sgp4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SatController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
 private MeshRenderer m_MeshRender;

 public GameObject label;
 public GameObject labelText;
 public GameObject cameraRig;
 public Globals globals;

 private bool pointerOn = false;
 private bool toggled = false;

 float baseScale = 0.0125f;
 float lowOpacity = 0.25f;

 void Awake () {
    m_MeshRender = transform.GetComponent<MeshRenderer>();

    transform.localScale = Vector3.one * baseScale;

 }

 void Update()
 {

    if (pointerOn){   
         
 
        Vector3 newPos = transform.position;
        label.transform.position = newPos;
        label.transform.rotation = cameraRig.transform.rotation;

        labelText.GetComponent<Text>().text = transform.name;
    }

    transform.GetComponent<SphereCollider>().enabled = !globals.controlsOn;


    Color c = m_MeshRender.material.color;
    if (toggled){
        c.a = 1.0f;
    }else{
        c.a = lowOpacity;
    }
    m_MeshRender.material.color = c;
 }

 //when pointer click, set the cube color to random color
 public void OnPointerClick(PointerEventData eventData)
 {
    toggled = !toggled;

 }

 //when pointer hover, set the cube color to green
 public void OnPointerEnter(PointerEventData eventData)
 {

     transform.localScale = Vector3.one * baseScale * 2.0f;
     pointerOn = true;
     label.GetComponent<Image>().enabled = true;
 }

 //when pointer exit hover, set the cube color to white
 public void OnPointerExit(PointerEventData eventData)
 {

     transform.localScale = Vector3.one * baseScale;
     pointerOn = false;
     label.GetComponent<Image>().enabled = false;
     labelText.GetComponent<Text>().text = "";
 }
}