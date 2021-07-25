using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocs.Sensor
{
    public class Lidar3D : MonoBehaviour
    {
        [SerializeField, Tooltip("Sampling update rate.")]
        private int _updateRate = 180;
        [SerializeField, Tooltip("Number of samples per layer.[1, inf]")]
        private int _samples = 360;
        [SerializeField, Tooltip("Number of layer.[2, inf]")]
        private int _layers = 16;
        [SerializeField, Tooltip("Minimum sampling angle in vertical direction.[-90, Angle Max]")]
        private float _angleMin = -10;
        [SerializeField, Tooltip("Maximum sampling angle in vertical direction.[Angle Min, 90]")]
        private float _angleMax = 10;
        [SerializeField, Tooltip("Minimum sampling distance.[0, Range Max]")]
        private float _rangeMin = 0.12f;
        [SerializeField, Tooltip("Maximum sampling distance.[Range Min, inf]")]
        private float _rangeMax = 100f;

        [SerializeField, Tooltip("The color of the irradiation point visualized in the Scene view.")]
        private Color _gizmoColor = Color.green;
        [SerializeField, Tooltip("The size of the irradiation point visualized in the Scene view.[0, inf]")]
        private float _gizmoSize = 0.01f;

        private float _layerIncrement;
        private float _azimuthIncrement;
        private float[] _azimuth;
        private float[] _distances;
        private Vector3[] _hitPoints;

        public int Samples { get => _samples; }
        public int Layers { get => _layers; }
        public float[] Azimuth { get => _azimuth; }
        public float[] Distances { get => _distances; }
        public Vector3[] HitPoints { get => _hitPoints; }

        private void Awake()
        {
            if (!ParameterCheck())
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
            }

            // Initialize params
            this._distances = new float[this._layers * this._samples];
            this._azimuth = new float[this._samples];
            this._layerIncrement = (this._angleMax - this._angleMin) / (this._layers - 1);
            this._azimuthIncrement = 360.0f / this._samples;
            this._hitPoints = new Vector3[this._layers * this._samples];
        }

        private void Update()
        {
            Scan();
        }

        public void Scan()
        {
            Vector3 dir;
            int indx = 0;
            float angle;

            //azimuth angles
            for (int incr = 0; incr < this._samples; incr++)
            {
                for (int layer = 0; layer < this._layers; layer++)
                {
                    this._azimuth[incr] = incr * this._azimuthIncrement;

                    indx = layer + incr * this._layers;
                    angle = this._angleMin + (float)layer * this._layerIncrement;
                    dir = this.transform.rotation * Quaternion.Euler(-angle, this._azimuth[incr], 0) * Vector3.forward;

                    if (Physics.Raycast(this.transform.position, dir, out RaycastHit hit, this._rangeMax))
                    {
                        this._distances[indx] = hit.distance;
                        this._hitPoints[indx] = this.transform.position + dir * hit.distance;
                    }
                    else
                    {
                        this._distances[indx] = this._rangeMax;
                        this._hitPoints[indx] = this.transform.position + dir * this._rangeMax;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (this._hitPoints != null && this._hitPoints.Length != 0)
            {
                Gizmos.color = this._gizmoColor;
                foreach (Vector3 p in this._hitPoints)
                {
                    Gizmos.DrawSphere(p, this._gizmoSize);
                }
            }
        }

        public bool ParameterCheck()
        {
            if (this._samples < 1)
            {
                Debug.LogError("[Samples] is invalid.");
                return false;
            }
            if (this._layers < 2)
            {
                Debug.LogError("[Layers] is invalid.");
                return false;
            }
            if (this._angleMin > this._angleMax)
            {
                Debug.LogError("[Angle Min/Max] is invalid.");
                return false;
            }
            if (this._rangeMin > this._rangeMax)
            {
                Debug.LogError("[Range Min/Max] is invalid.");
                return false;
            }

            return true;
        }
    }
}
