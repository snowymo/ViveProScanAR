//========= Copyright 2017, HTC Corporation. All rights reserved. ===========

using UnityEngine;
using System.IO;

namespace Vive.Plugin.SR
{
    public static class ViveSR_FileTool
    {
        public static void SaveRenderTextureToJPG(RenderTexture renderTexture, string folderPath, string filename, string extension = ".jpg", int quality = 100)
        {
            Texture2D texture2d = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            texture2d.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2d.Apply();
            SaveTexture2DToJPG(texture2d, folderPath, filename, extension, quality);
        }

        public static void SaveTexture2DToJPG(Texture2D texture2d, string folderPath, string filename, string extension = ".jpg", int quality = 100)
        {
            byte[] bytes = texture2d.EncodeToJPG(quality);
            string rpcText = System.Convert.ToBase64String(bytes);
            bytes = System.Convert.FromBase64String(rpcText);
            File.WriteAllBytes(folderPath + filename + extension, bytes);
        }

        public static void SaveTexture2DToPNG(Texture2D texture2d, string folderPath, string filename, string extension = ".png")
        {
            byte[] bytes = texture2d.EncodeToPNG();
            string rpcText = System.Convert.ToBase64String(bytes);
            bytes = System.Convert.FromBase64String(rpcText);
            File.WriteAllBytes(folderPath + filename + extension, bytes);
        }

        public static bool LoadTexture2D(ref Texture2D texture2d, string folderPath, string filename, string extension)
        {
            texture2d = new Texture2D(texture2d.width, texture2d.height);
            if (!File.Exists(folderPath + filename + extension))
                return false;

            texture2d.LoadImage(File.ReadAllBytes(folderPath + filename + extension));
            return true;
        }

        public static void SaveSerialData<T>(T Data, string folderPath, string filename, string extension)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            StreamWriter stream = new StreamWriter(folderPath + filename + extension);
            var bformatter = new System.Xml.Serialization.XmlSerializer(Data.GetType());
            bformatter.Serialize(stream, Data);
            stream.Close();
        }

        public static bool LoadSerialData<T>(ref T Data, string folderPath, string filename, string extension)
        {
            if (!File.Exists(folderPath + filename + extension))
                return false;

            FileStream stream = new FileStream(folderPath + filename + extension, FileMode.Open);
            var bformatter = new System.Xml.Serialization.XmlSerializer(Data.GetType());
            Data = (T)bformatter.Deserialize(stream);
            stream.Close();
            return true;
        }

        public static bool CopyFile(string folderPath, string filename, string extension, string destination, bool overwrite = false)
        {
            if (!overwrite)
            {
                if (File.Exists(destination + filename + extension)) return true;
            }
            if (!File.Exists(folderPath + filename + extension))
            {
                Debug.LogError("[ViveSR] FileCopy Failed : file not found");
                return false;
            }
            File.Copy(folderPath + filename + extension, destination + filename + extension, overwrite);
            return true;
        }
    }
}