// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections.Generic;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Chooses which of several cover candidates to recommend.
    /// </summary>
    internal static class CoverRanking
    {
        /// <summary>
        /// Finds the candidate with strictly the most pixels.
        /// </summary>
        /// <param name="pixels">Pixel count of each candidate, <see langword="null"/> when the size is unknown.</param>
        /// <returns>Index of the largest candidate, or <see langword="null"/> if no size is known or several candidates tie for the largest.</returns>
        public static int? IndexOfLargest(IReadOnlyList<long?> pixels)
        {
            int? best = null;
            long bestPixels = 0;
            bool tie = false;
            for (int i = 0; i < pixels.Count; i++)
            {
                if (pixels[i] is not { } value)
                    continue;

                if (best == null || value > bestPixels)
                {
                    best = i;
                    bestPixels = value;
                    tie = false;
                }
                else if (value == bestPixels)
                {
                    tie = true;
                }
            }

            return tie ? null : best;
        }
    }
}
