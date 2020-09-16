using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Models;
using Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Services;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AuthenticationController : BasePluginController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly GoogleAuthenticatorService _googleAuthenticatorService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor
        public AuthenticationController(
            CustomerSettings customerSettings,
            GoogleAuthenticatorService googleAuthenticatorService,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
        {
            _customerSettings = customerSettings;
            _googleAuthenticatorService = googleAuthenticatorService;
            _authenticationService = authenticationService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpPost]
        public IActionResult RegisterGoogleAuthenticator(AuthModel model)
        {
            var currentCustomer = _workContext.CurrentCustomer;

            var isValidToken = _googleAuthenticatorService.ValidateTwoFactorToken(model.SecretKey, model.Code);
            if (isValidToken)
            {
                //try to find config with current customer and update
                if (_googleAuthenticatorService.IsRegisteredCustomer(currentCustomer.Email))
                {
                    _googleAuthenticatorService.UpdateGoogleAuthenticatorAccount(currentCustomer.Email, model.SecretKey);
                }
                else
                {
                    _googleAuthenticatorService.AddGoogleAuthenticatorAccount(currentCustomer.Email, model.SecretKey);
                }
                _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.MultiFactorAuth.GoogleAuthenticator.Token.Successful"));
            }
            else
            {
                _notificationService.ErrorNotification(_localizationService.GetResource("Plugins.MultiFactorAuth.GoogleAuthenticator.Token.Unsuccessful"));
                return RedirectToRoute("CustomerMFAProviderConfig", new { providerSysName = GoogleAuthenticatorDefaults.SystemName });
            }
            
            return RedirectToRoute("MultiFactorAuthenticationSettings");
        }

        [HttpPost]
        public IActionResult VerifyGoogleAuthenticator(TokenModel model)
        {
            var username = HttpContext.Session.GetString(NopCustomerDefaults.MFAUserName);
            var returnUrl = HttpContext.Session.GetString(NopCustomerDefaults.MFAReturnUrl);
            bool.TryParse(HttpContext.Session.GetString(NopCustomerDefaults.MFARememberMe), out var isPersist);

            var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(username) : _customerService.GetCustomerByEmail(username);
            if (customer == null)
                return RedirectToRoute("Login");

            var record = _googleAuthenticatorService.GetConfigurationByCustomerEmail(customer.Email);
            if (record != null)
            {
                var isValidToken = _googleAuthenticatorService.ValidateTwoFactorToken(record.SecretKey, model.Token);
                if (isValidToken)
                {
                    //migrate shopping cart
                    _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                    //sign in new customer
                    _authenticationService.SignIn(customer, isPersist);

                    //raise event       
                    _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                    //activity log
                    _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                        _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                    if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                        return RedirectToRoute("Homepage");

                    return Redirect(returnUrl);
                }
                else
                {
                    _notificationService.ErrorNotification(_localizationService.GetResource("Plugins.MultiFactorAuth.GoogleAuthenticator.Token.Unsuccessful"));
                }
            }

            return RedirectToRoute("MultiFactorAuthorization");
        }

        #endregion

    }
}
