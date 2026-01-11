using Microsoft.AspNetCore.Identity;

namespace OnForkHub.Persistence.Models;

public class ApplicationUser : IdentityUser<long>
{
    public List<IdentityRole<long>>? Roles { get; set; }
}
