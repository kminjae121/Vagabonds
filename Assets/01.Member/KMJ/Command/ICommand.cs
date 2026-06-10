namespace _01.Member.KMJ.Command
{
    public interface ICommand
    {
        void Execute();
    }

    public interface ICommand<T>
    {
        void Execute(T value);
    }
}