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
    protected DHUI_FlightController _flightControl;
    

    protected List<float> errors = new List<float>();
    protected List<float> errorsRetargeted = new List<float>();
    protected List<float> errorsRot = new List<float>();

    private System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.GetCultureInfo("de");

    protected bool record = false;

    public Transform _baseTransform = null;
    public List<Transform> _transforms = new List<Transform>();
    // 90Hz

    private int maxCounter = 900;
    private int counter = 0;
    protected void FixedUpdate()
    {

        if (record)
        {
            errors.Add(Vector3.Distance(_target.position,_drone.position));
            errorsRetargeted.Add(GetHapticRetargetingDist());
            errorsRot.Add(Quaternion.Angle(_target.rotation, _drone.rotation));

            Debug.Log(Vector3.Distance(coll.ClosestPoint(_target.position), _target.position));

            if (counter == maxCounter)
            {
                NextTarget();
                return;
            }

            counter++;

        }

        if (Input.GetKeyDown(KeyCode.A)){
            if (record)
            {
            }
            else
            {
                Debug.Log("Start");
                errors = new List<float>();
                errorsRetargeted = new List<float>();
                errorsRot = new List<float>();
                record = true;
                counter = 0;

                _target = _transforms[0];
                DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(_transforms[0].position, _transforms[0].rotation, 0.2f);
                _flightControl.AddToFrontOfQueue(cmd, true, true);
            }
        }
    }

    private int target = 0;
    private bool back = false;
    private void NextTarget()
    {
        if (!back)
        {
            WriteFile("Test5_Target" + target + "_To.txt");
            back = true;

            _target = _baseTransform;
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(_baseTransform.position, _baseTransform.rotation, 0.2f);
            _flightControl.AddToFrontOfQueue(cmd, true, true);
        }
        else
        {
            WriteFile("Test5_Target" + target + "_Back.txt");
            back = false;
            target++;
            
            if (target >= _transforms.Count)
            {
                Debug.Log("Done Recording");
                record = false;
                return;
            }

            _target = _transforms[target];
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(_transforms[target].position, _transforms[target].rotation, 0.2f);
            _flightControl.AddToFrontOfQueue(cmd, true, true);
        }
        errors = new List<float>();
        errorsRetargeted = new List<float>();
        errorsRot = new List<float>();
        counter = 0;
    }

    protected void WriteFile(string _fileName)
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

    public BoxCollider coll;
    protected float GetHapticRetargetingDist()
    {
        return Vector3.Distance(coll.ClosestPoint(_target.position), _target.position);


        
        List<Transform> droneBoundingBox = _droneControl.contactFaceBoundingBox;
        float closestDronePointDist = float.MaxValue;
        int closestDronePointIndex = 0;

        List<Vector3> droneProjectedPoints = new List<Vector3>();
        foreach (Transform t in droneBoundingBox)
        {
            droneProjectedPoints.Add(StaticContactPlane.GetProjectedPoint(t.position));
        }
        Vector3 topleft = _target.InverseTransformPoint(droneProjectedPoints[0]);
        Vector3 bottomright = _target.InverseTransformPoint(droneProjectedPoints[3]);

        if (0 < topleft.x && 0 < topleft.y && 0 > bottomright.x && 0 > bottomright.y)
        {
            Debug.Log(true);
            return Vector3.Distance(_droneControl.contactPointTransform.position,StaticContactPlane.GetProjectedPoint(_droneControl.contactPointTransform.position));
            
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
