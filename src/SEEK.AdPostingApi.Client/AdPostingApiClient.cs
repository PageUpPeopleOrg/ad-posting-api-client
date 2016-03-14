﻿using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SEEK.AdPostingApi.Client.Models;
using SEEK.AdPostingApi.Client.Resources;

namespace SEEK.AdPostingApi.Client
{
    public class AdPostingApiClient : IAdPostingApiClient
    {
        private readonly IOAuth2TokenClient _tokenClient;
        private IndexResource _indexResource;
        private readonly Lazy<Task> _ensureIndexResourceInitialised;
        private readonly Hal.Client _client;

        public AdPostingApiClient(string id, string secret, Environment env = Environment.Production) : this(id, secret, env.GetAttribute<EnvironmentUrlAttribute>().Uri)
        {
        }

        public AdPostingApiClient(string id, string secret, Uri adPostingUri) : this(adPostingUri, new OAuth2TokenClient(id, secret))
        {
        }

        internal AdPostingApiClient(Uri adPostingUri, IOAuth2TokenClient tokenClient)
        {
            this._ensureIndexResourceInitialised = new Lazy<Task>(() => this.InitialiseIndexResource(adPostingUri), LazyThreadSafetyMode.ExecutionAndPublication);
            this._tokenClient = tokenClient;
            this._client = new Hal.Client(new HttpClient(new OAuthMessageHandler(tokenClient)));
        }

        private Task EnsureIndexResourceInitialised()
        {
            return this._ensureIndexResourceInitialised.Value;
        }

        internal async Task InitialiseIndexResource(Uri adPostingUri)
        {
            _indexResource = await this._client.GetResourceAsync<IndexResource>(adPostingUri);
        }

        public async Task<AdvertisementResource> CreateAdvertisementAsync(Advertisement advertisement)
        {
            if (advertisement == null)
            {
                throw new ArgumentNullException(nameof(advertisement));
            }

            await this.EnsureIndexResourceInitialised();

            return await this._indexResource.CreateAdvertisementAsync(advertisement);
        }

        public async Task<AdvertisementResource> CreateAdvertisementAsync(Advertisement advertisement, Uri uri)
        {
            if (advertisement == null)
            {
                throw new ArgumentNullException(nameof(advertisement));
            }

            return await this._client.PostResourceAsync<AdvertisementResource, Advertisement>(uri, advertisement);
        }

        public async Task<AdvertisementResource> ExpireAdvertisementAsync(Uri uri, AdvertisementPatch advertisementPatch)
        {
            return await this._client.PatchResourceAsync<AdvertisementResource, AdvertisementPatch>(uri, advertisementPatch);
        }

        public async Task<AdvertisementResource> GetAdvertisementAsync(Uri uri)
        {
            return await this._client.GetResourceAsync<AdvertisementResource>(uri);
        }

        public async Task<ProcessingStatus> GetAdvertisementStatusAsync(Uri uri)
        {
            HttpResponseHeaders httpResponseHeaders = await this._client.HeadResourceAsync<AdvertisementResource>(uri);

            return (ProcessingStatus)Enum.Parse(typeof(ProcessingStatus), httpResponseHeaders.GetValues("Processing-Status").Single());
        }

        public async Task<AdvertisementSummaryPageResource> GetAllAdvertisementsAsync()
        {
            await this.EnsureIndexResourceInitialised();

            return await _indexResource.GetAllAdvertisements();
        }

        public async Task<AdvertisementResource> UpdateAdvertisementAsync(Uri uri, Advertisement advertisement)
        {
            if (advertisement == null)
                throw new ArgumentNullException(nameof(advertisement));

            return await this._client.PutResourceAsync<AdvertisementResource, Advertisement>(uri, advertisement);
        }

        public void Dispose()
        {
            this._tokenClient.Dispose();
            this._client.Dispose();
        }
    }
}