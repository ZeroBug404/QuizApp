using System.ComponentModel.DataAnnotations;


namespace QuizApp.Models;


public class Question
{
public int Id { get; set; }


[Required]
public int QuizId { get; set; }
public Quiz? Quiz { get; set; }


[Required, StringLength(1000)]
public string Text { get; set; } = string.Empty;


public ICollection<Option> Options { get; set; } = new List<Option>();
}
