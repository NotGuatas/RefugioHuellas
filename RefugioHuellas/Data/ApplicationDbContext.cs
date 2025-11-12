using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RefugioHuellas.Models;

namespace RefugioHuellas.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Dog> Dogs => Set<Dog>();   

        public DbSet<AdoptionApplication> AdoptionApplications => Set<AdoptionApplication>();

        public DbSet<PersonalityTrait> PersonalityTraits => Set<PersonalityTrait>();
        public DbSet<UserTraitResponse> UserTraitResponses => Set<UserTraitResponse>();

        public DbSet<AdoptionApplicationAnswer> AdoptionApplicationAnswers => Set<AdoptionApplicationAnswer>();

    }
}
