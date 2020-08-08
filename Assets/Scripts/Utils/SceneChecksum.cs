using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.jlabarca.cpattern.Utils
{


    public static class SceneChecksum
    {
        private static List<string> _hashCodes;
        private static readonly List<string> IgnoredFields = new List<string>{"_elapsed", "_version", "m_Ptr", "smoothPosition"};

        [MenuItem("Utils/Calculate scene checksum")]
        public static void GetSceneCheckSum()
        {
            _hashCodes = new List<string>();
            var checksum = "";
            var monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var script in monoBehaviours.ToList())
            {
                try
                {
                    checksum += FieldsChecksum(script);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
            Debug.Log(ComputeHash(Encoding.Unicode.GetBytes(checksum)));
        }

        private static string FieldsChecksum(object script, bool force = false)
        {
            if (!force)
            {
                var hash = RuntimeHelpers.GetHashCode(script);
                if (_hashCodes.Contains(script.GetType().Name)) return string.Empty;
                _hashCodes.Add(script.GetType().Name);
            }

            var result = "";
            var fieldFields =  script.GetType().GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                BindingFlags.FlattenHierarchy);

            foreach (var field in fieldFields)
            {
                if (IgnoredFields.Contains(field.Name)) continue;

                var obj = field.GetValue(script);

                if (obj == null) continue;

                if (field.FieldType.IsClass)
                {
                    if(typeof(IEnumerable).IsAssignableFrom(field.FieldType))
                    {
                        foreach (var enumerableObj in (IEnumerable) obj)
                        {
                            result += FieldsChecksum(enumerableObj, true);
                        }
                    }
                    else
                    {
                        result += FieldsChecksum(obj);
                    }
                }
                else
                {
                    // obj.ToString() because obj still gives some variable results
                    // Debug.Log($"{field.Name} ({field.FieldType.FullName}) : {obj}");
                    result += GenerateKey(obj.ToString());
                }
            }
            return result;
        }

        private static bool IsGenericList(object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        private static string GenerateKey(object sourceObject)
        {
            return sourceObject != null ? sourceObject.GetHashCode().ToString() : string.Empty;
        }

        private static string ComputeHash(byte[] objectAsBytes)
        {
            var md5 = new MD5CryptoServiceProvider();
            try
            {
                var result = md5.ComputeHash(objectAsBytes);

                // Build the final string by converting each byte
                // into hex and appending it to a StringBuilder
                var sb = new StringBuilder();
                foreach (var t in result)
                {
                    sb.Append(t.ToString("X2"));
                }

                // And return it
                return sb.ToString();
            }
            catch (ArgumentNullException ane)
            {
                //If something occurred during serialization,
                //this method is called with a null argument.
                Console.WriteLine("Hash has not been generated.");
                return null;
            }
        }

        private static readonly Object locker = new Object();

        private static byte[] ObjectToByteArray(object objectToSerialize)
        {
            var fs = new MemoryStream();
            var formatter = new BinaryFormatter();
            try
            {
                //Here's the core functionality! One Line!
                lock (locker)
                {
                    formatter.Serialize(fs, objectToSerialize);
                }
                return fs.ToArray();
            }
            catch (SerializationException se)
            {
                Console.WriteLine("Error occurred during serialization. Message: " + se.Message);
                return null;
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
