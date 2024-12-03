// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Linq;

namespace SUSUProgramming.MusicDownloader.Collections
{
    /// <summary>
    /// Represents extension methods for custom collection types for an application.
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Projects an <see cref="IEnumerable{TSource}"/> to make observable projection that reacts on every source update.
        /// </summary>
        /// <remarks>
        /// Note that <paramref name="source"/> should implement <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
        /// to make observable projection follow source updates.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source items.</typeparam>
        /// <typeparam name="TProjection">Type of projected items.</typeparam>
        /// <param name="source">Source collection to select values from.</param>
        /// <param name="projection">Projection function that selects new items from old ones.</param>
        /// <param name="backMapping">Predicate that handles comparison between new created and old items (to handle items removal).</param>
        /// <returns>An instance of the <see cref="ObservableProjection{TSource, TProjection}"/> that follows updates of the source collection.</returns>
        public static ObservableProjection<TSource, TProjection> Project<TSource, TProjection>(this IEnumerable<TSource> source, Func<TSource, TProjection> projection, Func<TProjection, TSource, bool>? backMapping = null) => new(source, projection) { BackMapping = backMapping };

        /// <summary>
        /// Projects multiple collections of <see cref="IEnumerable{TSource}"/> to make observable projection that reacts on every source update.
        /// </summary>
        /// <remarks>
        /// Note that every item of <paramref name="source"/> should implement <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
        /// to make observable projection follow source updates.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source items.</typeparam>
        /// <typeparam name="TProjection">Type of projected items.</typeparam>
        /// <param name="source">An enumeration of source collections to aggregate and select values from.</param>
        /// <param name="projection">Projection function that selects new items from old ones.</param>
        /// <returns>An instance of the <see cref="ObservableProjection{TSource, TProjection}"/> that will contain joined items and follow updates of the source collections.</returns>
        public static ObservableMultiProjection<TSource, TProjection> MultiProject<TSource, TProjection>(this IEnumerable<IEnumerable<TSource>> source, Func<TSource, TProjection> projection) => new(source, projection);

        /// <summary>
        /// Wraps a collection to make projection with fixed items count.
        /// </summary>
        /// <typeparam name="TSource">Type of the source items.</typeparam>
        /// <typeparam name="TTarget">Type of projected items.</typeparam>
        /// <param name="source">Source collection to select values from.</param>
        /// <param name="projection">Projection function that selects new items from old ones.</param>
        /// <returns>An instance of the <see cref="Projection{TSource, TTarget}"/> that will map <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</returns>
        public static Projection<TSource, TTarget> AsCollection<TSource, TTarget>(this IReadOnlyCollection<TSource> source, Func<TSource, TTarget> projection) => new(source, projection);

        /// <summary>
        /// Return single item of the present collection or <see langword="default"/>{<typeparamref name="T"/>}
        /// if there are no items or more that one item.
        /// </summary>
        /// <typeparam name="T">Type of the source collection.</typeparam>
        /// <param name="values">Collection to select.</param>
        /// <returns>A single item of this collection or <see langword="default"/>{<typeparamref name="T"/>} if there are no items or more than one item.</returns>
        public static T? SingleNoExcept<T>(this IEnumerable<T> values)
        {
            if (values is IList<T> list)
            {
                // If the collection is an IList, we can check the count directly
                if (list.Count == 1)
                    return list[0];
            }
            else if (values.TryGetNonEnumeratedCount(out var count))
            {
                // If we can get the count without enumerating
                if (count == 1)
                    return values.First();
            }
            else
            {
                // If we can't get the count, we enumerate the first two items
                using (var enumerator = values.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        var first = enumerator.Current;
                        if (!enumerator.MoveNext())
                        {
                            return first; // Exactly one item
                        }
                    }
                }
            }

            return default; // Return default if there are 0 or more than 1 items
        }

        /// <summary>
        /// Finds the index of the first element in a collection that satisfies a specified condition.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="items">The collection of items to search through.</param>
        /// <param name="predicate">A function that defines the condition to be met. It takes an element of type <typeparamref name="T"/> and returns a boolean value.</param>
        /// <returns>
        /// The zero-based index of the first element that satisfies the condition defined by the <paramref name="predicate"/>; 
        /// or -1 if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="items"/> or <paramref name="predicate"/> is null.
        /// </exception>
        public static int IndexOfFirst<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (var item in items)
            {
                if (predicate(item))
                    return index;
                index++;
            }

            return -1;
        }
    }
}