using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI;

public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; } = null!;
}