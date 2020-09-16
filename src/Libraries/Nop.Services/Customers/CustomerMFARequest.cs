using System;
using System.Collections.Generic;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer MFA request
    /// </summary>
    public partial class CustomerMFARequest
    {
        public CustomerMFARequest()
        {
            CustomValues = new Dictionary<string, object>();
        }
        public string MFAUserName { get; set; }

        public bool MFARememberMe { get; set; }

        public string MFAReturnUrl { get; set; }

        /// <summary>
        /// You can store any custom value in this property
        /// </summary>
        public Dictionary<string, object> CustomValues { get; set; }
    }
}
