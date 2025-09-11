using Microsoft.EntityFrameworkCore;
using QuizApp.Models;


namespace QuizApp.Data;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<Student> Students => Set<Student>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Option → Question : CASCADE (when a question is deleted, its options go)
        b.Entity<Option>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Question → Quiz : CASCADE (when a quiz is deleted, its questions go)
        b.Entity<Question>()
            .HasOne(q => q.Quiz)
            .WithMany(z => z.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        // AttemptAnswer → Attempt : CASCADE (delete attempt = delete its answers)
        b.Entity<AttemptAnswer>()
            .HasOne(a => a.Attempt)
            .WithMany(at => at.Answers)
            .HasForeignKey(a => a.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // AttemptAnswer → Question : **NO CASCADE** to avoid multiple cascade paths
        b.Entity<AttemptAnswer>()
            .HasOne(a => a.Question)
            .WithMany() // no back-collection required
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // AttemptAnswer → Option : keep default (NO ACTION). SelectedOptionId is nullable.
        b.Entity<AttemptAnswer>()
            .HasOne(a => a.SelectedOption)
            .WithMany()
            .HasForeignKey(a => a.SelectedOptionId)
            .OnDelete(DeleteBehavior.NoAction);

        // Student: unique email
        b.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();
    }
}
