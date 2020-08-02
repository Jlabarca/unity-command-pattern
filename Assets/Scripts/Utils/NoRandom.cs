using System;
using System.Globalization;
using UnityEngine;

namespace com.jlabarca.cpattern.Utils
{
    //Replacing random values to deterministic seed based
    internal static class NoRandom
    {
        //private const string Seed = "SvgjvxbfeejKdfGCWVEvHjIvzgOkcLmXessADdvYfiZoLLAnbkEuSPBUrKaOqukEDxKWzllUsODXORCZNCeoJKtxfNBOjmICgJrDytMUzEEoyPwPYXDrEjbxlJaYZuxmwCkVZnZhcaX";
        private const string Seed = "urJOHCYpEgyiLyLLOhscLtFhVQdBCpmUmpAIMAWLTRWAFWLxozqFEkfdcsbfldCCEFUXQRYISyYjdIUZjxuOByzVIXEHvVgOjTWL";
        private static int _index;
        public static float value => GetValue();

        public static int Range(int min, int max)
        {
            var step = (max - min) / 10f;
            var val = min + step * GetValue() * 10;
            var intVal = Math.Round(val);

            //Debug.Log($"{step} : {min} to {max} = {intVal} ");

            if (val > min && val < max) return (int) intVal;
            return (min+max)/2;
        }

        public static float GetValue()
        {
            if (_index >= Seed.Length) _index = 0;
            var val = 1 + Math.Pow(80 - (Seed[_index++] - 1), 2) / 100;

            if (val > 0) val = 1 / val;
            //if (_index >= Seed.Length) _index = 0;
            //val /= Seed[_index++];
            //Debug.Log($"{_index}: {val} ");

            return (float) val;
        }

        public static float Range(float min, float max)
        {
            var val = min + GetValue();
            if (val > min && val < max) return val;
            return (min+max)/2;
        }
    }
}
