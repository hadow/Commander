using System;
using System.Collections.Generic;
namespace EW.Xna.Platforms
{
    public partial class Game
    {
        private struct AddJournalEntry<T>
        {
            public readonly int Order;
            public readonly T Item;


            public AddJournalEntry(int order,T item)
            {
                Order = order;
                Item = item;
            }

            public static AddJournalEntry<T> CreateKey(T item)
            {
                return new AddJournalEntry<T>(-1, item);
            }

            public override int GetHashCode()
            {
                return Item.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is AddJournalEntry<T>))
                    return false;

                return object.Equals(Item, ((AddJournalEntry<T>)obj).Item);
            }
        }

        class SortingFilteringCollection<T> : ICollection<T>
        {
            private bool _shouldRebuildCache;

            private readonly List<T> _items;

            private readonly List<AddJournalEntry<T>> _addJournal;

            private readonly Comparison<AddJournalEntry<T>> _addJournalSortComparison;

            private readonly List<int> _removeJournal;

            private readonly List<T> _cachedFilteredItems;


            private readonly Predicate<T> _filter;

            private readonly Comparison<T> _sort;

            private readonly Action<T, EventHandler<EventArgs>> _filterChangedSubscriber;

            private readonly Action<T, EventHandler<EventArgs>> _filterChangedUnsubscriber;

            private readonly Action<T, EventHandler<EventArgs>> _sortChangedSubscriber;

            private readonly Action<T, EventHandler<EventArgs>> _sortChangedUnsubscriber;


            public SortingFilteringCollection(Predicate<T> filter,Action<T,EventHandler<EventArgs>> filterChangedSubscriber,
                                                                    Action<T,EventHandler<EventArgs>> filterChangedUnsubscriber,
                                                                    Comparison<T> sort,
                                                                    Action<T,EventHandler<EventArgs>> sortChangedSubscriber,
                                                                    Action<T,EventHandler<EventArgs>> sortChangedUnsubscriber)
            {
                _items = new List<T>();
                _addJournal = new List<AddJournalEntry<T>>();
                _removeJournal = new List<int>();
                _cachedFilteredItems = new List<T>();
                _shouldRebuildCache = true;

                _filter = filter;
                _filterChangedSubscriber = filterChangedSubscriber;
                _filterChangedUnsubscriber = filterChangedUnsubscriber;
                _sort = sort;
                _sortChangedSubscriber = sortChangedSubscriber;
                _sortChangedUnsubscriber = sortChangedUnsubscriber;

                _addJournalSortComparison = CompareAddJournalEntry;
            }

            private int CompareAddJournalEntry(AddJournalEntry<T> x,AddJournalEntry<T> y)
            {
                int result = _sort(x.Item, y.Item);
                if (result != 0)
                    return result;

                return x.Order - y.Order;
            }
            
            public bool IsReadOnly { get { return false; } }

            public void Add(T item)
            {
                _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
                InvalidateCache();
            }

            public bool Remove(T item)
            {
                if (_addJournal.Remove(AddJournalEntry<T>.CreateKey(item)))
                    return true;

                var index = _items.IndexOf(item);
                if (index >= 0)
                {
                    UnsubscribeFromItemEvents(item);
                    _removeJournal.Add(index);
                    InvalidateCache();
                    return true;
                }
                return false;
            }

            public void Clear()
            {
                for(int i = 0; i < _items.Count; i++)
                {
                    _filterChangedUnsubscriber(_items[i], Item_FilterPropertyChanged);
                    _sortChangedUnsubscriber(_items[i], Item_FilterPropertyChanged);
                }

                _addJournal.Clear();
                _removeJournal.Clear();
                _items.Clear();

                InvalidateCache();
            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(T[] array,int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public int Count { get { return _items.Count; } }

            public IEnumerator<T> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((System.Collections.IEnumerable)_items).GetEnumerator();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TUserData"></typeparam>
            /// <param name="action"></param>
            /// <param name="userData"></param>
            public void ForEachFilteredItem<TUserData>(Action<T,TUserData> action,TUserData userData)
            {
                if (_shouldRebuildCache)
                {

                    //Rebuild the cache
                    _cachedFilteredItems.Clear();
                    for(int i = 0; i < _items.Count; i++)
                    {
                        if (_filter(_items[i]))
                            _cachedFilteredItems.Add(_items[i]);
                    }

                    _shouldRebuildCache = false;
                }

                for(int i = 0; i < _cachedFilteredItems.Count; i++)
                {
                    action(_cachedFilteredItems[i], userData);
                }

                if (_shouldRebuildCache)
                    _cachedFilteredItems.Clear();
            }


            /// <summary>
            /// Sort high to low
            /// </summary>
            private static readonly Comparison<int> RemoveJournalSortComparison = (x, y) => Comparer<int>.Default.Compare(x, y);

            /// <summary>
            /// 
            /// </summary>
            private void ProcessRemoveJournal()
            {
                if (_removeJournal.Count == 0)
                    return;

                _removeJournal.Sort(RemoveJournalSortComparison);
                for(int i = 0; i < _removeJournal.Count; i++)
                {
                    _items.RemoveAt(_removeJournal[i]);
                }

                _removeJournal.Clear();
            }

            private void ProcessAddJournal()
            {
                if (_addJournal.Count == 0)
                    return;

                _addJournal.Sort(_addJournalSortComparison);

                int iAddJournal = 0;
                int iItems = 0;

                while (iItems < _items.Count && iAddJournal < _addJournal.Count)
                {
                    var addJournalItem = _addJournal[iAddJournal].Item;

                    //
                    if (_sort(addJournalItem, _items[iItems]) < 0)
                    {
                        SubscribeToItemEvents(addJournalItem);
                        _items.Insert(iItems, addJournalItem);
                        ++iAddJournal;
                    }

                    ++iItems;
                }

                for (; iAddJournal < _addJournal.Count; iAddJournal++)
                {
                    var addJournalItem = _addJournal[iAddJournal].Item;
                    SubscribeToItemEvents(addJournalItem);
                    _items.Add(addJournalItem);
                }

                _addJournal.Clear();
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            private void SubscribeToItemEvents(T item)
            {
                _filterChangedSubscriber(item, Item_FilterPropertyChanged);
                _sortChangedSubscriber(item, Item_SortPropertyChanged);

            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            private void UnsubscribeFromItemEvents(T item)
            {
                _filterChangedUnsubscriber(item, Item_FilterPropertyChanged);
                _sortChangedUnsubscriber(item, Item_SortPropertyChanged);
            }

            private void Item_FilterPropertyChanged(object sender,EventArgs e)
            {
                InvalidateCache();
            }

            private void Item_SortPropertyChanged(object sender,EventArgs e)
            {
                var item = (T)sender;
                var index = _items.IndexOf(item);

                _addJournal.Add(new AddJournalEntry<T>(_addJournal.Count, item));
                _removeJournal.Add(index);

                //Until the item is back in place,we don't care about its events,we will re-subscribe when _addJournal is processed.
                UnsubscribeFromItemEvents(item);
                InvalidateCache();

            }

            /// <summary>
            /// 重新构建缓存
            /// </summary>
            private void InvalidateCache()
            {
                _shouldRebuildCache = true;
            }

        }
    }
}