using System.ComponentModel.DataAnnotations;


namespace QuizApp.Models;


public class Attempt
{
public int Id { get; set; }


[Required]
public int QuizId { get; set; }
public Quiz Quiz { get; set; } = default!;


[Required, StringLength(190)]
public string StudentId { get; set; } = "demo-student"; // replace with Identity UserId later


public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
public DateTime? SubmittedAtUtc { get; set; }


public int Score { get; set; }
public int MaxScore { get; set; }


public double Percentage => MaxScore == 0 ? 0 : (double)Score / MaxScore * 100.0;


public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}