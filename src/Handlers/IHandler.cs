public interface IHandler
{
    bool Process(Action action);
    void Reverse(); //Maybe this should be an abstract class instead, as this method is not always implemented
}