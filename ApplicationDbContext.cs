
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
    public DbSet<ParticipantProfile> ParticipantProfiles { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<EventCategory> EventCategories { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary keys
        modelBuilder.Entity<UserAccount>().HasKey(u => u.UserId);
        modelBuilder.Entity<OrganizerProfile>().HasKey(o => o.OrganizerId);
        modelBuilder.Entity<ParticipantProfile>().HasKey(p => p.ParticipantId);
        // ... (do the same for all)

        // Unique email
        modelBuilder.Entity<UserAccount>().HasIndex(u => u.Email).IsUnique();

        // Relationships (1-1 and 1-M)
        modelBuilder.Entity<OrganizerProfile>()
            .HasOne(o => o.User)
            .WithOne(u => u.OrganizerProfile)
            .HasForeignKey<OrganizerProfile>(o => o.UserId);

        modelBuilder.Entity<ParticipantProfile>()
            .HasOne(p => p.User)
            .WithOne(u => u.ParticipantProfile)
            .HasForeignKey<ParticipantProfile>(p => p.UserId);

        // Event → Organizer
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganizerId);

        // EventCategory relationships
        modelBuilder.Entity<EventCategory>()
            .HasOne(ec => ec.Event)
            .WithMany(e => e.EventCategories)
            .HasForeignKey(ec => ec.EventId);

        modelBuilder.Entity<EventCategory>()
            .HasOne(ec => ec.Category)
            .WithMany(c => c.EventCategories)
            .HasForeignKey(ec => ec.CategoryId);

        // Enrollment
        modelBuilder.Entity<Enrollment>()
            .HasOne(en => en.EventCategory)
            .WithMany(ec => ec.Enrollments)
            .HasForeignKey(en => en.EventCategoryId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(en => en.Participant)
            .WithMany(p => p.Enrollments)
            .HasForeignKey(en => en.ParticipantId);

        // Result 1-1 with Enrollment
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrollment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrollmentId);
    }
}