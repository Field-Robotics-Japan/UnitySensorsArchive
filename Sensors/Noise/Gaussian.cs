using System;
using UnityEngine;

namespace SensorNoise
{
    public class Gaussian
    {
        private System.Random random;

        public Gaussian()
        {
            this.random = new System.Random(Environment.TickCount);
        }

        public Gaussian(int seed)
        {
            this.random = new System.Random(seed);
        }

        public double Apply(double value, double sigma = 1.0d)
        {
            // using Box-Muller Method
            //double rand = 0.0d;
            double rand2 = this.random.NextDouble();
            //double normrand = Math.Sqrt(-2.0d * Math.Log(rand)) * Math.Cos(2.0d * Math.PI * rand2);
            double normrand = Math.Sqrt(-2.0d * Math.Log(0.0d)) * Math.Cos(2.0d * Math.PI * rand2);
            normrand = normrand * sigma + value;
            return normrand;
        }

        public Vector3 Apply(Vector3 value, Vector3 sigma)
        {
            Vector3 data;
            data.x = (float)Apply(value.x, sigma.x);
            data.y = (float)Apply(value.y, sigma.y);
            data.z = (float)Apply(value.z, sigma.z);
            return data;
        }

        public Vector4 Apply(Vector4 value, Vector4 sigma)
        {
            Vector4 data;
            data.x = (float)Apply(value.x, sigma.x);
            data.y = (float)Apply(value.y, sigma.y);
            data.z = (float)Apply(value.z, sigma.z);
            data.w = (float)Apply(value.w, sigma.w);
            return data;
        }
    }
}