using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace MvcClient.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "Home/Index"
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
                {
                    RedirectUri = "Home/Index"
                }, OpenIdConnectDefaults.AuthenticationScheme,
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}