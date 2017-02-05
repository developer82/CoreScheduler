namespace CoreScheduler
{
    public interface ITypedHandlerContainer
    {
        void Invoke(object obj);
        void Invoke(object obj1, object obj2);
    }
}
