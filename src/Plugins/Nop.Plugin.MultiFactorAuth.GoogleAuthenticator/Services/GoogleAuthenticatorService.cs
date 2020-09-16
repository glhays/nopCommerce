using System;
using System.Linq;
using Google.Authenticator;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Domains;
using Nop.Services.Caching;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Services
{
    /// <summary>
    /// Represents Google Authenticator service
    /// </summary>
    public class GoogleAuthenticatorService
    {
        #region Fields

        private readonly ICacheKeyService _cacheKeyService;
        private readonly IRepository<GoogleAuthenticatorRecord> _repository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private TwoFactorAuthenticator _twoFactorAuthenticator;
        

        #endregion

        #region Ctr

        public GoogleAuthenticatorService(ICacheKeyService cacheKeyService,
            IRepository<GoogleAuthenticatorRecord> repository,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _cacheKeyService = cacheKeyService;
            _repository = repository;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }
        #endregion

        #region Properties

        private TwoFactorAuthenticator TwoFactorAuthenticator
        {
            get
            {
                _twoFactorAuthenticator = new TwoFactorAuthenticator();
                return _twoFactorAuthenticator;
            }
        }

        #endregion

        #region Utilites

        /// <summary>
        /// Insert the configuration
        /// </summary>
        /// <param name="configuration">Configuration</param>
        protected void InsertConfiguration(GoogleAuthenticatorRecord configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _repository.Insert(configuration);
            _staticCacheManager.RemoveByPrefix(GoogleAuthenticatorDefaults.PrefixCacheKey);
        }

        /// <summary>
        /// Update the configuration
        /// </summary>
        /// <param name="configuration">Configuration</param>
        protected void UpdateConfiguration(GoogleAuthenticatorRecord configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _repository.Update(configuration);
            _staticCacheManager.RemoveByPrefix(GoogleAuthenticatorDefaults.PrefixCacheKey);
        }

        /// <summary>
        /// Delete the configuration
        /// </summary>
        /// <param name="configuration">Configuration</param>
        internal void DeleteConfiguration(GoogleAuthenticatorRecord configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _repository.Delete(configuration);
            _staticCacheManager.RemoveByPrefix(GoogleAuthenticatorDefaults.PrefixCacheKey);
        }

        /// <summary>
        /// Get a configuration by the identifier
        /// </summary>
        /// <param name="configurationId">Configuration identifier</param>
        /// <returns>Configuration</returns>
        internal GoogleAuthenticatorRecord GetConfigurationById(int configurationId)
        {
            if (configurationId == 0)
                return null;

            return _staticCacheManager.Get(_cacheKeyService.PrepareKeyForDefaultCache(GoogleAuthenticatorDefaults.ConfigurationCacheKey, configurationId), () =>
                _repository.GetById(configurationId));
        }

        internal GoogleAuthenticatorRecord GetConfigurationByCustomerEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var query = _repository.Table;
            return query.Where(record => record.Customer == email && record.StoreId == _storeContext.CurrentStore.Id).FirstOrDefault();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get configurations
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged list of configurations</returns>
        public IPagedList<GoogleAuthenticatorRecord> GetPagedConfigurations(string email = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _repository.Table;
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Customer.Contains(email));
            query = query.OrderBy(configuration => configuration.Id);

            return new PagedList<GoogleAuthenticatorRecord>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Check if the customer is registered  
        /// </summary>
        /// <param name="customerEmail"></param>
        /// <returns></returns>
        public bool IsRegisteredCustomer(string customerEmail)
        {
            return GetConfigurationByCustomerEmail(customerEmail) != null;
        }

        /// <summary>
        /// Add configuration of GoogleAuthenticator
        /// </summary>
        /// <param name="customerEmail">Customer email</param>
        /// <param name="key">Secret key</param>
        public void AddGoogleAuthenticatorAccount(string customerEmail, string key)
        {
            var account = new GoogleAuthenticatorRecord
            {
                Customer = customerEmail,
                SecretKey = key,
                StoreId  = _storeContext.CurrentStore.Id
            };
            InsertConfiguration(account);

        }

        /// <summary>
        /// Update configuration of GoogleAuthenticator
        /// </summary>
        /// <param name="customerEmail">Customer email</param>
        /// <param name="key">Secret key</param>
        public void UpdateGoogleAuthenticatorAccount(string customerEmail, string key)
        {
            var account = GetConfigurationByCustomerEmail(customerEmail);
            if (account != null)
            {
                account.SecretKey = key;
                UpdateConfiguration(account);
            }
        }

        /// <summary>
        /// Generate a setup code for a Google Authenticator user to scan
        /// </summary>
        /// <param name="secretkey">Secret key</param>
        /// <returns></returns>
        public SetupCode GenerateSetupCode(string secretkey)
        {
            return TwoFactorAuthenticator.GenerateSetupCode(
                _storeContext.CurrentStore.CompanyName, 
                _workContext.CurrentCustomer.Email, 
                secretkey, false, GoogleAuthenticatorDefaults.DefaultQRPixelsPerModule);
        }

        /// <summary>
        /// Validate token auth
        /// </summary>
        /// <param name="secretkey">Secret key</param>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public bool ValidateTwoFactorToken(string secretkey, string token)
        {
            return TwoFactorAuthenticator.ValidateTwoFactorPIN(secretkey, token);
        }

        #endregion
    }
}
