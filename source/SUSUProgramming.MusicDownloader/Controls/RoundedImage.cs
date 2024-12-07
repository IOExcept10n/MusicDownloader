// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SUSUProgramming.MusicDownloader.Controls
{
    /// <summary>
    /// Represents an image with rounded corners in an Avalonia application.
    /// </summary>
    public class RoundedImage : Image
    {
        /// <summary>
        /// Identifies the <see cref="CornerRadius"/> dependency property.
        /// The default value is 5.
        /// </summary>
        public static readonly StyledProperty<double> CornerRadiusProperty =
            AvaloniaProperty.Register<RoundedImage, double>("CornerRadius", 5);

        /// <summary>
        /// Gets or sets the radius of the corners of the image.
        /// This property is a styled property, which means it can be set in XAML.
        /// </summary>
        public double CornerRadius
        {
            get => GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Measures the size of the image and applies the corner radius clipping.
        /// This method overrides the <see cref="Image.MeasureOverride(Size)"/> method.
        /// </summary>
        /// <param name="availableSize">The available size that the image can occupy.</param>
        /// <returns>The size that the image should be rendered at.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            IImage? source = Source; // Get the source image
            Size result = default; // Initialize the result size

            // If the source image is not null, calculate the size based on the available size and source size
            if (source != null)
            {
                result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);
            }

            // Set the clipping geometry to create rounded corners
            Clip = new RectangleGeometry(new Rect(0, 0, result.Width, result.Height), CornerRadius, CornerRadius);
            return result; // Return the calculated size
        }
    }
}