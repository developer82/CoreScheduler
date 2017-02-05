using System;

namespace CoreScheduler
{
    public class TypedHandlerContainer<T> : ITypedHandlerContainer
    {
        private readonly Action<T> _handler;

        public TypedHandlerContainer(Action<T> handler)
        {
            _handler = handler;
        }

        public void Invoke(object obj)
        {
            _handler?.Invoke((T)obj);
        }

        public void Invoke(object obj1, object obj2)
        {
            Invoke(obj1);
        }
    }
}
