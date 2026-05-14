using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.Models
{
    public class MediQueueContext : IdentityDbContext<User>
    {
        public MediQueueContext(DbContextOptions<MediQueueContext> options)
        : base(options)
        {
        }

        // DbSets
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Queue> Queues { get; set; }
        public DbSet<DoctorAvailableSlot> DoctorAvailableSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(u => u.PatientAppointments)
                .HasForeignKey(a => a.PatientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(u => u.DoctorAppointments)
                .HasForeignKey(a => a.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(u => u.ClinicID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Queue)
                .WithOne(q => q.Appointment)
                .HasForeignKey<Queue>(q => q.AppointmentID);

            modelBuilder.Entity<Queue>()
                .HasOne(q => q.Doctor)
                .WithMany()
                .HasForeignKey(q => q.DoctorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorID, a.AppointmentDate, a.AppointmentTime });

            modelBuilder.Entity<Queue>()
                .HasIndex(q => new { q.DoctorID, q.Position });

            modelBuilder.Entity<DoctorAvailableSlot>()
                .HasOne(s => s.Doctor)
                .WithMany(u => u.AvailableSlots)
                .HasForeignKey(s => s.DoctorID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorAvailableSlot>()
                .HasIndex(s => new { s.DoctorID, s.Date, s.StartTime });
        }
    }
}
