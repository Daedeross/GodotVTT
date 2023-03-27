using Godot;
using Projects;
using System;

namespace GodotVTT
{
    /// <summary>
    /// Class that manages a single multiplayer session.
    /// </summary>
    public partial class Session : Node
    {
        public const int ServerPort = 6500;
        public const int MaxPlayers = 64;

        public Campaign Campaign { private set; get; }

        [Signal]
        public delegate void PlayerListChangedEventHandler();
        [Signal]
        public delegate void ConnectionFailedEventHandler();
        [Signal]
        public delegate void ConnectionSucceededEventHandler();
        [Signal]
        public delegate void SessionEndedEventHandler();
        [Signal]
        public delegate void SessionErrorEventHandler(string errorMessage);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Multiplayer.PeerConnected += this.PlayerConnected;
            Multiplayer.PeerDisconnected += PlayerDisconnected;
            Multiplayer.ConnectedToServer += () => EmitSignal(SignalName.ConnectionSucceeded);
            Multiplayer.ConnectionFailed += () => EmitSignal(SignalName.ConnectionFailed);
            Multiplayer.ServerDisconnected += ServerDisconected;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public void StartServer()
        {
            // TODO: following is prototype
            var peer = new ENetMultiplayerPeer();
            peer.CreateServer(ServerPort, MaxPlayers);
            GetTree().GetMultiplayer().MultiplayerPeer = peer;
        }

        public void ConnectToServer(string address, int port)
        {
            var peer = new ENetMultiplayerPeer();
            peer.CreateClient(address, port);
            GetTree().GetMultiplayer().MultiplayerPeer = peer;
        }

        public void LoadCampaign(Campaign campaign)
        {
            Campaign = campaign;
            AddChild(campaign);
            campaign.Owner = this;
        }

        #region Signal Handling

        private void PlayerConnected(long id)
        {
            GD.Print($"Player Connected id:{id}");
        }

        private void PlayerDisconnected(long id)
        {
            GD.Print($"Player Disconnected id:{id}");
        }

        private void ServerDisconected()
        {
            GD.Print("Disconected from server.");
        }

        #endregion
    }
}