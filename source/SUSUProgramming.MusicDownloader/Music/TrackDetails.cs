// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under CC BY-NC 4.0 license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Collections;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using TagLib;

namespace SUSUProgramming.MusicDownloader.Music
{
    /// <summary>
    /// Represents a collection of the track tags.
    /// </summary>
    public class TrackDetails : KeyedCollection<string, ITag>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// Defines a value for the missing artist.
        /// </summary>
        public const string UnknownArtist = "Unknown Artist";

        /// <summary>
        /// Defines a value for the missing title.
        /// </summary>
        public const string UnknownTitle = "Untitled";

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackDetails"/> class.
        /// </summary>
        public TrackDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackDetails"/> class.
        /// </summary>
        /// <param name="source">Source collection of tags to create tags from.</param>
        public TrackDetails(IEnumerable<ITag> source)
        {
            foreach (var tag in source)
            {
                int index = IndexOf(tag);
                if (index == -1)
                    Add(tag);
                else
                    this[index] = tag;
            }
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the URI of the cover image associated with the media item.
        /// If the cover tag is not present or cannot be retrieved, returns <c>null</c>.
        /// </summary>
        public Uri? CoverUri =>
            TryGetValue(nameof(CoverTag.Cover), out var t) && t is CoverTag cover ? cover.CoverUri : null;

        /// <summary>
        /// Gets the file path of the media item.
        /// If the FilePath tag is not present or cannot be retrieved, returns <c>null</c>.
        /// </summary>
        public string? FilePath =>
            TryGetTag<string?>(nameof(FilePath), out var result) ? result : null;

        /// <summary>
        /// Gets a formatted string of the artists associated with the media item.
        /// If no artists are found, returns a predefined string representing an unknown artist.
        /// The string concatenates artist names, handling special cases for featured artists.
        /// </summary>
        public string FormedArtistString
        {
            get
            {
                if (!TryGetTag<string[]>(nameof(Tag.Performers), out var performers) || performers == null || performers.Length == 0)
                    return UnknownArtist;

                StringBuilder result = new();
                bool addSeparator = false;

                foreach (var performer in performers)
                {
                    // Check if the performer is a featured artist
                    if (performer.StartsWith("feat. ", StringComparison.InvariantCultureIgnoreCase) ||
                        performer.StartsWith("ft. ", StringComparison.InvariantCultureIgnoreCase))
                        addSeparator = false;

                    // Add a separator if needed
                    if (addSeparator)
                        result.Append(',').Append(' ');

                    result.Append(performer);
                    addSeparator = true;
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Gets a formatted title string for the media item.
        /// If the title is not present or is empty, returns a predefined string representing an unknown title.
        /// If a subtitle is present, it appends it in parentheses to the title.
        /// </summary>
        public string FormedTitle
        {
            get
            {
                if (!TryGetTag<string>(nameof(Tag.Title), out var title) || string.IsNullOrWhiteSpace(title))
                {
                    return UnknownTitle;
                }

                if (TryGetTag<string>(nameof(Tag.Subtitle), out var subtitle) && !string.IsNullOrWhiteSpace(subtitle))
                    return $"{title} ({subtitle})";

                return title;
            }
        }

        /// <summary>
        /// Gets a combined track name string that includes the formatted artist string and the formatted title.
        /// The format is "Artist - Title".
        /// </summary>
        public string FormedTrackName =>
            FormedArtistString + " - " + FormedTitle;

        /// <summary>
        /// Gets a value indicating whether the media item has associated artists.
        /// Returns <see langword="true"/> if the Performers tag is present; otherwise, <see langword="false"/>.
        /// </summary>
        public bool HasArtists =>
            ContainsKey(nameof(Tag.Performers));

        /// <summary>
        /// Gets a value indicating whether the media item has a cover image.
        /// Returns <see langword="true"/> if either the HasCover virtual tag or the Cover tag is present; otherwise, <see langword="false"/>.
        /// </summary>
        public bool HasCover =>
            ContainsKey(nameof(VirtualTags.HasCover)) || ContainsKey(nameof(CoverTag.Cover));

        /// <summary>
        /// Gets a value indicating whether the media item has a title.
        /// Returns <see langword="true"/> if the Title tag is present; otherwise, <see langword="false"/>.
        /// </summary>
        public bool HasTitle =>
            ContainsKey(nameof(Tag.Title));

        /// <summary>
        /// Gets the URI associated with the media track.
        /// If the URI tag is not present or cannot be retrieved, returns <c>null</c>.
        /// </summary>
        public Uri? TrackUri =>
            TryGetTag<Uri?>(nameof(Uri), out var result) ? result : null;

        /// <summary>
        /// Joins two sets of tags with replacing old ones with new.
        /// </summary>
        /// <param name="left">Set of tags to replace.</param>
        /// <param name="right">Set of tags to add and replace with.</param>
        /// <returns>New set of tags with replaced data.</returns>
        public static TrackDetails ReplaceWith(TrackDetails left, TrackDetails right) => [.. left, .. right];

        /// <summary>
        /// Joins two sets of tags with uniting old ones with new.
        /// </summary>
        /// <param name="left">Set of tags to union.</param>
        /// <param name="right">Set of tags to add and union with.</param>
        /// <returns>New set of tags with united data.</returns>
        public static TrackDetails UnionWith(TrackDetails left, TrackDetails right) => [.. right, .. left];

        /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey(TKey)"/>
        public bool ContainsKey(string key) => TryGetValue(key, out _);

        /// <summary>
        /// Gets the value of the specified tag.
        /// </summary>
        /// <typeparam name="T">Type of the value to use.</typeparam>
        /// <param name="name">Name of the tag to access.</param>
        /// <returns>Value of the tag.</returns>
        public T GetTag<T>(string name) => (T)this[name].Value!;

        /// <summary>
        /// Sets the value to the specified tag.
        /// </summary>
        /// <typeparam name="T">Type of the value to use.</typeparam>
        /// <param name="name">Name of the tag to access.</param>
        /// <param name="value">Value to store.</param>
        public void SetTag<T>(string name, T value)
        {
            if (TryGetValue(name, out var t))
            {
                t.Value = value;
                if (!t.HasValue)
                    Remove(t);
            }
            else if (Tags.AllTags.FirstOrDefault(x => x.Name == name) is Tag<T> tag)
            {
                t = tag + value;
                if (t.HasValue)
                    Add(t);
            }
            else if (VirtualTags.AllTags.FirstOrDefault(x => x.Name == name) is VirtualTag<T> virtualTag)
            {
                t = virtualTag + value;
                if (t.HasValue)
                    Add(t);
            }
        }

        /// <summary>
        /// Tries to get the value of the specified tag.
        /// </summary>
        /// <typeparam name="T">Type of the value to use.</typeparam>
        /// <param name="name">Name of the tag to access.</param>
        /// <param name="tag">Value stored in this set.</param>
        /// <returns><see langword="true"/> if tag found and value is returned; otherwise <see langword="false"/>.</returns>
        public bool TryGetTag<T>(string name, out T? tag)
        {
            if (TryGetValue(name, out var t))
            {
                tag = (T?)t.Value;
                return true;
            }

            tag = default;
            return false;
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                Notify(item.Name);
                item.ValueUpdated -= Item_ValueUpdated;
            }

            base.ClearItems();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(ITag item) => item.Name;

        /// <inheritdoc/>
        protected override void InsertItem(int index, ITag item)
        {
            // Discard empty tags.
            if (item == null || !item.HasValue)
            {
                return;
            }

            int oldIndex = this.IndexOfFirst(x => x.Name == item.Name);

            // To replace old value with new one.
            if (oldIndex >= 0)
            {
                Remove(item.Name);
                if (oldIndex < index)
                    index--;
            }

            item.ValueUpdated += Item_ValueUpdated;
            base.InsertItem(index, item);
            Notify(item.Name);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var item = Items[index];
            item.ValueUpdated -= Item_ValueUpdated;
            base.RemoveItem(index);
            Notify(item.Name);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, ITag item)
        {
            // Discard empty tags.
            if (item == null || !item.HasValue)
            {
                return;
            }

            item.ValueUpdated += Item_ValueUpdated;
            Notify(item.Name);
            base.SetItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, index));
        }

        private void Item_ValueUpdated(object? sender, EventArgs e)
        {
            var tag = sender as ITag;
            Notify(tag?.Name ?? string.Empty);
        }

        private void Notify(string name)
        {
            PropertyChanged?.Invoke(this, new(name));
            PropertyChanged?.Invoke(this, new(nameof(HasTitle)));
            PropertyChanged?.Invoke(this, new(nameof(HasArtists)));
            PropertyChanged?.Invoke(this, new(nameof(FormedTitle)));
            PropertyChanged?.Invoke(this, new(nameof(FormedArtistString)));
            PropertyChanged?.Invoke(this, new(nameof(FormedTrackName)));
            PropertyChanged?.Invoke(this, new(nameof(FilePath)));
            PropertyChanged?.Invoke(this, new(nameof(TrackUri)));
        }
    }
}