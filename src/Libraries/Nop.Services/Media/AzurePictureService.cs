﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Seo;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service for Windows Azure
    /// </summary>
    public partial class AzurePictureService : PictureService
    {
        #region Fields

        private static bool _azureBlobStorageAppendContainerName;
        private static bool _isInitialized;
        private static string _azureBlobStorageConnectionString;
        private static string _azureBlobStorageContainerName;
        private static string _azureBlobStorageEndPoint;
        private static CloudBlobContainer _container;

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly MediaSettings _mediaSettings;

        private readonly object _locker = new object();

        #endregion

        #region Ctor

        public AzurePictureService(AppSettings appSettings,
            INopDataProvider dataProvider,
            IDownloadService downloadService,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings)
            : base(dataProvider,
                  downloadService,
                  httpContextAccessor,
                  fileProvider,
                  productAttributeParser,
                  pictureRepository,
                  pictureBinaryRepository,
                  productPictureRepository,
                  settingService,
                  urlRecordService,
                  webHelper,
                  mediaSettings)
        {
            _staticCacheManager = staticCacheManager;
            _mediaSettings = mediaSettings;

            OneTimeInit(appSettings);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Initialize cloud container
        /// </summary>
        /// <param name="appSettings">App settings</param>
        protected void OneTimeInit(AppSettings appSettings)
        {
            if (_isInitialized)
                return;

            if (string.IsNullOrEmpty(appSettings.AzureBlobConfig.ConnectionString))
                throw new Exception("Azure connection string for Blob is not specified");

            if (string.IsNullOrEmpty(appSettings.AzureBlobConfig.ContainerName))
                throw new Exception("Azure container name for Blob is not specified");

            if (string.IsNullOrEmpty(appSettings.AzureBlobConfig.EndPoint))
                throw new Exception("Azure end point for Blob is not specified");

            lock (_locker)
            {
                if (_isInitialized)
                    return;

                _azureBlobStorageAppendContainerName = appSettings.AzureBlobConfig.AppendContainerName;
                _azureBlobStorageConnectionString = appSettings.AzureBlobConfig.ConnectionString;
                _azureBlobStorageContainerName = appSettings.AzureBlobConfig.ContainerName.Trim().ToLower();
                _azureBlobStorageEndPoint = appSettings.AzureBlobConfig.EndPoint.Trim().ToLower().TrimEnd('/');

                CreateCloudBlobContainer();

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Create cloud Blob container
        /// </summary>
        protected virtual async void CreateCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(_azureBlobStorageConnectionString);
            if (storageAccount == null)
                throw new Exception("Azure connection string for Blob is not working");

            //should we do it for each HTTP request?
            var blobClient = storageAccount.CreateCloudBlobClient();

            //GetContainerReference doesn't need to be async since it doesn't contact the server yet
            _container = blobClient.GetContainerReference(_azureBlobStorageContainerName);

            await _container.CreateIfNotExistsAsync();
            await _container.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async void DeletePictureThumbs(Picture picture)
        {
            await DeletePictureThumbsAsync(picture);
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            var path = _azureBlobStorageAppendContainerName ? _azureBlobStorageContainerName + "/" : string.Empty;

            return $"{_azureBlobStorageEndPoint}/{path}{thumbFileName}";
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            return GetThumbLocalPath(thumbFileName);
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            return GeneratedThumbExistsAsync(thumbFilePath, thumbFileName).Result;
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected override async void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            await SaveThumbAsync(thumbFilePath, thumbFileName, mimeType, binary);
        }

        /// <summary>
        /// Initiates an asynchronous operation to delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual async Task DeletePictureThumbsAsync(Picture picture)
        {
            //create a string containing the Blob name prefix
            var prefix = $"{picture.Id:0000000}";

            BlobContinuationToken continuationToken = null;
            do
            {
                //get result segment
                //listing snapshots is only supported in flat mode, so set the useFlatBlobListing parameter to true.
                var resultSegment = await _container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.All, null, continuationToken, null, null);

                //delete files in result segment
                await Task.WhenAll(resultSegment.Results.Select(blobItem => ((CloudBlockBlob)blobItem).DeleteAsync()));

                //get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefix);
        }

        /// <summary>
        /// Initiates an asynchronous operation to get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected virtual async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName)
        {
            try
            {
                var key = _staticCacheManager.PrepareKeyForDefaultCache(NopMediaDefaults.ThumbExistsCacheKey, thumbFileName);

                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    //GetBlockBlobReference doesn't need to be async since it doesn't contact the server yet
                    var blockBlob = _container.GetBlockBlobReference(thumbFileName);

                    return await blockBlob.ExistsAsync();
                });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Initiates an asynchronous operation to save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected virtual async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            //GetBlockBlobReference doesn't need to be async since it doesn't contact the server yet
            var blockBlob = _container.GetBlockBlobReference(thumbFileName);

            //set mime type
            if (!string.IsNullOrEmpty(mimeType))
                blockBlob.Properties.ContentType = mimeType;

            //set cache control
            if (!string.IsNullOrEmpty(_mediaSettings.AzureCacheControlHeader))
                blockBlob.Properties.CacheControl = _mediaSettings.AzureCacheControlHeader;

            await blockBlob.UploadFromByteArrayAsync(binary, 0, binary.Length);

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefix);
        }

        #endregion
    }
}