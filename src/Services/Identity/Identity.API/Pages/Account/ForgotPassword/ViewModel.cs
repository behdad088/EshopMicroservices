using System.ComponentModel.DataAnnotations;

namespace Identity.API.Pages.Account.ForgotPassword;

public class ViewModel
{
    
    public string? Email { get; set; } = default!;
    public bool ShowMessage { get; set; } = false;
    
}