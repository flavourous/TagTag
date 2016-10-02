using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagTag.Backend
{
    class MenuPresenter : IEntityHooks
    {
        readonly IMenuPresentationStrategy strat;
        readonly IModel model;
        readonly IMenu view;
        public event Action<IEntity> selected = delegate { };

        // For normalmenu
        public MenuPresenter(IMenu menu, IModel model, IMenuPresentationStrategy strat)
        {
            this.strat = strat;
            this.model = model;
            this.view = menu;
            menu.MenuBack += Menu_MenuBack;

            // defaults
            isticked = _ => false;
            ticked = delegate { };
        }

        // For tagmenu
        IEntity tagging;
        public event Action changed = delegate { };
        public MenuPresenter(ITagMenu menu, IModel model, IMenuPresentationStrategy strat) : this(menu as IMenu, model, strat)
        {
            // we're tagging that first argumentso....
            menu.tagging += t =>
            {
                tagging = t;
                Refresh();
            };
            isticked = tag => tagging != null && tagging.tags.Contains(tag as ITag);
            ticked = (tag, v) =>
            {
                if (tagging == null) return;
                var taggy = tag as ITag;
                if (v) model.AddTag(tagging, taggy);
                else model.RemoveTag(tagging, taggy);
                model.eman.UpdateEntity(taggy);
                changed();
            };
        }

        // selections end up here.
        bool esel = false;
        Stack<ITag> root = new Stack<ITag>();
        public ITag head { get { return root.PeekNull(); } }

        public void SelectEntity(IEntity en)
        {
            var models = model.GetEntities();
            models = models.Where(e => !e.Equals(tagging));

            if (en is ITag || en == null)
            {
                esel = false;
                var r = en as ITag;
                if (root.PeekNull() != r) root.Push(r);
                var menuItems = strat.GetItems(r, models, this);
                view.SetMenuItems(menuItems);
                view.SetTree((from rt in root select rt.name).Reverse());
                view.MenuID = r;
            }
            else esel = true;
            selected(en);
        }
        public void Refresh(ITag deleted = null)
        {
            // This pops down to a valid tagtree (there can be many or no paths paths to a severd tree - lets not try to arbitrairaly pick one)
            if (deleted != null) while (root.Contains(deleted)) root.PopNull();
            SelectEntity(head);
        }

        private void Menu_MenuBack()
        {
            if (esel) esel = false;
            else root.PopNull();
            SelectEntity(root.PeekNull());
        }

        // for strategy
        public Action<IEntity> activated { get { return SelectEntity; } }
        public Action<IEntity, bool> ticked { get; set; }
        public Func<IEntity, bool> isticked { get; set; }
    }
}
