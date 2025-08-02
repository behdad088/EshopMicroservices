using System.Security.Claims;
using Duende.IdentityModel;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Identity.API.Pages.Account.Register;

public class Index : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    
    public Index(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    [BindProperty]
    public RegisterViewModel Input { get; set; }
    
    [TempData]
    public string RegistrationMessage { get; set; } = string.Empty; // used to display messages on the page
    [TempData]
    public string? RegisteredUserEmail { get; set; }
    
    public async Task<IActionResult> OnGet()
    {
        Input = new RegisterViewModel()
        {
            ReturnUrl = Request.Query["returnUrl"]
        };
        
        
        if (TempData["RegistrationMessage"] is string message)
        {
            RegistrationMessage = message;
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {        
        if (ModelState.IsValid)
        {
            var dbUser = await _userManager.FindByNameAsync(Input.Email.ToLowerInvariant());
            
            if (dbUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                return Page();
            }
            
            var user = new ApplicationUser
            {
                UserName = Input.Email.ToLowerInvariant(),
                Email = Input.Email.ToLowerInvariant(),
                EmailConfirmed = true, // set it to true for simplicity
                Name = Input.Email.Split('@')[0], // use the part before @ as name
            };
            
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty,
                    $"failed to create user due to {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return Page();
            }
            
            result = await _userManager.AddToRoleAsync(user, Config.Roles.Customer);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "failed to add user to role.");
                return Page();
            }
            
            result = await AddClaimsAsync(user, [
                new Claim(JwtClaimTypes.Email, Input.Email)
            ]);
            
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }
        
        RegistrationMessage = "User created successfully. Check your email for verification!";
        RegisteredUserEmail = Input.Email.ToLowerInvariant();

        return RedirectToPage();
    }
    
    public IActionResult OnPostResendEmail()
    {
        if (string.IsNullOrEmpty(RegisteredUserEmail))
        {
            return RedirectToPage();
        }

        // Resend confirmation logic here
        RegistrationMessage = $"A new confirmation email was sent to {RegisteredUserEmail}.";
        return Page();
    }
    
    private async Task<IdentityResult> AddClaimsAsync(
        ApplicationUser user,
        Claim[] claims)
    {
        return await _userManager.AddClaimsAsync(user, claims).ConfigureAwait(false);
    }
}