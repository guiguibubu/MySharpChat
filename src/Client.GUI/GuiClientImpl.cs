using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;

namespace MySharpChat.Client.GUI
{
    internal class GuiClientImpl : BaseClientImpl
    {
        public GuiClientImpl() : base()
        { }

        public void SetUsername(string? username)
        {
            if(!string.IsNullOrEmpty(username))
                Username = username;
        }

        public override void Run(Client client)
        {
            if (networkModule.IsConnected() && networkModule.HasDataAvailable)
            {
                List<PacketWrapper> packets = networkModule.Read(TimeSpan.FromSeconds(1));
                foreach (PacketWrapper packet in packets)
                {
                    if (packet.Package is ClientInitialisationPacket connectInitPackage)
                    {
                        bool isInitialised = ClientId != Guid.Empty;
                        if (isInitialised)
                        {
                            string newUsername = connectInitPackage.Username;
                            if (!string.IsNullOrEmpty(newUsername))
                                Username = newUsername;
                        }
                        else
                        {
                            ClientId = connectInitPackage.SessionId;
                            // Tell the server our username
                            ClientInitialisationPacket initPacket = new ClientInitialisationPacket(ClientId, Username);
                            PacketWrapper packetWrapper = new PacketWrapper(ClientId, initPacket);
                            NetworkModule.Send(packetWrapper);
                        }

                    }
                    else if (packet.Package is ChatPacket chatPackage)
                    {
                        HandleChatPacket(chatPackage);
                    }
                }
            }
        }

        public event Action<string> ChatMessageReceivedEvent = (string message) => {};

        private void HandleChatPacket(ChatPacket chatPacket)
        {
            string readText = chatPacket.Message;
            if (!string.IsNullOrEmpty(readText))
            {
                ChatMessageReceivedEvent(readText);
            }
        }
    }
}
