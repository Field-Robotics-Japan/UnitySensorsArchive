using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRJ.Sensor
{
    public class DCamera : MonoBehaviour
    {
        [SerializeField] private Camera _cam;
        [SerializeField] private ComputeShader _depthShader;

        [SerializeField] private Vector2Int _resolution = new Vector2Int(640, 480);

        [SerializeField, Range(0.0001f, 179.0f)] private float _verticalFOV = 60.0f;

        [SerializeField, Range(0.01f, 1000.0f)] private float _minDistance = 0.3f;
        [SerializeField, Range(0.01f, 1000.0f)] private float _maxDistance = 1000.0f;

        [SerializeField] private float _scanRate = 20.0f;

#if UNITY_EDITOR
        [SerializeField] private bool _drawPoints;
        [SerializeField, Range(0.0f, 1.0f)] private float _pointSize = 0.1f; 
#endif

        private RenderTexture _rt_color = null;
        private RenderTexture _rt_depth = null;

        private ComputeBuffer _data_cb;
        private byte[] _data;

        private float _time_old;

        public Vector2Int resolution { get => this._resolution; }
        
        public float scanRate { get => this._scanRate; } 

        public byte[] data { get => this._data; }

        private void Awake()
        {
            _rt_color = new RenderTexture(_resolution.x, _resolution.y, 0, RenderTextureFormat.ARGB32);
            _rt_depth = new RenderTexture(_resolution.x, _resolution.y, 32, RenderTextureFormat.Depth);
            _data_cb = new ComputeBuffer(_resolution.x*_resolution.y*16, sizeof(float));
            _data = new byte[_resolution.x*_resolution.y*16];
        }

        private void Start()
        {
            _cam.fieldOfView = _verticalFOV;
            _cam.SetTargetBuffers(_rt_color.colorBuffer, _rt_depth.depthBuffer);
            _cam.nearClipPlane = _minDistance;
            _cam.farClipPlane = _maxDistance;

            float n_inv = 1.0f / _minDistance;
            float f_inv = 1.0f / _maxDistance;
            
            _depthShader.SetFloat("n_f", n_inv - f_inv);
            _depthShader.SetFloat("f", f_inv);
            _depthShader.SetInt("width", _resolution.x);
            _depthShader.SetInt("height", _resolution.y);
            _depthShader.SetFloat("vDisW", _resolution.x * 0.5f / Mathf.Tan(_cam.fieldOfView * _cam.aspect * 0.5f * Mathf.Deg2Rad));
            _depthShader.SetFloat("vDisH", _resolution.y * 0.5f / Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
            _depthShader.SetTexture(0, "colorBuffer", _rt_color);
            _depthShader.SetTexture(0, "depthBuffer", _rt_depth);
            _depthShader.SetBuffer(0, "data", _data_cb);
        }

        private void Update()
        {
            
        }

        private void OnPostRender()
        {
            float time_now = Time.time;
            if (time_now - _time_old < (1.0f / _scanRate)) return;
            _time_old = time_now;

            _depthShader.Dispatch(0, _resolution.x/16, _resolution.y/16, 1);
            _data_cb.GetData(_data);
        }

        private void OnValidate()
        {
            if(Application.isPlaying)return;
            
            if (_maxDistance < _minDistance) _maxDistance = _minDistance;
            _cam.fieldOfView = _verticalFOV;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawPoints || !Application.isPlaying) return;
            for (int i = 0; i < _resolution.x * _resolution.y * 16; i += 16)
            {
                Gizmos.color = new Color(_data[i + 14] / 255.0f, _data[i + 13] / 255.0f, _data[i + 12] / 255.0f);
                byte[] tmp = new byte[4];
                float x, y, z;
                tmp[0] = _data[i + 0];
                tmp[1] = _data[i + 1];
                tmp[2] = _data[i + 2];
                tmp[3] = _data[i + 3];
                x = BitConverter.ToSingle(tmp, 0);
                tmp[0] = _data[i + 4];
                tmp[1] = _data[i + 5];
                tmp[2] = _data[i + 6];
                tmp[3] = _data[i + 7];
                y = BitConverter.ToSingle(tmp, 0);
                tmp[0] = _data[i + 8];
                tmp[1] = _data[i + 9];
                tmp[2] = _data[i + 10];
                tmp[3] = _data[i + 11];
                z = BitConverter.ToSingle(tmp, 0);
                Gizmos.DrawSphere(this.transform.TransformVector(new Vector3(x, z, y)) + this.transform.position, _pointSize);
            }
        }
#endif
    }
}
