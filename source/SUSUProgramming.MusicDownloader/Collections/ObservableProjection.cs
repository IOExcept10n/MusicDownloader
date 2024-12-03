// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Diagnostics;

namespace SUSUProgramming.MusicDownloader.Collections
{
    /// <summary>
    /// Represents a projection of a source collection into a read-only collection of projected items.
    /// This class implements <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>
    /// to provide notifications when the underlying source collection changes or when properties of the
    /// projected items change.
    /// </summary>
    /// <typeparam name="TSource">The type of the source collection items.</typeparam>
    /// <typeparam name="TProjection">The type of the projected collection items.</typeparam>
    public sealed class ObservableProjection<TSource, TProjection> : ReadOnlyCollection<TProjection>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDisposable
    {
        private readonly ObservableCollection<TProjection> projectedCollection;
        private readonly Func<TSource, TProjection> projection;
        private readonly IEnumerable<TSource> sourceCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableProjection{TSource, TProjection}"/> class.
        /// </summary>
        /// <param name="source">The source collection to project from.</param>
        /// <param name="projection">A function that defines how to project items from the source collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="projection"/> is null.</exception>
        public ObservableProjection(IEnumerable<TSource> source, Func<TSource, TProjection> projection)
            : base(new ObservableCollection<TProjection>(source.Select(projection)))
        {
            sourceCollection = source ?? throw new ArgumentNullException(nameof(source));
            this.projection = projection ?? throw new ArgumentNullException(nameof(projection));
            projectedCollection = (ObservableCollection<TProjection>)Items;

            // Subscribe to source collection changes
            if (sourceCollection is not INotifyCollectionChanged notify)
            {
                ThrowHelper.ThrowArgumentException(nameof(source), $"Collection should implement {nameof(INotifyCollectionChanged)}.");
                return;
            }

            notify.CollectionChanged += SourceCollectionChanged;
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
        /// Occurs when a property of an item in the collection changes.
        /// </summary>
        public event EventHandler<ContentPropertyChangedEventArgs>? ContentPropertyChanged;

        /// <summary>
        /// Occurs when a property of the projected collection changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => ((INotifyPropertyChanged)projectedCollection).PropertyChanged += value;
            remove => ((INotifyPropertyChanged)projectedCollection).PropertyChanged -= value;
        }

        /// <summary>
        /// Gets or sets a function for back-mapping projected items to their source items.
        /// </summary>
        public Func<TProjection, TSource, bool>? BackMapping { get; set; }

        /// <summary>
        /// Disposes the resources used by the <see cref="ObservableProjection{TSource, TProjection}"/> instance.
        /// </summary>
        public void Dispose()
        {
            ((INotifyCollectionChanged)sourceCollection).CollectionChanged -= SourceCollectionChanged;
        }

        private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
        {
            ContentPropertyChanged?.Invoke(this, new(sender, e));
        }

        private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (TSource item in e.NewItems)
                        {
                            var projected = projection(item);
                            if (projected is INotifyPropertyChanged notify)
                                notify.PropertyChanged += OnItemChanged;
                            projectedCollection.Add(projected);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (TSource item in e.OldItems)
                        {
                            TProjection projectedItem;
                            if (BackMapping != null)
                            {
                                projectedItem = projectedCollection.First(x => BackMapping(x, item));
                            }
                            else
                            {
                                // Assumes 1-to-1 mapping between source and projection
                                projectedItem = projection(item);
                            }

                            if (projectedItem is INotifyPropertyChanged notify)
                                notify.PropertyChanged -= OnItemChanged;
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
                            var oldProjection = projection(oldItem);
                            if (oldProjection is INotifyPropertyChanged notify)
                            {
                                notify.PropertyChanged -= OnItemChanged;
                            }

                            int index = projectedCollection.IndexOf(oldProjection);
                            if (index >= 0)
                            {
                                var newProjection = projection(newItem);
                                if (newProjection is INotifyPropertyChanged notify1)
                                {
                                    notify1.PropertyChanged += OnItemChanged;
                                }

                                projectedCollection[index] = newProjection;
                            }
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Move:
                    if (e.OldItems != null && e.NewStartingIndex >= 0)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            TSource item = (TSource)e.OldItems[i]!;
                            int oldIndex = projectedCollection.IndexOf(projection(item));
                            if (oldIndex >= 0)
                            {
                                projectedCollection.Move(oldIndex, e.NewStartingIndex + i);
                            }
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in projectedCollection.OfType<INotifyPropertyChanged>())
                        item.PropertyChanged -= OnItemChanged;
                    projectedCollection.Clear();
                    foreach (var item in sourceCollection)
                    {
                        var projectedItem = projection(item);
                        if (projectedItem is INotifyPropertyChanged notify)
                            notify.PropertyChanged += OnItemChanged;
                        projectedCollection.Add(projectedItem);
                    }

                    break;
            }
        }

        /// <summary>
        /// Represents the event arguments for the <see cref="ContentPropertyChanged"/> event.
        /// </summary>
        /// <param name="Item">The item that changed.</param>
        /// <param name="Args">The property change event arguments.</param>
        public readonly record struct ContentPropertyChangedEventArgs(object? Item, PropertyChangedEventArgs Args);
    }
}