// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;

namespace SUSUProgramming.MusicDownloader.Music.Metadata
{
    /// <summary>
    /// Reads the pixel size of PNG and JPEG images from their headers without decoding them.
    /// </summary>
    internal static class ImageSize
    {
        /// <summary>
        /// Tries to read the size of the image.
        /// </summary>
        /// <param name="data">Image file bytes.</param>
        /// <returns>Width and height in pixels or <see langword="null"/> if the format is unknown or the data is truncated.</returns>
        public static (int Width, int Height)? TryRead(ReadOnlySpan<byte> data)
        {
            if (data.Length >= 24 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                return (ReadInt32(data, 16), ReadInt32(data, 20));

            if (data.Length >= 4 && data[0] == 0xFF && data[1] == 0xD8)
                return TryReadJpeg(data);

            return null;
        }

        private static (int Width, int Height)? TryReadJpeg(ReadOnlySpan<byte> data)
        {
            int i = 2;
            while (i + 3 < data.Length)
            {
                if (data[i] != 0xFF)
                {
                    i++;
                    continue;
                }

                byte marker = data[i + 1];
                if (marker == 0xFF)
                {
                    i++;
                    continue;
                }

                if (marker == 0xD8 || marker == 0x01 || (marker >= 0xD0 && marker <= 0xD7))
                {
                    i += 2;
                    continue;
                }

                if (marker == 0xD9)
                    return null;

                bool startOfFrame = marker is >= 0xC0 and <= 0xCF && marker is not (0xC4 or 0xC8 or 0xCC);
                if (startOfFrame)
                {
                    if (i + 8 >= data.Length)
                        return null;
                    int height = (data[i + 5] << 8) | data[i + 6];
                    int width = (data[i + 7] << 8) | data[i + 8];
                    return (width, height);
                }

                int length = (data[i + 2] << 8) | data[i + 3];
                i += 2 + length;
            }

            return null;
        }

        private static int ReadInt32(ReadOnlySpan<byte> data, int offset) =>
            (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
    }
}
