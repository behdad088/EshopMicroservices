using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Identity.API.Models;

namespace Identity.API.Data;

public class VerificationCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } 
    
    public string UserId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = null!;

    public string Type { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }

    public bool IsActivated { get; set; } = false;
    
    public ApplicationUser User { get; set; } = null!;
    
    public bool IsExpired => 
        CreatedAt.AddMinutes(30) < DateTime.UtcNow; // Assuming 30 minutes expiration time
}