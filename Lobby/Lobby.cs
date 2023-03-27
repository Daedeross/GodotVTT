using Godot;
using System;

namespace GodotVTT
{
    public partial class Lobby : Control
    {
        [Export]
        public PackedScene SessionScene { get; set; }

        Session _currentSession;

        Node _connectPanel = null;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _connectPanel = GetNode("Connect");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public void OnHostPressed()
        {
            CreateSession();
            _currentSession.StartServer();
            DetatchConnect();
            GD.Print("Session Started");
        }

        public void OnJoinPressed()
        {
            CreateSession();
            var ip = GetNode<LineEdit>("Connect/SessionAddress").Text;
            _currentSession.ConnectToServer(ip, Session.ServerPort);
            DetatchConnect();
            GD.Print("Session Joined");
        }

        public void CreateSession()
        {
            _currentSession = SessionScene.Instantiate<Session>();
            AddChild( _currentSession );
            _currentSession.ConnectionSucceeded += () => GD.Print("Connection Succeeded");
            _currentSession.ConnectionFailed += () => GD.Print("Connection Failed");
            _currentSession.PlayerListChanged += () => GD.Print("Player List Changed");
            _currentSession.SessionEnded += () => GD.Print("Session Ended");
            _currentSession.SessionError += (msg) => GD.PrintErr($"Error in session: {msg}");
        }

        private void DetatchConnect()
        {
            RemoveChild(_connectPanel);
        }
    }
}