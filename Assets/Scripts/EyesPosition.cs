using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyesPosition : MonoBehaviour {

    enum MoveMode
    {
        XY, XZ
    }

    public GameObject xObj;
    public GameObject yObj;
    public GameObject zObj;

    SetupLocation _myLocation;
    public float step = 0.005f;
    MoveMode _mode;

    void Start () {
        _mode = MoveMode.XZ;
        string ConfigFile =  Application.dataPath + "/config.txt";
        _myLocation = (SetupLocation)Enum.Parse(enumType: typeof(SetupLocation), value: ConfigProperties.load(ConfigFile, "setup.type"));

    }



    void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_mode == MoveMode.XZ) _mode = MoveMode.XY;
            else _mode = MoveMode.XZ;
        }

        if(_mode == MoveMode.XY)
        {
            zObj.SetActive(false);
            yObj.SetActive(true);
            xObj.SetActive(true);
        }
        else
        {
            zObj.SetActive(true);
            yObj.SetActive(false);
            xObj.SetActive(true);
        }

        int sign = 1;
        if (_myLocation == SetupLocation.LEFT) sign = -1;
        if (Input.GetKeyDown(KeyCode.RightArrow)) transform.position = transform.position + new Vector3(sign*step, 0, 0);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) transform.position = transform.position + new Vector3(sign * -step, 0, 0);

        if(_mode == MoveMode.XY)
        {

            if (Input.GetKeyDown(KeyCode.UpArrow)) transform.position = transform.position + new Vector3(0, -step, 0);
            if (Input.GetKeyDown(KeyCode.DownArrow)) transform.position = transform.position + new Vector3(0, step, 0);

        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) transform.position = transform.position + new Vector3(0, 0, sign * step);
            if (Input.GetKeyDown(KeyCode.DownArrow)) transform.position = transform.position + new Vector3(0, 0, sign * -step);

        }
    }
}
