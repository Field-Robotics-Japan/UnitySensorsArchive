using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class Imu : MonoBehaviour
{
    private Rigidbody rb;
    private Transform trans;

    // Previous value
    private Vector3 lastVelocity = Vector3.zero;

    private Vector4 geometryQuaternion;
    private Vector3 angularVelocity;
    private Vector3 linearAcceleration;
    
    public Vector4 GeometryQuaternion
    {
        get { return this.geometryQuaternion; }
    }
    public Vector3 AngularVelocity
    {
        get { return this.angularVelocity; }
    }
    public Vector3 LinearAcceleration
    {
        get { return this.linearAcceleration; }
    }

    private SensorNoise.Gaussian gaussianNoise;
    private SensorNoise.Bias biasNoise;

    public bool enableGaussianNoise;
    public bool enableBiasNoise;
    public NoiseSetting setting = new NoiseSetting();

    [System.Serializable]
    public class NoiseSetting
    {
        public Vector4 quatSigma;
        public Vector4 quatBias;
        public Vector3 angVelSigma;
        public Vector3 angVelBias;
        public Vector3 linAccSigma;
        public Vector3 linAccBias;
    }

    private void Start()
    {
        this.trans = GetComponent<Transform>();
        this.rb = GetComponent<Rigidbody>();
        this.geometryQuaternion = new Vector4();
        this.angularVelocity = new Vector3();
        this.linearAcceleration = new Vector3();
    }

    private void FixedUpdate()
    {
        UpdateImu();
    }

    private void UpdateImu()
    {
        // Update Object State //

        // Calculate Move Element
        Vector3 localLinearVelocity = this.trans.InverseTransformDirection(this.rb.velocity);
        Vector3 acceleration = (localLinearVelocity - this.lastVelocity) / Time.deltaTime;
        this.lastVelocity = localLinearVelocity;
        // Add Gravity Element
        acceleration += this.trans.InverseTransformDirection(Physics.gravity);

        // Update //

        // Raw
        this.geometryQuaternion = new Vector4(this.trans.rotation.x, this.trans.rotation.y, this.trans.rotation.z, this.trans.rotation.w);
        this.angularVelocity = this.rb.angularVelocity;
        this.linearAcceleration = acceleration;

        // Apply Gaussian Noise
        if (this.enableGaussianNoise) { this.geometryQuaternion = this.gaussianNoise.Apply(this.geometryQuaternion, this.setting.quatSigma); }
        if (this.enableGaussianNoise) { this.angularVelocity    = this.gaussianNoise.Apply(this.angularVelocity,    this.setting.angVelSigma); }
        if (this.enableGaussianNoise) { this.linearAcceleration = this.gaussianNoise.Apply(this.linearAcceleration, this.setting.linAccSigma); }

        // Apply Bias Noise
        if (this.enableBiasNoise) { this.geometryQuaternion = this.biasNoise.Apply(this.geometryQuaternion, this.setting.quatSigma); }
        if (this.enableBiasNoise) { this.angularVelocity    = this.biasNoise.Apply(this.angularVelocity,    this.setting.angVelSigma); }
        if (this.enableBiasNoise) { this.linearAcceleration = this.biasNoise.Apply(this.linearAcceleration, this.setting.linAccSigma); }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Imu))]
    public class ImuEditor : Editor
    {
        private Imu variables;

        private void Awake()
        {
            this.variables = target as Imu;
        }

        // inspectorÇÃGUIê›íË
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            this.variables.enableGaussianNoise = EditorGUILayout.ToggleLeft("Enable Gaussian Noise", this.variables.enableGaussianNoise);
            if (this.variables.enableGaussianNoise)
            {
                EditorGUILayout.LabelField("Gaussian Noise Setting");
                this.variables.setting.quatSigma    = EditorGUILayout.Vector4Field("->Quaternion Sigma", this.variables.setting.quatSigma);
                this.variables.setting.angVelSigma  = EditorGUILayout.Vector3Field("->AngularVelocity Sigma", this.variables.setting.angVelSigma);
                this.variables.setting.linAccSigma  = EditorGUILayout.Vector3Field("->LinearAcceleration Sigma", this.variables.setting.linAccSigma);
            }
            this.variables.enableBiasNoise = EditorGUILayout.ToggleLeft("Enable Bias Noise", this.variables.enableBiasNoise);
            if (this.variables.enableBiasNoise)
            {
                EditorGUILayout.LabelField("Bias Noise Setting");
                this.variables.setting.quatBias     = EditorGUILayout.Vector4Field("->Quaternion Bias", this.variables.setting.quatBias);
                this.variables.setting.angVelBias   = EditorGUILayout.Vector3Field("->AngularVelocity Bias", this.variables.setting.angVelBias);
                this.variables.setting.linAccBias   = EditorGUILayout.Vector3Field("->LinearAcceleration Bias", this.variables.setting.linAccBias);
            }

            // GUIÇÃçXêVÇ™Ç†Ç¡ÇΩÇÁé¿çs
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this.variables);
            }
        }
    }
#endif

}
