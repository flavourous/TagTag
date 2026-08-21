using System;
using System.Collections;
using System.Collections.Generic;

namespace TagTag.Backend
{
    public interface IView
    {
        IEntityManager eman { set; }
        ITagMenu tagger { get; }
        void SetDetailItems(IEnumerable<IEntity> items);
    }
    public interface ITagMenu
    {
        IEntity tagging { get; }
        void SetItems(IEnumerable<IMenuItem<ITag>> items);
    }
    public interface IMenuItem<T> where T : IEntity
    {
        bool selected { get; set; }
        T entity { get; }
    }

    internal interface IEntityHooks
    {
        IEnumerable<ITag> filter { get; }
    }
    internal interface IModel
    {
        IEnumerable<IEntity> GetEntities();
        IEntityManager eman { get; }
        void AddTag(IEntity entity, ITag tag);
        void RemoveTag(IEntity entity, ITag tag);
    }

    public interface IEntityManager
    {
        T CreateEntity<T>() where T : IEntity;
        void DeleteEntity(IEntity d);
        IEntity UpdateEntity(IEntity e);
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