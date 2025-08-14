// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Cantare.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cantare.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<UserModel> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            return Page();
        }
    }
}
