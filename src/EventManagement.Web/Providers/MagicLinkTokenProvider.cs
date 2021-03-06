using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace losol.EventManagement.Web.Providers
{
    public class MagicLinkTokenProvider<TUser> : DataProtectorTokenProvider<TUser>  
        where TUser: class  
    {
        public MagicLinkTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<MagicLinkTokenProviderOptions> options) 
            : base(dataProtectionProvider, options)
        {

        }
    }
}