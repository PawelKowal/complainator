using System;
using Microsoft.AspNetCore.Identity;

namespace ComplainatorAPI.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        // Additional properties can be added here if needed in the future
    }
}