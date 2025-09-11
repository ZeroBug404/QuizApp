using System.ComponentModel.DataAnnotations;


namespace QuizApp.Models;


public class Quiz
{
public int Id { get; set; }


[Required, StringLength(160)]
public string Title { get; set; } = string.Empty;


[StringLength(500)]
public string? Description { get; set; }


public int? TimeLimitMinutes { get; set; } // null = no time limit
public bool ShuffleQuestions { get; set; } = true;
public bool ShuffleOptions { get; set; } = true;


public ICollection<Question> Questions { get; set; } = new List<Question>();
}