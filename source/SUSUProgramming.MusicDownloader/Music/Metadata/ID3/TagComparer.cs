// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections.Generic;
using CommunityToolkit.Diagnostics;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Represents an implementation of <see cref="IComparer{T}"/> for <see cref="ITag"/>.
    /// </summary>
    internal class TagComparer : IComparer<ITag>
    {
        /// <inheritdoc cref="Compare(ITag?, ITag?)"/>
        public static int CompareTags(ITag? x, ITag? y)
        {
            switch (x, y)
            {
                case (null, not null): return -1;
                case (null, null): return 0;
                case (not null, null): return 1;
            }

            if (x.Name == y.Name)
            {
                if (x is CoverTag cover1 && y is CoverTag cover2)
                {
                    return StringComparer.OrdinalIgnoreCase.Compare(cover1.CoverUri?.ToString(), cover2.CoverUri?.ToString());
                }

                return (x.Value, y.Value) switch
                {
                    (null, not null) => -1,
                    (not null, null) => 1,
                    (null, null) => 0,
                    (string[] sa1, string[] sa2) => CompareStringArrays(sa1, sa2),
                    (string s1, string s2) => StringComparer.OrdinalIgnoreCase.Compare(s1, s2),
                    (IComparable c1, object c2) => c1.CompareTo(c2),
                    _ => ThrowHelper.ThrowArgumentException<int>("Selected tags cannot be compared."),
                };
            }

            return x.Name.CompareTo(y.Name);
        }

        /// <inheritdoc/>
        public int Compare(ITag? x, ITag? y) => CompareTags(x, y);

        private static int CompareStringArrays(string[] sa1, string[] sa2)
        {
            if (sa1.Length > sa2.Length)
                return 1;
            if (sa1.Length < sa2.Length)
                return -1;

            for (int i = 0; i < sa1.Length; i++)
            {
                int c = StringComparer.OrdinalIgnoreCase.Compare(sa1[i], sa2[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        }
    }
}