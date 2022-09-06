using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FRJ.Sensor
{
    [ExecuteInEditMode]
    public class Camera : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private UnityEngine.Camera _cam;
        [SerializeField] private ComputeShader _depthShader;

        [SerializeField] private Vector2Int _resolution = new Vector2Int(640, 480);
        
        //[SerializeField, Range(0.0001f, 179.0f)] private float _verticalFOV = 60.0f;
        //[SerializeField, Range(0.0001f, 179.0f)] private float _horizontalFOV = 91.45445f;
        
        [SerializeField, Range(0.02f, 1000.0f)] private float _minDistance = 0.3f;
        [SerializeField, Range(0.02f, 1000.0f)] private float _maxDistance = 1000.0f;

        [SerializeField] private float _scanRate = 20.0f;

#if UNITY_EDITOR
        [SerializeField] private bool _drawPoints;
        [SerializeField, Range(0.0f, 1.0f)] private float _pointSize = 0.1f;
#endif

        public Vector2Int resolution { get => this._resolution; }
        public float scanRate { get => this._scanRate; }

        [HideInInspector] public bool isInit = false;

        [Header("Informations(No need to input)")]

        // Render Textures
        private RenderTexture _rt_color = null;
        private RenderTexture _rt_depth = null;

        // Texutre2D
        private Texture2D _tex2d;

        // Results
        private ComputeBuffer _dataCB;
        private byte[] _data_pc;    // PointCloud
        private byte[] _data_img;   // JPEG Image

        private float _time_old;

        public byte[] data_pc { get => this._data_pc; }
        public byte[] data_ig { get => this._data_img; }

        #region FOV settings
        private float _vFOV_old;
        private float _hFOV_old;

        public float aspect { get => this._cam.aspect; }

        /*
        private void UpdateFOV()
        {
            if (!_cam) return;
            if (_verticalFOV == _vFOV_old && _horizontalFOV == _hFOV_old) return;
            _cam.fieldOfView = _verticalFOV;
            _cam.aspect = Mathf.Tan(_horizontalFOV * 0.5f * Mathf.Deg2Rad) /
                            Mathf.Tan(_verticalFOV * 0.5f * Mathf.Deg2Rad);
            _vFOV_old = _verticalFOV;
            _hFOV_old = _horizontalFOV;
        }

        public void ResetAspectRatio()
        {
            if (!_cam) return;
            _cam.ResetAspect();
            _cam.fieldOfView = _verticalFOV;
            _horizontalFOV = _verticalFOV*_cam.aspect;//Mathf.Atan(Mathf.Tan(_verticalFOV * 0.5f * Mathf.Deg2Rad) * Screen.width / Screen.height) * Mathf.Rad2Deg * 2.0f;
            UpdateFOV();
        }
        */
        #endregion

        public void Init()
        {
            if (isInit) return;
            _rt_color = new RenderTexture(_resolution.x, _resolution.y, 0, RenderTextureFormat.ARGB32);
            _rt_depth = new RenderTexture(_resolution.x, _resolution.y, 32, RenderTextureFormat.Depth);

            _tex2d = new Texture2D(_resolution.x, _resolution.y);
            _tex2d.Apply();

            _cam.SetTargetBuffers(_rt_color.colorBuffer, _rt_depth.depthBuffer);

            _data_pc = new byte[_resolution.x * _resolution.y * 16];
            _dataCB = new ComputeBuffer(_resolution.x * _resolution.y * 4, sizeof(float));

            _cam.nearClipPlane = _minDistance - 0.01f;
            _cam.farClipPlane = _maxDistance + 0.01f;

            float n_inv = 1.0f / _cam.nearClipPlane;
            float f_inv = 1.0f / _cam.farClipPlane;

            _depthShader.SetFloat("n_f", n_inv - f_inv);
            _depthShader.SetFloat("f", f_inv);
            _depthShader.SetInt("width", _resolution.x);
            _depthShader.SetInt("height", _resolution.y);
            _depthShader.SetFloat("vDisW", _resolution.x * 0.5f / Mathf.Tan(_cam.fieldOfView * _cam.aspect * 0.5f * Mathf.Deg2Rad));
            _depthShader.SetFloat("vDisH", _resolution.x * 0.5f / Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad));
            _depthShader.SetTexture(0, "_depthBuffer", _rt_depth);
            _depthShader.SetTexture(0, "_colorBuffer", _rt_color);

            _depthShader.SetBuffer(0, "_data", _dataCB);

            isInit = true;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (_maxDistance < _minDistance) _maxDistance = _minDistance;
                //UpdateFOV();
                return;
            }
            if (!isInit) return;
        }

        private void OnPostRender()
        {
            if (!Application.isPlaying || !isInit) return;

            float time_now = Time.time;
            if (time_now - _time_old < (1.0f / this.scanRate)) return;
            _time_old = time_now;

            _depthShader.Dispatch(0, _rt_depth.width / 16, _rt_color.height / 16, 1);
            _dataCB.GetData(_data_pc);

            for (int h = 0; h < _resolution.y; h++)
            {
                for (int w = 0; w < _resolution.x; w++)
                {
                    int index = h * _resolution.x + w;
                    _tex2d.SetPixel(w, h, new Color(_data_pc[index + 12] / 255.0f, _data_pc[index + 13] / 255.0f, _data_pc[index + 14] / 255.0f));
                }
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawPoints || !Application.isPlaying) return;
            for (int i = 0; i < _resolution.x * _resolution.y * 16; i += 16)
            {
                Gizmos.color = new Color(_data_pc[i + 14] / 255.0f, _data_pc[i + 13] / 255.0f, _data_pc[i + 12] / 255.0f);
                byte[] tmp = new byte[4];
                float x, y, z;
                tmp[0] = _data_pc[i + 0];
                tmp[1] = _data_pc[i + 1];
                tmp[2] = _data_pc[i + 2];
                tmp[3] = _data_pc[i + 3];
                x = BitConverter.ToSingle(tmp, 0);
                tmp[0] = _data_pc[i + 4];
                tmp[1] = _data_pc[i + 5];
                tmp[2] = _data_pc[i + 6];
                tmp[3] = _data_pc[i + 7];
                y = BitConverter.ToSingle(tmp, 0);
                tmp[0] = _data_pc[i + 8];
                tmp[1] = _data_pc[i + 9];
                tmp[2] = _data_pc[i + 10];
                tmp[3] = _data_pc[i + 11];
                z = BitConverter.ToSingle(tmp, 0);
                Gizmos.DrawSphere(this.transform.position + new Vector3(-x, z, y), _pointSize);
            }
        }
#endif

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FRJ.Sensor.Camera))]
public class CameraEditor : Editor
{
    private FRJ.Sensor.Camera _cam;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Reset Aspect Ratio"))
        {
            //_cam.ResetAspectRatio();
            this.serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.LabelField("Aspect Ratio", _cam.aspect.ToString("F4"));

        if (GUILayout.Button("Initialize"))
        {
            _cam.Init();
            this.serializedObject.ApplyModifiedProperties();

            Debug.Log("Camera initialized");
        }

        if (GUILayout.Button("Reset"))
        {
            _cam.isInit = false;
            this.serializedObject.ApplyModifiedProperties();
            Debug.Log("Camera reset");
        }
    }

    private void OnEnable()
    {
        _cam = this.target as FRJ.Sensor.Camera;
    }
}

#endif