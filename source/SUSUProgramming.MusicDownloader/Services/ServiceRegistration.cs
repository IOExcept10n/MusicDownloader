// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SUSUProgramming.MusicDownloader.Music;
using SUSUProgramming.MusicDownloader.Music.Metadata;
using SUSUProgramming.MusicDownloader.Music.StreamingServices;
using SUSUProgramming.MusicDownloader.Music.StreamingServices.Vk;
using SUSUProgramming.MusicDownloader.ViewModels;
using SUSUProgramming.MusicDownloader.Views;
using VkNet;
using VkNet.AudioBypassService.Extensions;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Provides extension methods for registering application services in an <see cref="IServiceCollection"/>.
    /// This class contains methods to add API services, application services, configuration services, media services,
    /// metadata services, view models, and views.
    /// </summary>
    internal static class ServiceRegistration
    {
        private static readonly Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();

        /// <summary>
        /// Adds API-related services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddSingleton<ApiHelper>()
                    .AddScoped<TokenStorage>()
                    .AddSingleton<AuthorizationCoordinator>();
            return services;
        }

        /// <summary>
        /// Adds all application-related services to the specified <see cref="IServiceCollection"/>.
        /// This method calls several other methods to register different categories of services.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddConfigurationServices()
                    .AddMediaServices()
                    .AddApiServices()
                    .AddMetadataServices()
                    .AddViewModels()
                    .AddViews();
            return services;
        }

        /// <summary>
        /// Adds configuration-related services to the specified <see cref="IServiceCollection"/>.
        /// This method sets up the configuration builder and loads application settings from a JSON file.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddConfigurationServices(this IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var config = builder.Build();
            var settings = AppConfig.LoadOrInitialize(config["SettingsPath"] ?? ".config");
            services.AddSingleton<IConfiguration>(config)
                    .AddSingleton(settings)
                    .AddLogging(builder =>
                    {
                        builder.AddConfiguration(config.GetSection("Logging"));
                        builder.AddDebug();
                    });
            return services;
        }

        /// <summary>
        /// Adds media-related services to the specified <see cref="IServiceCollection"/>.
        /// This method registers services related to audio processing and media management.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMediaServices(this IServiceCollection services)
        {
            services.AddAudioBypass();
            var vkApi = new VkApi(services);
            services.AddSingleton(vkApi)
                    .AddSingleton<VkOAuthService>()
                    .AddSingleton<IMediaProvider, VkMediaProvider>()
                    .AddSingleton<TracksAsyncLoader>()
                    .AddSingleton<MediaLibrary>();
            return services;
        }

        /// <summary>
        /// Adds metadata-related services to the specified <see cref="IServiceCollection"/>.
        /// This method registers detail and lyrics providers based on the types found in the executing assembly.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMetadataServices(this IServiceCollection services)
        {
            foreach (var type in AllTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;
                if (type.IsAssignableTo(typeof(IDetailProvider)))
                    services.AddSingleton(typeof(IDetailProvider), type);
                if (type.IsAssignableTo(typeof(ILyricsProvider)))
                    services.AddSingleton(typeof(ILyricsProvider), type);
            }

            services.AddSingleton<TagService>();
            return services;
        }

        /// <summary>
        /// Adds view model services to the specified <see cref="IServiceCollection"/>.
        /// This method registers view models based on their lifetime attributes.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            foreach (var type in AllTypes.Where(x => x.IsAssignableTo(typeof(ViewModelBase))))
            {
                if (type.GetCustomAttribute<SingletonAttribute>() != null)
                    services.AddSingleton(type);
                else if (type.GetCustomAttribute<ScopedAttribute>() != null)
                    services.AddScoped(type);
                else
                    services.AddTransient(type);
            }

            return services;
        }

        /// <summary>
        /// Adds view services to the specified <see cref="IServiceCollection"/>.
        /// This method registers views based on their lifetime attributes and the presence of a <see cref="ViewAttribute"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddViews(this IServiceCollection services)
        {
            foreach (var type in AllTypes.Where(x => x.GetCustomAttribute<ViewAttribute>() != null))
            {
                if (type.GetCustomAttribute<SingletonAttribute>() != null)
                    services.AddKeyedSingleton(typeof(UserControl), type.Name, type);
                else if (type.GetCustomAttribute<ScopedAttribute>() != null)
                    services.AddKeyedScoped(typeof(UserControl), type.Name, type);
                else
                    services.AddKeyedTransient(typeof(UserControl), type.Name, type);
            }

            return services;
        }
    }
}