using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    internal class EntityItem<T>(T entity) : IEntityItem<T> where T : IEntity
    {
        public T entity { get; } = entity;

        IObservable<bool?> IEntityItem<T>.selected { get => selected; }
        IObservable<bool?> IEntityItem<T>.tagging { get => tagging; }
        IObservable<bool?> IEntityItem<T>.tagged { get => tagged; }

        public Observable<bool?> selected { get; set; }
        public Observable<bool?> tagging { get; set; }
        public Observable<bool?> tagged { get; set; }
    }
}
