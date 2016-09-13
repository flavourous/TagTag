using System;
using System.Collections;
using System.Collections.Generic;

namespace TagTag.Backend
{
    public interface IView
    {
        event Action<IEntity, IMenu> tagger;
        IEntityManager eman { set; }
        IMenu menu { get; }
        void SetDetailItems(IEnumerable<IEntity> items);
    }
    public interface IMenu
    {
        event Action MenuBack;
        void SetMenuItems(IEnumerable<IMenuItem> items);
        void SetTree(IEnumerable<String> tree);
    }
    public interface IMenuItem
    {
        bool ticked { get; set; }
        IEntity entity { get; }
        void Activate();
    }
    public interface IDetailPresentationStrategy
    {
        IEnumerable<IEntity> GetEntities(IEntity selected, IEnumerable<IEntity> all);
    }
    public interface IMenuPresentationStrategy
    {
        IEnumerable<IMenuItem> GetItems(ITag root, IEnumerable<IEntity> models, IEntityHooks hooks);
    }
    public interface IEntityHooks
    {
    }
    public interface IModel
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
    public interface IEntity
    {
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