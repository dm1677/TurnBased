public interface IHandler
{
    bool Process();
    void Reverse(); //Maybe this should be an abstract class instead, as this method is not always implemented
}