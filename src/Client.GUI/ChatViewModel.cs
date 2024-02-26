using System;

namespace MySharpChat.Client.GUI
{
    internal class ChatViewModel
    {
        public GuiClientImpl Client { get; private set; }
        public string InputMessage { get; set; } = "";

        public ChatViewModel(GuiClientImpl client)
        {
            Client = client;
            Client.DisconnectionEvent += OnDisconnection;
            Client.OnUserAddedEvent += OnUserAdded;
            Client.OnUserRemovedEvent += OnUserRemoved;
            Client.OnUsernameChangeEvent += OnUsernameChange;
            Client.ChatMessageReceivedEvent += OnMessageReceived;
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };
        public event Action<Guid> OnUserAddedEvent = (Guid idUser) => { };
        public event Action<Guid> OnUserRemovedEvent = (Guid idUser) => { };
        public event Action<Guid, string, string> OnUsernameChangeEvent = (Guid idUser, string s1, string s2) => { };
        public event Action OnLocalUsernameChangeEvent = () => { };
        public event Action<Guid> OnMessageReceivedEvent = (Guid idMessage) => { };
        public event Action OnSendFinishedEvent = () => { };

        public void OnSendSuccess()
        {
            OnSendFinishedEvent();
        }

        public void OnDisconnection(bool manual)
        {
            OnDisconnectionEvent(manual);
        }

        public void OnUserAdded(Guid idUser)
        {
            OnUserAddedEvent(idUser);
        }
        
        public void OnUserRemoved(Guid idUser)
        {
            OnUserRemovedEvent(idUser);
        }

        public void OnUsernameChange(Guid idUser, string oldUsername, string newUsername)
        {
            OnUsernameChangeEvent(idUser, oldUsername, newUsername);
        }

        public void OnLocalUsernameChange()
        {
            OnLocalUsernameChangeEvent();
        }

        public void OnMessageReceived(Guid idMessage)
        {
            if (idMessage != Guid.Empty)
            {
                OnMessageReceivedEvent(idMessage);
            }
        }
    }
}
