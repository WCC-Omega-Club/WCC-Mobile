using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace WCCMobile.Adapters
{
    public class GenericAdapter<T, X> : RecyclerView.Adapter where X : BasePopulateViewHolder<T>
    {
        public event EventHandler<int> ItemClick;
        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IList<T> items { get; private set; }
        /// <summary>
        /// Gets the resource identifier.
        /// </summary>
        /// <value>
        /// The resource identifier.
        /// </value>
        public int resourceId { get; private set; }
        /// <summary>
        /// Gets the creator.
        /// </summary>
        /// <value>
        /// The creator.
        /// </value>
        public Func<View, X> creator { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAdapter{T, X}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <param name="creator">The creator.</param>
        public GenericAdapter(IList<T> items, int resourceId, Func<View, X> creator)
        {
            this.items = items;
            this.resourceId = resourceId;
            this.creator = creator;
        }
        /// <summary>
        /// Sets the list items.
        /// </summary>
        /// <param name="items">The items.</param>
        public void SetListItems(IList<T> items)
        {
            this.items = items;
        }

        #region implemented abstract members of Adapter        
        /// <summary>
        /// Called when [bind view holder].
        /// </summary>
        /// <param name="holder">The holder.</param>
        /// <param name="position">The position.</param>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            X vh = (X)holder;
            vh.PopulateFrom(items[position]);
        }
        /// <summary>
        /// Called when [create view holder].
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="viewType">Type of the view.</param>
        /// <returns></returns>
        public override RecyclerView.ViewHolder OnCreateViewHolder(Android.Views.ViewGroup parent, int viewType)
        {
            //Have to use delegate because we cannot invoke generic constructor
            var vh = creator(LayoutInflater.From(parent.Context).Inflate(resourceId, parent, false));
            vh.SetClickListener(clickPosition => OnClick(clickPosition));
            return vh;
        }
        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>
        /// The item count.
        /// </value>
        public override int ItemCount
        {
            get
            {
                return items == null ? 0 : items.Count;
            }
        }
        /// <summary>
        /// Called when [click].
        /// </summary>
        /// <param name="position">The position.</param>
        void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick.Invoke(this, position);
            }
        }

        #endregion
    }

    public abstract class BasePopulateViewHolder<T> : RecyclerView.ViewHolder
    {
        /// <summary>
        /// Populates overriding children of <see cref="BasePopulateViewHolder{T}"/> with <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        public abstract void PopulateFrom(T data);
        private Action<int> listener;
        /// <summary>
        /// Initializes a new instance of the <see cref="BasePopulateViewHolder{T}"/> class.
        /// </summary>
        /// <param name="itemView">The item view.</param>
        public BasePopulateViewHolder(View itemView) : base(itemView) { }
        /// <summary>
        /// Sets the click listener.
        /// </summary>
        /// <param name="listener">The listener.</param>
        public void SetClickListener(Action<int> listener)
        {
            this.listener = listener;
            ItemView.Click += HandleClick;
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (ItemView != null)
            {
                ItemView.Click -= HandleClick;
            }
            listener = null;
        }
        /// <summary>
        /// Handles the click event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleClick(object sender, EventArgs e)
        {
            if (listener != null)
            {
                listener(base.AdapterPosition);
            }
        }
    }
}