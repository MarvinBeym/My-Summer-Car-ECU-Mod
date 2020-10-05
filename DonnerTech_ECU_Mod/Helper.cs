using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    static public class Helper
    {
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }
        public static string CreatePathIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
               Directory.CreateDirectory(path);
            }
            return path;
        }

        public static bool ApproximatelyVector(Vector3 me, Vector3 other, float allowedDifference = 0)
        {
            var dx = me.x - other.x;
            if (Mathf.Abs(dx) > allowedDifference)
                return false;

            var dy = me.y - other.y;
            if (Mathf.Abs(dy) > allowedDifference)
                return false;

            var dz = me.z - other.z;

            return Mathf.Abs(dz) >= allowedDifference;
        }

        public static bool ApproximatelyQuaternion(this Quaternion quatA, Quaternion value, float acceptableRange = 0)
        {
            return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
        }
    }
}
