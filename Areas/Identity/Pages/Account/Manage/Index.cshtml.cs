// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System.ComponentModel.DataAnnotations;
using BlogHub.Models;
using BlogHub.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogHub.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<MyUser> _userManager;
        private readonly SignInManager<MyUser> _signInManager;
        private readonly IWebHostEnvironment _env;
        private readonly IUnitOfWork _unitOfWork;


        public IndexModel(
            UserManager<MyUser> userManager,
            SignInManager<MyUser> signInManager,IWebHostEnvironment env,IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePicture { get; set; }



        }

        private async Task LoadAsync(MyUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            user.ImageId = _userManager.Users.FirstOrDefault(u => u.Id == user.Id).ImageId;
            user.Image = _userManager.Users.Select(u => u.Image).FirstOrDefault(i => i.ImageId == user.ImageId);
            using (var stream =System.IO.File.OpenRead(Path.Combine(_env.WebRootPath,user.Image.ImageURL)))
            {
                Input = new InputModel
                {
                    PhoneNumber = phoneNumber,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePicture = new FormFile(stream, 0, stream.Length, stream.Name, stream.Name)
                };
            }
            ViewData["ImageUrl"] = user.Image.ImageURL;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to update your profile.";
                    return RedirectToPage();
                }
            }
            if(Input.ProfilePicture != null)
            {
                user.Image = _userManager.Users.Select(u => u.Image).FirstOrDefault(i => i.ImageId == user.ImageId);
                var toDelete = user.Image;
                var Image = new Image
                {
                    ImageURL = await _unitOfWork.ImageRepository.SaveImageAsync(Input.ProfilePicture, "profileImages")
                };
                await _unitOfWork.ImageRepository.AddAsync(Image);
                user.ImageId = Image.ImageId;
                user.Image=Image;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to update your profile.";
                    return RedirectToPage();
                }
                _unitOfWork.ImageRepository.RemoveImage(toDelete.ImageURL);
                if (toDelete.ImageId != 17025) //default image
                {
                    await _unitOfWork.ImageRepository.DeleteAsync(toDelete.ImageId);
                }   
                await _unitOfWork.SaveChangesAsync();
            } 
            if (Input.FirstName != user.FirstName || Input.LastName != user.LastName)
            {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to update your profile.";
                    return RedirectToPage();
                }
            }  
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
