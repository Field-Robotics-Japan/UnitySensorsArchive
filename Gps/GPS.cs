using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FRJ.Sensors
{
    public class GPS : MonoBehaviour
    {
        [SerializeField] private double _baseLatitude = 35.71020206575301;      // 基地局の緯度 [m]
        [SerializeField] private double _baseLongitude = 139.81070039691542;    // 基地局の経度 [m]
        [SerializeField] private double _baseAltitude = 3.0;                    // 基地局の標高（海抜高さ）[m]
        [SerializeField] private int    _satelliteNum = 8;                      // 使用衛星数
        [SerializeField] private double _horizontalUnderAccuracyRate = 1.0;     // 水平精度低下率
        [SerializeField] private double _geoidHeight = 36.7071;                 // ジオイド高 [m]

        private double _latitude;   // 緯度 [m]
        private double _longitude;  // 経度 [m]
        private double _altitude;   // 標高 [m]

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
