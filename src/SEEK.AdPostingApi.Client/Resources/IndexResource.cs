﻿using System;
using System.Threading.Tasks;
using SEEK.AdPostingApi.Client.Hal;
using SEEK.AdPostingApi.Client.Models;

namespace SEEK.AdPostingApi.Client.Resources
{
    public class IndexResource : HalResource
    {
        public Task<Uri> PostAdvertisementAsync(Advertisement advertisement)
        {
            return this.PostResourceAsync("advertisements", advertisement);
        }

        public Task<AdvertisementResource> GetAdvertisementByIdAsync(Guid id)
        {
            return this.GetResourceAsync<AdvertisementResource>("advertisement", new {advertisementId = id});
        }
        public Task<AdvertisementListResource> GetAllAdvertisements()
        {

            return this.GetResourceByQueryAsync<AdvertisementListResource>("/advertisements");
        }
        public Task<AdvertisementListResource> GetAllAdvertisements(string queryLink)
        {

            return this.GetResourceByQueryAsync<AdvertisementListResource>(queryLink);
        }

        public Task PutAdvertisementByIdAsync(Guid id, Advertisement advertisement)
        {
            return this.PutResourceAsync("advertisement", new { advertisementId = id }, advertisement);
        }
    }
}
