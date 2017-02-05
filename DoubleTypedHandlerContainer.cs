using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreScheduler
{
    public class DoubleTypedHandlerContainer<T1, T2> : ITypedHandlerContainer
    {
        private readonly Action<T1, T2> _handler;

        public DoubleTypedHandlerContainer(Action<T1, T2> handler)
        {
            _handler = handler;
        }

        public void Invoke(object obj)
        {
            _handler?.Invoke((T1)obj, default(T2));
        }

        public void Invoke(object obj1, object obj2)
        {
            _handler?.Invoke((T1)obj1, (T2)obj2);
        }
    }
}
