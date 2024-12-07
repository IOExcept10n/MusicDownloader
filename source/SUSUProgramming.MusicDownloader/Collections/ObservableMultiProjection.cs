// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SUSUProgramming.MusicDownloader.Collections
{
    /// <summary>
    /// Represents a multi-projection of multiple source collections into a read-only collection of projected items.
    /// This class implements <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>
    /// to provide notifications when any of the underlying source collections change or when properties of the
    /// projected items change.
    /// </summary>
    /// <typeparam name="TSource">The type of the source collection items.</typeparam>
    /// <typeparam name="TProjection">The type of the projected collection items.</typeparam>
    public class ObservableMultiProjection<TSource, TProjection> : ReadOnlyCollection<TProjection>,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        private readonly ObservableCollection<TProjection> projectedCollection;
        private readonly Func<TSource, TProjection> projection;
        private readonly List<IEnumerable<TSource>> sourceCollections;
        private readonly INotifyCollectionChanged? sourcesCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableMultiProjection{TSource, TProjection}"/> class.
        /// </summary>
        /// <param name="sources">The collection of source collections to project from.</param>
        /// <param name="projection">A function that defines how to project items from the source collections.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sources"/> or <paramref name="projection"/> is null.</exception>
        public ObservableMultiProjection(IEnumerable<IEnumerable<TSource>> sources, Func<TSource, TProjection> projection)
            : base(new ObservableCollection<TProjection>())
        {
            sourceCollections = sources?.ToList() ?? throw new ArgumentNullException(nameof(sources));
            this.projection = projection ?? throw new ArgumentNullException(nameof(projection));
            projectedCollection = (ObservableCollection<TProjection>)Items;

            // Check if sources implement INotifyCollectionChanged
            if (sources is INotifyCollectionChanged notifyCollection)
            {
                sourcesCollection = notifyCollection;
                sourcesCollection.CollectionChanged += SourcesCollectionChanged;
            }

            // Subscribe to each source collection's changes and initialize the projected collection
            foreach (var source in sourceCollections)
            {
                InitializeSourceCollection(source);
            }
        }

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add => projectedCollection.CollectionChanged += value;
            remove => projectedCollection.CollectionChanged -= value;
        }

        /// <summary>
        /// Occurs when a property of the projected collection changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => ((INotifyPropertyChanged)projectedCollection).PropertyChanged += value;
            remove => ((INotifyPropertyChanged)projectedCollection).PropertyChanged -= value;
        }

        /// <summary>
        /// Removes a source collection from the multi-projection.
        /// </summary>
        /// <param name="source">The source collection to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        public void RemoveSourceCollection(IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);

            // Unsubscribe from the source's CollectionChanged event
            ((INotifyCollectionChanged)source).CollectionChanged -= SourceCollectionChanged;

            // Remove the source from the list of tracked sources
            sourceCollections.Remove(source);

            // Remove all projected items from the projected collection
            foreach (var item in source)
            {
                var projectedItem = projection(item);
                projectedCollection.Remove(projectedItem);
            }
        }

        private void InitializeSourceCollection(IEnumerable<TSource> source)
        {
            ((INotifyCollectionChanged)source).CollectionChanged += SourceCollectionChanged;

            // Initialize the projected collection with the current items
            foreach (var item in source)
            {
                projectedCollection.Add(projection(item));
            }
        }

        private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<TSource> source)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (e.NewItems != null)
                        {
                            foreach (TSource item in e.NewItems)
                            {
                                projectedCollection.Add(projection(item));
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems != null)
                        {
                            foreach (TSource item in e.OldItems)
                            {
                                // Find the projected item and remove it
                                var projectedItem = projection(item);
                                projectedCollection.Remove(projectedItem);
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Replace:
                        if (e.NewItems != null && e.OldItems != null)
                        {
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                TSource oldItem = (TSource)e.OldItems[i]!;
                                TSource newItem = (TSource)e.NewItems[i]!;

                                // Remove the old projected item
                                var projectedOldItem = projection(oldItem);
                                projectedCollection.Remove(projectedOldItem);

                                // Add the new projected item
                                projectedCollection.Add(projection(newItem));
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Move:
                        // Moving items doesn't affect the projection directly
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        projectedCollection.Clear();
                        foreach (var sourceCollection in sourceCollections)
                        {
                            foreach (var item in sourceCollection)
                            {
                                projectedCollection.Add(projection(item));
                            }
                        }

                        break;
                }
            }
        }

        private void SourcesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (IEnumerable<TSource> newSource in e.NewItems)
                {
                    InitializeSourceCollection(newSource);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (IEnumerable<TSource> oldSource in e.OldItems)
                {
                    RemoveSourceCollection(oldSource);
                }
            }
        }
    }
}