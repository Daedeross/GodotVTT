using Godot;
using System;

namespace GodotVTT
{
    public partial class VTT : Node
    {
        MapImporter _importer;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _importer = GetNode<MapImporter>("MapImporter");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {

        }

        public void OnFileIndexPressed(int index)
        {
            switch (index)
            {
                case 0:
                    OnImportBtnPressed();
                    break;
                default:
                    break;
            }
        }

        public void OnImportBtnPressed()
        {
            GetNode<FileDialog>("FileDialog").Show();
        }

        public void ImportMap(string path)
        {
            var map = _importer.LoadMapFromFile(path);
            AddChild(map);
        }
    }
}