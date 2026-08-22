using System.Collections;
using System.Collections.Generic;

namespace TagTag.Backend
{
    public interface IView
    {
        IEntityRepository entities { set; }
        ITagMenu cloud { get; }
        void SetDetailItems(IEnumerable<IEntityItem<IEntity>> items);
    }

    public interface ITagMenu
    {
        void SetItems(IEnumerable<IEntityItem<ITag>> items);
    }

    public interface IEntityItem<T> where T : IEntity
    {
        IObservable<bool?> selected { get; }
        IObservable<bool?> tagging { get; }
        IObservable<bool?> tagged { get; }
        T entity { get; }
    }

    // not System.IObservable to try to keep architecture "pure simple c#" just for fun
    public interface IObservable<T>
    {
        public T Value { get; set; }
        public Action<T> Observe { set; }
    }

    public interface IEntityRepository
    {
        IEnumerable<IEntity> GetEntities();
        T CreateEntity<T>() where T : IEntity;
        void DeleteEntity(IEntity d);
        IEntity UpdateEntity(IEntity e);
        void AddTag(IEntity entity, ITag tag);
        void RemoveTag(IEntity entity, ITag tag);
    }

    // I was tempted by an ES approach, but decided a normal ORM modelling 
    // approach in the end.  One entity being any number of things is useful - i.e.
    // a note, a few photos and a couple recordings.  You can get the polymorphism
    // with this approach, but have to do IMultiEntity : IEntity for the multivalued
    // entries on the same type of thing.  Though you gain simplified presentation, eg here we
    // couldnt have INote : IEntity.  It's be INote[] INoteSystem.GetNotes(IEntity id).
    //
    // The model implimentation will probabbly still look like an ES however.
    public interface IEntity
    {
        DateTime created { get; }
        String name { get; set; }
        IEnumerable<ITag> tags { get; }
    }
    public interface ITag : IEntity
    {

    }
    public interface INote : IEntity
    {
        String text { get; set; }
    }
}