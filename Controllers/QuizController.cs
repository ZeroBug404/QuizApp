using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly AppDbContext _db;
        private readonly Random _rand = new();

        public QuizController(AppDbContext db) => _db = db;

        // GET /Quiz
        public async Task<IActionResult> Index()
        {
            var quizzes = await _db.Quizzes
                .Select(q => new { q.Id, q.Title, q.Description, q.TimeLimitMinutes })
                .ToListAsync();
            return View(quizzes);
        }

        // GET /Quiz/Take/5
        public async Task<IActionResult> Take(int id)
        {
            var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return NotFound();

            // shuffle
            var questions = quiz.Questions.ToList();
            if (quiz.ShuffleQuestions) questions = questions.OrderBy(_ => _rand.Next()).ToList();
            foreach (var q in questions)
                if (quiz.ShuffleOptions)
                    q.Options = q.Options.OrderBy(_ => _rand.Next()).ToList();

            // create attempt
            var attempt = new Attempt
            {
                QuizId = id,
                StudentId = User?.Identity?.Name ?? "demo-student",
                StartedAtUtc = DateTime.UtcNow,
                MaxScore = questions.Count
            };
            _db.Attempts.Add(attempt);
            await _db.SaveChangesAsync();

            ViewBag.AttemptId = attempt.Id;
            ViewBag.TimeLimitMinutes = quiz.TimeLimitMinutes;
            return View(questions);
        }

        // POST /Quiz/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int attemptId, List<AnswerPost> answers)
        {
            var attempt = await _db.Attempts
                .Include(a => a.Quiz)
                .FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attempt == null) return NotFound();

            var questionIds = answers.Select(a => a.QuestionId).ToList();
            var options = await _db.Options
                .Where(o => questionIds.Contains(o.QuestionId))
                .ToListAsync();

            int score = 0;
            foreach (var a in answers)
            {
                var selected = a.SelectedOptionId.HasValue
                    ? options.FirstOrDefault(o => o.Id == a.SelectedOptionId.Value)
                    : null;

                bool isCorrect = selected?.IsCorrect == true;

                attempt.Answers.Add(new AttemptAnswer
                {
                    QuestionId = a.QuestionId,
                    SelectedOptionId = a.SelectedOptionId,
                    IsCorrect = isCorrect
                });

                if (isCorrect) score++;
            }

            attempt.Score = score;
            attempt.SubmittedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = attempt.Id });
        }

        // GET /Quiz/Result/123
        public async Task<IActionResult> Result(int id)
        {
            var attempt = await _db.Attempts
                .Include(a => a.Quiz)
                .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOption)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (attempt == null) return NotFound();
            return View(attempt);
        }
    }

    // Put this DTO in its own file if you prefer: Models/AnswerPost.cs
    public class AnswerPost
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
    }
}
