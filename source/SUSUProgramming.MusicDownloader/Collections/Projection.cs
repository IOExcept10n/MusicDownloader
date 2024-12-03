// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections;
using System.Collections.Generic;

namespace SUSUProgramming.MusicDownloader.Collections
{
    /// <summary>
    /// Represents a projection of an original collection into a read-only collection of projected items.
    /// This class implements <see cref="IReadOnlyCollection{T}"/> to provide a way to enumerate over the
    /// projected items without modifying the original collection.
    /// </summary>
    /// <typeparam name="TSource">The type of the original collection items.</typeparam>
    /// <typeparam name="TTarget">The type of the projected collection items.</typeparam>
    internal class Projection<TSource, TTarget> : IReadOnlyCollection<TTarget>
    {
        private readonly IReadOnlyCollection<TSource> originalCollection;
        private readonly Func<TSource, TTarget> projection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Projection{TSource, TTarget}"/> class.
        /// </summary>
        /// <param name="originalCollection">The original collection to project from.</param>
        /// <param name="projection">A function that defines how to project items from the original collection.</param>
        public Projection(IReadOnlyCollection<TSource> originalCollection, Func<TSource, TTarget> projection)
        {
            this.originalCollection = originalCollection ?? throw new ArgumentNullException(nameof(originalCollection));
            this.projection = projection ?? throw new ArgumentNullException(nameof(projection));
        }

        /// <summary>
        /// Gets the number of projected items in the collection.
        /// </summary>
        public int Count => originalCollection.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the projected collection.
        /// </summary>
        /// <returns>An enumerator for the projected collection.</returns>
        public IEnumerator<TTarget> GetEnumerator()
        {
            foreach (var item in originalCollection)
                yield return projection(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the projected collection.
        /// </summary>
        /// <returns>An enumerator for the projected collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}