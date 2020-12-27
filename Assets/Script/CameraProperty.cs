using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraProperty : MonoBehaviour
{

    [SerializeField] Text cameraName;
    [SerializeField] InputField inputDelay;

    private static CameraProperty _instance;
    private List<CameraObj> cameraList = new List<CameraObj>();

    private CameraObj currentEditCam = null;

    public static CameraProperty getInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    public void AddCamera(CameraObj cam)
    {
        if (cameraList.Exists(x => x == cam) == false)
            cameraList.Add(cam);
    }

    public void RemoveCamera(CameraObj cam)
    {
        if (currentEditCam != null && currentEditCam == cam)
        {
            Reset();
            currentEditCam = null;
        }

        if (cameraList.Exists(x => x == cam) == true)
            cameraList.Remove(cam);
    }

    public void SetSelectCamera(CameraObj cam)
    {
        cameraName.text = cam.cameraName;
        inputDelay.text = cam.delaytime.ToString();

        if (currentEditCam != null)
            currentEditCam.SetSelected(false);

        currentEditCam = cam;
        currentEditCam.SetSelected(true);
    }

    public void OnChangeDelayValue()
    {
        //Debug.Log(inputDelay.text);
        float delay = Convert.ToSingle(inputDelay.text);
        if (currentEditCam != null)
        {
            currentEditCam.delaytime = delay;
            CalculateAllCameraDelay();
        }
    }

    private void Reset()
    {
        cameraName.text = "";
        inputDelay.text = "";
    }

    // 실제 적용할 값들을 계산힌다.
    void CalculateAllCameraDelay()
    {
        float mindelaytime = 10000.0f;
        for(int i=0; i<cameraList.Count; i++)
        {
            if (cameraList[i].delaytime < mindelaytime)
                mindelaytime = cameraList[i].delaytime;
        }

        for (int i = 0; i < cameraList.Count; i++)
            cameraList[i].applydelaytime = -mindelaytime + cameraList[i].delaytime;
    }
}
