using System.ComponentModel.DataAnnotations;
using Identity.API.Data;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? CardNumber { get; set; }
    public string? SecurityNumber { get; set; }
    public string? Expiration { get; set; }
    public string? CardHolderName { get; set; }
    public int? CardType { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    
    public ICollection<VerificationCode> EmailVerifications { get; set; } = new List<VerificationCode>();

}