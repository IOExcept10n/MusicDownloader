// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommunityToolkit.Diagnostics;
using TagLib;

namespace SUSUProgramming.MusicDownloader.Music.Metadata.ID3
{
    /// <summary>
    /// Represents an info about the ID3 tag bound to <see cref="TagLib"/> tags.
    /// </summary>
    /// <typeparam name="T">Type of the stored value.</typeparam>
    [DebuggerDisplay($"{{{nameof(GetDebugView)}(),nq}}")]
    internal record class Tag<T> : ITag
    {
        private readonly Func<Tag, T> getter;
        private readonly Action<Tag, T> setter;
        private MemberExpression propertyAccessor;

        private T @value = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag{T}"/> class by compiling binding between <see cref="Tag"/> and <see cref="TrackDetails"/> tag.
        /// </summary>
        /// <param name="propertyLambda">Lambda to deconstruct into compiled expression.</param>
        internal Tag(Expression<Func<Tag, T>> propertyLambda)
        {
            DeconstructLambda(propertyLambda);
            Name = propertyAccessor.Member.Name;

            ArgumentNullException.ThrowIfNull(propertyAccessor.Expression, nameof(propertyAccessor.Expression));

            // Compile accessors from lambda.
            ParameterExpression tagParam = (ParameterExpression)propertyAccessor.Expression;

            getter = Expression.Lambda<Func<Tag, T>>(propertyAccessor, tagParam).Compile();

            ParameterExpression valueParam = Expression.Parameter(typeof(T));
            Expression assign = Expression.Assign(propertyAccessor, valueParam);
            setter = Expression.Lambda<Action<Tag, T>>(assign, tagParam, valueParam).Compile();
        }

        /// <inheritdoc/>
        public event EventHandler? ValueUpdated;

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the value of this tag.
        /// </summary>
        public T Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, this.@value))
                {
                    this.@value = value;
                    ValueUpdated?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(Value))]
        public bool HasValue => Value?.Equals(default(T)) == false &&
                                (Value as IEnumerable)?.Cast<object>().Any() != false &&
                                !(Value is string s && string.IsNullOrWhiteSpace(s));

        /// <inheritdoc/>
        object? ITag.Value { get => Value; set => Value = (T)(Convert.ChangeType(value, typeof(T)) ?? ThrowHelper.ThrowArgumentNullException<T>(nameof(value))); }

        /// <summary>
        /// Creates new tag based on specified prototype and given value.
        /// </summary>
        /// <param name="tag">Prototype tag instance to clone.</param>
        /// <param name="value">Value to set to the new instance.</param>
        /// <returns>New cloned instance of the specified tag with given value set.</returns>
        public static Tag<T> operator +(Tag<T> tag, T? value) => tag with { Value = value! };

        /// <inheritdoc/>
        public void Apply(File file) => setter(file.Tag, Value);

        /// <inheritdoc/>
        public void ReadFrom(File file) => Value = getter(file.Tag);

        /// <inheritdoc/>
        ITag ITag.Clone() => (ITag)MemberwiseClone();

        [MemberNotNull(nameof(propertyAccessor))]
        private void DeconstructLambda(Expression<Func<Tag, T>> propertyLambda)
        {
            if (propertyLambda.Body is not MemberExpression member)
            {
                ThrowHelper.ThrowArgumentException(nameof(propertyLambda), $"Expression {propertyLambda} refers to a method, not a property.");
                return;
            }

            if (member.Member is not PropertyInfo property)
            {
                ThrowHelper.ThrowArgumentException(nameof(propertyLambda), $"Expression {propertyLambda} refers to a field, not a property.");
                return;
            }

            if (property.DeclaringType?.IsAssignableTo(typeof(Tag)) == false)
            {
                ThrowHelper.ThrowArgumentException(nameof(propertyLambda), $"Expression {propertyLambda} refers to a property not defined in TagLib Tag type.");
                return;
            }

            propertyAccessor = member;
        }

        private string GetDebugView() => Value switch
        {
            string[] arr => $"Name = {Name}, Value = [{string.Join(", ", arr)}]",
            _ => $"Name = {Name}, Value = {Value}",
        };
    }
}