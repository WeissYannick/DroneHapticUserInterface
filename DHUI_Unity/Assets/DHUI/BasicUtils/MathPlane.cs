using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Utils
{
    public class MathPlane
    {
        private float A = 0;
        private float B = 0;
        private float C = 0;
        private float D = 0;
        

        public MathPlane(Transform _transform)
        {
            Generate(_transform.position, _transform.forward);
        }

        public MathPlane(Vector3 _point, Vector3 _normal)
        {
            Generate(_point, _normal);
        }
        private void Generate(Vector3 _point, Vector3 _normal)
        {
            _normal = _normal.normalized;

            A = _normal.x;
            B = _normal.y;
            C = _normal.z;

            D = -(_point.x * _normal.x + _point.y * _normal.y + _point.z * _normal.z);
        }

        public void LogNormalForm()
        {
            Debug.Log("E = " + A + "x + " + B + "y + " + C + "z + " + D);
        }

        public float GetDistance(Vector3 _p)
        {
            return A * _p.x + B * _p.y + C * _p.z + D;
        }
        
        public Vector3 GetProjectedPoint(Vector3 _p)
        {
            return _p - GetDistance(_p) * new Vector3(A, B, C);
        }

        public bool PointInFrontOfPlane(Vector3 _p)
        {
            return GetDistance(_p) < 0;
        }


    }

}