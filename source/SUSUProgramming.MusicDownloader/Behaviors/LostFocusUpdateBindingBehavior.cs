// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace SUSUProgramming.MusicDownloader.Behaviors
{
    /// <summary>
    /// Represents a class that handles behavior on lost focus binding value update.
    /// </summary>
    public class LostFocusUpdateBindingBehavior : Behavior<AutoCompleteBox>
    {
        /// <summary>
        /// A dependency property for <see cref="AutoCompleteBox.Text"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<LostFocusUpdateBindingBehavior, string>(
            "Text", defaultBindingMode: BindingMode.TwoWay);

        static LostFocusUpdateBindingBehavior()
        {
            TextProperty.Changed.Subscribe(e =>
            {
                ((LostFocusUpdateBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }

        /// <summary>
        /// Gets or sets the text for binding.
        /// </summary>
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            ArgumentNullException.ThrowIfNull(AssociatedObject);
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            ArgumentNullException.ThrowIfNull(AssociatedObject);
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnBindingValueChanged()
        {
            if (AssociatedObject != null)
                AssociatedObject.Text = Text;
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
                Text = AssociatedObject.Text!;
        }
    }
}