using System.ComponentModel.DataAnnotations;
using Identity.API.Data;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [MaxLength(19)]
    public string? CardNumber { get; set; }
    [MaxLength(4)]
    public string? SecurityNumber { get; set; }
    [MaxLength(7)]
    public string? Expiration { get; set; }
    [MaxLength(100)]
    public string? CardHolderName { get; set; }
    public int? CardType { get; set; }
    [MaxLength(150)]
    public string? Street { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(50)]
    public string? State { get; set; }
    [MaxLength(50)]
    public string? Country { get; set; }
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    public ICollection<VerificationCode> EmailVerifications { get; set; } = new List<VerificationCode>();

}