using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ERNI.PhotoDatabase.Server.Utils
{
    public static class PrincipalExtensions
    {
        public static bool CanWrite(this IPrincipal principal)
        {
            return principal.IsInRole("uploader");
        }
    }
}
