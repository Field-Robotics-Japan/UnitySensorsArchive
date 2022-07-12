using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRJ.Sensor
{
    public class GPS : MonoBehaviour
    {
        [SerializeField] private double _baseLatitude = 35.71020206575301;      // ��n�ǂ̈ܓx [m]
        [SerializeField] private double _baseLongitude = 139.81070039691542;    // ��n�ǂ̌o�x [m]
        [SerializeField] private double _baseAltitude = 3.0;                    // ��n�ǂ̕W���i�C�������j[m]
        [SerializeField] private uint   _satelliteNum = 8;                      // �g�p�q����
        [SerializeField] private double _HDOP = 1.0;                            // �������x�ቺ��
        [SerializeField] private double _geoidHeight = 36.7071;                 // �W�I�C�h�� [m]
        [SerializeField] private float _updateRate = 10f; 

        private double _latitude;   // �ܓx [m]
        private double _longitude;  // �o�x [m]
        private double _altitude;   // �W�� [m]

        [Header("Informations(No need to input)")]
        [SerializeField] private string _gprmc;      // GPRMC message
        [SerializeField] private string _gpgga;      // GPGGA message
        // private string _gpvtg;      // GPVTG message
        // private string _gphdt;      // GPHDT message

        [SerializeField] private Vector3 _velocity;
        private Vector3 _pos_old;
        private float _time_old;
        private const float meterPerSec2knot = 1.9384f;

        public float updateRate{ get => this._updateRate; }
        
        public string gprmc { get => this._gprmc; }
        public string gpgga { get => this._gpgga; }
        // public string gpvtg { get => this._gpvtg; }
        // public string gphdt { get => this._gphdt; }

        private GeoCoordinate _gc;
        [SerializeField] private NMEASerializer _serializer;

        public void Init()
        {
            this._pos_old = this.transform.position;
            this._time_old = Time.time;

            this._gc = new GeoCoordinate(this._baseLatitude, this._baseLongitude);
            this._serializer = new NMEASerializer();
            this._serializer.GPGGA_DATA.quality = NMEASerializer.GPGGA_QUALITY.SPS;
            this._serializer.GPGGA_DATA.geoidLevel = (float)this._geoidHeight;
            this._serializer.GPGGA_DATA.satelliteNum = this._satelliteNum;
            this._serializer.GPGGA_DATA.hdop = (float)this._HDOP;
            this._serializer.GPRMC_DATA.status = false;
            this._serializer.GPRMC_DATA.mode = NMEASerializer.GPRMC_MODE.NONE;
        }

        public void updateGPS()
        {
            float time = Time.time;
            this._velocity = (this.transform.position - _pos_old) / (time - _time_old);
            _pos_old = this.transform.position;
            _time_old = time;

            (this._latitude, this._longitude) = this._gc.XZ2LatLon(this.transform.position.x, this.transform.position.z);
            this._altitude = this._baseAltitude + this.transform.position.y;

            this._serializer.latitude = (float)this._latitude;
            this._serializer.longitude = (float)this._longitude;

            this._serializer.GPGGA_DATA.altitude = (float)this._altitude;

            this._serializer.GPRMC_DATA.groundSpeed = Mathf.Sqrt(_velocity.x * _velocity.x + _velocity.z * _velocity.z)*meterPerSec2knot;
            float directionOfMovement = Mathf.Atan2(_velocity.x, _velocity.z)*Mathf.Rad2Deg;
            if (directionOfMovement < 0) directionOfMovement += 360.0f;
            this._serializer.GPRMC_DATA.directionOfMovement = directionOfMovement;

            this._gprmc = this._serializer.GPRMC();
            this._gpgga = this._serializer.GPGGA();
        }
    }
}
