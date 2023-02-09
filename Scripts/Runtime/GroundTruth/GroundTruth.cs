using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRJ.Sensor
{
  [RequireComponent(typeof(Rigidbody))]
  public class GroundTruth : MonoBehaviour
  {

    private Rigidbody _rb;
    private Transform _trans;

    private Vector3 _geometryEuclidean;
    private Vector4 _geometryQuaternion;

    [SerializeField] private float _updateRate = 100f;
    public float updateRate{ get => this._updateRate; }

    public Vector4 GeometryQuaternion { get => _geometryQuaternion; }
    public Vector3 GeometryEuclidean { get => _geometryEuclidean; }

    // Start is called before the first frame update
    private void Start()
    {
      this._trans = this.GetComponent<Transform>();
      this._rb = this.GetComponent<Rigidbody>();
      this._geometryQuaternion = new Vector4();
      this._geometryEuclidean = new Vector3();
    }

    // Update is called once per frame
    public void UpdateGroundTruth()
    {
      this._geometryEuclidean = new Vector3(this._trans.position.x, this._trans.position.y, this._trans.position.z);
      this._geometryQuaternion = new Vector4(this._trans.rotation.x, this._trans.rotation.y, this._trans.rotation.z, this._trans.rotation.w);
    }
  }
}