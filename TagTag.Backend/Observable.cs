using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    internal class Observable<T>(T value, Action<T> InObserver) : IObservable<T>
    {
        // intentional hiding and single subscriber
        T IObservable<T>.Value
        {
            get => _value;
            set 
            {
                // bit of hard coded behaviour - client cant un-null the value.
                // could be done less intrinsic but this is simple
                if(_value is null) return;
                InObserver(_value = value); 
            }
        }
        Action<T> IObservable<T>.Observe { set => OutObserver = value; }

        private T _value = value;
        private Action<T> OutObserver { get; set; } = delegate { };

        public T Get() => _value;
        public void SetAndRaise(T value) => OutObserver(_value = value);
    }
}
