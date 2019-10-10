using System.Threading.Tasks;

public interface IRouter {
    Task Start();
    bool IsRunning();
    Task Stop();
}