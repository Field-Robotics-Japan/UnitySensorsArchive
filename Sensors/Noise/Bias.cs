using System;
using UnityEngine;

namespace SensorNoise
{
    public class Bias
    {
        public Bias()
        {
        }

        public double Apply(double value, double bias = 0.0d)
        {
            return value + bias;
        }
        public Vector3 Apply(Vector3 value, Vector3 bias)
        {
            return value + bias;
        }
        public Vector4 Apply(Vector4 value, Vector4 bias)
        {
            return value + bias;
        }
    }
}