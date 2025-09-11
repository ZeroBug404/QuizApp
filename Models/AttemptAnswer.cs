namespace QuizApp.Models;


public class AttemptAnswer
{
public int Id { get; set; }


public int AttemptId { get; set; }
public Attempt Attempt { get; set; } = default!;


public int QuestionId { get; set; }
public Question Question { get; set; } = default!;


public int? SelectedOptionId { get; set; }
public Option? SelectedOption { get; set; }


public bool IsCorrect { get; set; }
}