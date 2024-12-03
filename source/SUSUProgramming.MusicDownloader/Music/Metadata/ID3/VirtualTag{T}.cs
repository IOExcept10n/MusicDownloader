// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Represents generic version of the <see cref="VirtualTag"/> class.
    /// </summary>
    /// <typeparam name="T">Type of the stored value.</typeparam>
    /// <param name="Name">Name of the tag.</param>
    public record class VirtualTag<T>(string Name) : VirtualTag(Name)
    {
        private T value = default!;

        /// <summary>
        /// Gets or sets value of the tag.
        /// </summary>
        public new T Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.@value, value))
                {
                    this.@value = value;
                    OnValueUpdated();
                }
            }
        }

        /// <inheritdoc/>
        public override bool HasValue => Value?.Equals(default(T)) == false &&
                                         (Value as IEnumerable)?.Cast<object>().Any() != false &&
                                         !(Value is string s && string.IsNullOrWhiteSpace(s));

        /// <inheritdoc/>
        protected override object? ValueAccessor { get => Value; set => Value = (T)value!; }

        /// <summary>
        /// Clones the tag instance and sets new value to it.
        /// </summary>
        /// <param name="tag">Tag instance to clone.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>A cloned instance of the specified tag with new value set.</returns>
        public static VirtualTag<T> operator +(VirtualTag<T> tag, T value)
        {
            return tag with { Value = value };
        }
    }
}