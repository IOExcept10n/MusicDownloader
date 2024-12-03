// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.Metadata.ID3;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SUSUProgramming.MusicDownloader.Services
{

    internal record TaggingResult(TrackDetails Details, ConflictsCollection Conflicts);
}