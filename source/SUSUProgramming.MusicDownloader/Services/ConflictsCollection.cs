// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.ObjectModel;
using SUSUProgramming.MusicDownloader.Music;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a collection of tagging conflicts.
    /// </summary>
    internal class ConflictsCollection : KeyedCollection<string, TaggingConflictInfo>
    {
        /// <summary>
        /// Tries to automatically resolve the conflict.
        /// </summary>
        /// <returns>Details about the track without conflicts.</returns>
        public TrackDetails AutoResolve()
        {
            TrackDetails result = [];
            for (int i = 0; i < Items.Count; i++)
            {
                TaggingConflictInfo? conflict = Items[i];
                if (conflict.FoundData.Count == 1)
                {
                    result.Add(conflict.FoundData[0]);
                    RemoveAt(i--);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(TaggingConflictInfo item) => item.TagName;
    }
}