using System.ComponentModel.DataAnnotations;


namespace QuizApp.Models;


public class Option
{
    public int Id { get; set; }


    [Required]
    public int QuestionId { get; set; }
    public Question? Question { get; set; }


    [Required, StringLength(500)]
    public string Text { get; set; } = string.Empty;


    public bool IsCorrect { get; set; }
}
