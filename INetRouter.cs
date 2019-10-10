public interface INetRouter : IRouter {
    string[] GetConnections();
    // string GetConnection();
    void SetConnection(string name);
}