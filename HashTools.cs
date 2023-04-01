using Godot;
using System;
using System.IO.Hashing;

namespace GodotVTT
{
    public static class HashTools
    {
        public static int GetImageHash(Image image) => BitConverter.ToInt32(Crc32.Hash(image.GetData()), 0);

        public static int GetImageHash(Texture2D texture) => GetImageHash(texture.GetImage());
    }
}
