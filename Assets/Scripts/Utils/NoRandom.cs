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
        public static float value => (float) GetValue();

        public static int Range(int min, int max)
        {
            var val = min + GetValue();
            if (val > min && val < max) return (int) val;
            return (min+max)/2;
        }

        public static float GetValue()
        {
            if (_index >= Seed.Length) _index = 0;
            var val = (Seed[_index++]  - 1)/2f;
            //if (_index >= Seed.Length) _index = 0;
            //val /= Seed[_index++];
            Debug.Log($"{val} ");
            return val;
        }

        public static float Range(float min, float max)
        {
            var val = min + GetValue();
            if (val > min && val < max) return val;
            return (min+max)/2;
        }
    }
}
