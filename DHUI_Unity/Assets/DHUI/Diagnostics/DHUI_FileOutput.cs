using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DHUI.Core;
using DHUI;

public class DHUI_FileOutput : MonoBehaviour
{
    [SerializeField]
    protected Transform _target;
    [SerializeField]
    protected Transform _drone;
    [SerializeField]
    protected DHUI_DroneController _droneControl;
    [SerializeField]
    protected string _fileName;

    protected List<float> errors = new List<float>();
    protected List<float> errorsRetargeted = new List<float>();
    protected List<float> errorsRot = new List<float>();

    private System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.GetCultureInfo("de");

    protected bool record = false;
    // 90Hz
    protected void FixedUpdate()
    {
        if (record)
        {
            errors.Add(Vector3.Distance(_target.position,_drone.position));
            errorsRetargeted.Add(GetHapticRetargetingDist());
            errorsRot.Add(Quaternion.Angle(_target.rotation, _drone.rotation));
            Debug.Log(errors.Count);
        }

        if (Input.GetKeyDown(KeyCode.A)){
            if (record)
            {
                WriteFile();
                record = false;
            }
            else
            {
                errors = new List<float>();
                errorsRetargeted = new List<float>();
                errorsRot = new List<float>();
                record = true;
            }
        }
    }

    protected void WriteFile()
    {

#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
        var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, _fileName);
        StreamWriter writer = new StreamWriter(filePath, false);

        //writer.WriteLine("Time; ErrorX, ErrorY, ErrorZ, ErrorDist");

        for (int i = 0; i < errors.Count; ++i)
        {
            writer.WriteLine(i + ";" + FloatToString(errors[i]) + ";" + FloatToString(errorsRetargeted[i]) + ";" + FloatToString(errorsRot[i]));
        }


        writer.Flush();
        writer.Close();
    }

    protected string FloatToString(float _v)
    {
        return _v.ToString(culture);
    }

    public DHUI.Utils.MathPlane StaticContactPlane
    {
        get { return new DHUI.Utils.MathPlane(_target); }
    }


    protected float GetHapticRetargetingDist()
    {
        List<Transform> droneBoundingBox = _droneControl.contactFaceBoundingBox;
        float closestDronePointDist = float.MaxValue;
        int closestDronePointIndex = 0;

        List<Vector3> droneProjectedPoints = new List<Vector3>();
        foreach (Transform t in droneBoundingBox)
        {
            droneProjectedPoints.Add(StaticContactPlane.GetProjectedPoint(t.position));
        }
        
        Vector3 touchablePP = StaticContactPlane.GetProjectedPoint(_target.position);
        if (touchablePP.x > droneProjectedPoints[0].x && touchablePP.y < droneProjectedPoints[0].y && touchablePP.x < droneProjectedPoints[droneProjectedPoints.Count - 1].x && touchablePP.y > droneProjectedPoints[droneProjectedPoints.Count - 1].y)
        {
            return Vector3.Distance(_target.position, _target.position + _droneControl.contactPointTransform.position - StaticContactPlane.GetProjectedPoint(_droneControl.contactPointTransform.position));

        }
        else
        {
            for (int dronePointCounter = 0; dronePointCounter < droneBoundingBox.Count; dronePointCounter++)
            {
                float dist = Vector3.Distance(_target.position, droneBoundingBox[dronePointCounter].position);
                if (dist < closestDronePointDist)
                {
                    closestDronePointIndex = dronePointCounter;
                    closestDronePointDist = dist;
                }
            }

            return Vector3.Distance(droneBoundingBox[closestDronePointIndex].position, _target.position);
        }
    }
}
