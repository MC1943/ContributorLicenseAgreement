﻿namespace ContributorLicenseAgreement.Core
{
    using System.Diagnostics.CodeAnalysis;
    using ContributorLicenseAgreement.Core.GitHubLinkClient;
    using ContributorLicenseAgreement.Core.Handlers;
    using ContributorLicenseAgreement.Core.Handlers.Helpers;
    using GitOps.Apps.Abstractions;
    using GitOps.Clients.Azure.BlobStorage;
    using GitOps.Clients.GitHub.Configuration;
    using GitOps.Common.Library.Extensions;
    using GitOps.Primitives;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    [ExcludeFromCodeCoverage]
    public sealed class ContributorLicenseAgreementStartup : AppStartupBase
    {
        /// <summary>
        /// This method is called when the app is initialized by the GitOps app server.
        /// Here the app can register items that will be added to the app's dependency
        /// injection container, just like a Startup.cs in a .NET web app.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        public override void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var azureBlobSettings = configuration.GetSection(nameof(AzureBlobSettings)).Get<AzureBlobSettings>();
            serviceCollection.TryAddSingleton<IBlobStorage>(
                p => new BlobStorage(
                    azureBlobSettings.AccountName,
                    azureBlobSettings.AccountKey,
                    true));
            var gitHubLinkSettings = configuration.GetSection(nameof(OspoGitHubLinkSettings)).Get<OspoGitHubLinkSettings>();
            serviceCollection.AddSingleton<OspoGitHubLinkSettings>(gitHubLinkSettings);
            serviceCollection.AddSingleton<IGitHubLinkRestClient, GitHubLinkRestClient>();
            serviceCollection.AddSingleton<PrimitiveCollection>();
            serviceCollection.RegisterAad(configuration);
            serviceCollection.AddSingleton<PullRequestHandler>();
            serviceCollection.AddSingleton<IssueCommentHandler>();
            serviceCollection.AddSingleton<PushHandler>();
            serviceCollection.AddSingleton<ClaHelper>();
            serviceCollection.AddSingleton<CommentHelper>();
            serviceCollection.AddSingleton<CheckHelper>();
            serviceCollection.AddSingleton<LoggingHelper>();
            serviceCollection.Configure<PlatformAppFlavorSettings>(
                configuration.GetSection(nameof(PlatformAppFlavorSettings)));

            // Legacy CLA app config. Remove once legacy app is disabled.
            serviceCollection.AddSingleton(configuration.GetSection(nameof(LegacyClaSettings)).Get<LegacyClaSettings>());
            serviceCollection.AddSingleton<LegacyClaCheckHandler>();
            serviceCollection.AddSingleton<LegacyClaCommentHandler>();
        }
    }
}
