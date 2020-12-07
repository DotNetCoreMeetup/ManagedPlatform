using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Controllers
{

    public class PostController : ControllerBase
    {
        private HttpClient _httpClient;
        private Options _options;

        public PostController(HttpClient httpClient, Options options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        private async Task<BlobContainerClient> GetCloudBlobContainer(string containerName)
        {
            BlobServiceClient serviceClient = new BlobServiceClient(_options.StorageConnectionString);
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);
            
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }

        [Route("/post/")]
        [HttpPost]
        public async Task<ActionResult> Post()
        {
            Stream image = Request.Body;
            BlobContainerClient containerClient = await GetCloudBlobContainer(_options.FullImageContainerName);
            string blobName = Guid.NewGuid().ToString().ToLower().Replace("-", String.Empty);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(image);
            return Created(blobClient.Uri, null);
        }
    }
}
