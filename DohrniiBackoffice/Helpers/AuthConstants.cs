using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DohrniiBackoffice.Helpers
{
    public class AuthConstants
    {
        public const string AuthSchemes = "Identity.Application" + "," + JwtBearerDefaults.AuthenticationScheme;
        public const string AdminRole = "Admin";
        public const string UserRole = "User";
        public const string EmailRegex = @"\A(?:[a-z0-9]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";
        public const string PhoneRegex = "\\A[0-9]{3}-[0-9]{3}-[0-9]{4}\\z";
        public const string DefaultProfile = "/imagefiles/defaultuser.png";
    }
}
