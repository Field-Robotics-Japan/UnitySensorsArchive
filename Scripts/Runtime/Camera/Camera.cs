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
        [SerializeField, Range(0.0001f, 179.0f)] private float _verticalFOV = 60.0f;
        [SerializeField, Range(0.0001f, 179.0f)] private float _horizontalFOV = 91.45445f;

        [SerializeField, Range(0.01f, 1000.0f)] private float _minDistance = 0.3f;
        [SerializeField, Range(0.01f, 1000.0f)] private float _maxDistance = 1000.0f;

        [SerializeField] private float _scanRate = 20.0f;

        public Vector2Int resolution { get => this._resolution; }
        public float scanRate { get => this._scanRate; }

        [Header("Informations(No need to input)")]

        // Render Textures
        private RenderTexture _rt_color = null;
        private RenderTexture _rt_depth = null;

        // Results
        private ComputeBuffer _dataArrayCB;
        private byte[] _dataArray;

        public byte[] dataArray { get => this._dataArray; }

        [HideInInspector] public bool isInit = false;

        #region FOV settings
        private float _vFOV_old;
        private float _hFOV_old;

        public float aspect { get => this._cam.aspect; }

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
            _horizontalFOV = Mathf.Atan(Mathf.Tan(_verticalFOV * 0.5f * Mathf.Deg2Rad) * Screen.width / Screen.height) * Mathf.Rad2Deg * 2.0f;
        }
        #endregion

        public void Init()
        {
            if (isInit) return;
            _rt_color = new RenderTexture(_resolution.x, _resolution.y, 0, RenderTextureFormat.ARGB32);
            _rt_depth = new RenderTexture(_resolution.x, _resolution.y, 32, RenderTextureFormat.Depth);

            _cam.SetTargetBuffers(_rt_color.colorBuffer, _rt_depth.depthBuffer);

            _dataArray = new byte[_resolution.x * _resolution.y * 16];
            _dataArrayCB = new ComputeBuffer(_resolution.x * _resolution.y * 4, sizeof(float));

            float n_inv = 1.0f / _cam.nearClipPlane;
            float f_inv = 1.0f / _cam.farClipPlane;

            _depthShader.SetFloat("n_f", n_inv - f_inv);
            _depthShader.SetFloat("f", f_inv);
            _depthShader.SetInt("width", _resolution.x);
            _depthShader.SetInt("width_2",  _resolution.x / 2);
            _depthShader.SetInt("height_2", _resolution.y / 2);
            _depthShader.SetFloat("vDisW", _resolution.x * 0.5f / Mathf.Tan(_horizontalFOV * 0.5f * Mathf.Deg2Rad));
            _depthShader.SetFloat("vDisH", _resolution.y * 0.5f / Mathf.Tan(_verticalFOV * 0.5f * Mathf.Deg2Rad));
            _depthShader.SetTexture(0, "_depthBuffer", _rt_depth);
            _depthShader.SetTexture(0, "_colorBuffer", _rt_color);

            _depthShader.SetBuffer(0, "_data", _dataArrayCB);

            isInit = true;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (_maxDistance < _minDistance) _maxDistance = _minDistance;
                UpdateFOV();
                return;
            }
            if (!isInit) return;
        }
        private void OnPostRender()
        {
            if (!Application.isPlaying || !isInit) return;
            _depthShader.Dispatch(0, _rt_depth.width / 8, _rt_color.height / 8, 1);
            _dataArrayCB.GetData(_dataArray);
        }
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
            _cam.ResetAspectRatio();
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