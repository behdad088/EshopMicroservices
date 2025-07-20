using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [Microsoft.Build.Framework.Required]
    public string CardNumber { get; set; }
    [Microsoft.Build.Framework.Required]
    public string SecurityNumber { get; set; }
    [Microsoft.Build.Framework.Required]
    [RegularExpression(@"(0[1-9]|1[0-2])\/[0-9]{2}", ErrorMessage = "Expiration should match a valid MM/YY value")]
    public string Expiration { get; set; }
    [Microsoft.Build.Framework.Required]
    public string CardHolderName { get; set; }
    public int CardType { get; set; }
    [Microsoft.Build.Framework.Required]
    public string Street { get; set; }
    [Microsoft.Build.Framework.Required]
    public string City { get; set; }
    [Microsoft.Build.Framework.Required]
    public string State { get; set; }
    [Microsoft.Build.Framework.Required]
    public string Country { get; set; }
    [Microsoft.Build.Framework.Required]
    public string ZipCode { get; set; }
    [Microsoft.Build.Framework.Required]
    public string Name { get; set; }
    [Microsoft.Build.Framework.Required]
    public string LastName { get; set; }
}