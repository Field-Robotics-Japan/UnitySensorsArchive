using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRJ.Sensors
{
    public class GPS : MonoBehaviour
    {
        [SerializeField] private double _baseLatitude = 35.71020206575301;      // ��n�ǂ̈ܓx [m]
        [SerializeField] private double _baseLongitude = 139.81070039691542;    // ��n�ǂ̌o�x [m]
        [SerializeField] private double _baseAltitude = 3.0;                    // ��n�ǂ̕W���i�C�������j[m]
        [SerializeField] private int    _satelliteNum = 8;                      // �g�p�q����
        [SerializeField] private double _horizontalUnderAccuracyRate = 1.0;     // �������x�ቺ��
        [SerializeField] private double _geoidHeight = 36.7071;                 // �W�I�C�h�� [m]

        private double _latitude;   // �ܓx [m]
        private double _longitude;  // �o�x [m]
        private double _altitude;   // �W�� [m]

        public double Latitude { get => this._latitude; }
        public double Longitude { get => this._longitude; }
        public double Altitude { get => this._altitude; }
        public int SatelliteNum { get => _satelliteNum; }
        public double HorizontalUnderAccuracyRate { get => _horizontalUnderAccuracyRate; }
        public double GeoidHeight { get => _geoidHeight; }

        GeoCoordinate gc;

        void Start() => gc = new GeoCoordinate(this._baseLatitude, this._baseLongitude);

        void Update()
        {
            (this._latitude, this._longitude) = gc.XZ2LatLon(this.transform.position.x, this.transform.position.z);
            this._altitude = this._baseAltitude + this.transform.position.y;

            Debug.Log((_latitude, _longitude, _altitude));
        }
    }
}
