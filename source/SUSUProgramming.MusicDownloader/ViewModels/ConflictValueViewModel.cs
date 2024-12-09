// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using SUSUProgramming.MusicDownloader.Services;

namespace SUSUProgramming.MusicDownloader.ViewModels
{
    /// <summary>
    /// Represents a view model that can visualize specified tag.
    /// </summary>
    public partial class ConflictValueViewModel : ViewModelBase
    {
        private static readonly Bitmap EmptyCover = new(AssetLoader.Open(new("avares://SUSUProgramming.MusicDownloader/Assets/music.png")));
        private readonly TaggingConflictInfo ownerConflict;
        private Bitmap? cover;
        private bool isCoverDirty = true;

        [ObservableProperty]
        private bool isLocked;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictValueViewModel"/> class.
        /// </summary>
        /// <param name="tag">Tag to set.</param>
        /// <param name="ownerConflict">Conflict with this tag.</param>
        public ConflictValueViewModel(ITag tag, TaggingConflictInfo ownerConflict)
        {
            this.ownerConflict = ownerConflict;
            ownerConflict.Resolved += (_, __) => IsLocked = true;
            ownerConflict.Rejected += (_, __) => IsLocked = true;
            Tag = tag;
            tag.ValueUpdated += (_, __) => isCoverDirty = IsImage;
        }

        /// <summary>
        /// Gets the conflict instance that owes current value.
        /// </summary>
        public TaggingConflictInfo Conflict => ownerConflict;

        /// <summary>
        /// Gets tag to visualize.
        /// </summary>
        public ITag Tag { get; }

        /// <summary>
        /// Gets a value indicating whether the tag is image.
        /// </summary>
        public bool IsImage => Tag is CoverTag;

        /// <summary>
        /// Gets a value indicating whether the tag is text.
        /// </summary>
        public bool IsText => !IsImage;

        /// <summary>
        /// Gets an image represented by specified tag.
        /// </summary>
        public Bitmap? Image
        {
            get
            {
                if (!IsImage)
                    return null;
                if (isCoverDirty)
                    UpdateCover();
                return cover ?? EmptyCover;
            }
        }

        /// <summary>
        /// Gets text represented by specified tag.
        /// </summary>
        public string? Text => Tag.Value is string[] s ? string.Join(", ", s) : Tag.Value?.ToString();

        /// <summary>
        /// Selects current value as solution to conflict.
        /// </summary>
        public void Select()
        {
            ownerConflict.Resolve(ownerConflict.FoundData.IndexOf(Tag));
        }

        private void UpdateCover()
        {
            cover?.Dispose();
            var data = (Tag as CoverTag)?.Cover?.Data;
            if (data == null)
                return;
            using var memory = new MemoryStream(data.Data);
            cover = new Bitmap(memory);
        }
    }
}