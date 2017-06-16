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

            }

            public bool Remove(T item)
            {
                return false;
            }

            public void Clear()
            {

            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(T[] array,int arrayIndex)
            {

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
        }
    }
}