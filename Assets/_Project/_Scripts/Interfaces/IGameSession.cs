using System;

public interface IGameSession : IDisposable
{
    void Initialize(int port, string lobbyName);
    void RefreshReceiver();
}