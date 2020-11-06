﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    static public class Helper
    {
        public static GameObject SetObjectNameTagLayer(GameObject gameObject, string name)
        {
            gameObject.name = name;
            gameObject.tag = "PART";

            gameObject.layer = LayerMask.NameToLayer("Parts");
            return gameObject;
        }
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

        public static Sprite LoadNewSprite(Sprite sprite, string FilePath, float pivotX = 0.5f, float pivotY = 0.5f, float PixelsPerUnit = 100.0f)
        {
            if (File.Exists(FilePath) && Path.GetExtension(FilePath) == ".png")
            {
                Sprite NewSprite = new Sprite();
                Texture2D SpriteTexture = LoadTexture(FilePath);
                NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(pivotX, pivotY), PixelsPerUnit);

                return NewSprite;
            }
            return sprite;
        }
        public static Texture2D LoadTexture(string FilePath)
        {
            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(FileData))
                    return Tex2D;
            }
            return null;
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
