using DohrniiBackoffice.Domain.Abstract;
using DohrniiBackoffice.Domain.Entities;
using DohrniiBackoffice.DTO.Request;
using DohrniiBackoffice.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DohrniiBackoffice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LessonsController : DefaultController<ChaptersController>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonClassRepository _lessonClassRepository;
        private readonly ILessonActivityRepository _lessonActivityRepository;
        private readonly ILessonClassActivityRepository _lessonClassActivityRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IQuizUnlockActivityRepository _quizUnlockActivityRepository;
        private readonly IChapterActivityRepository _chapterActivityRepository;
        private readonly IClassQuestionRepository _classQuestionRepository;
        private readonly IClassQuestionAnswerRepository _classQuestionAnswerRepository;
        private readonly IQuestionAttemptRepository _questionAttemptRepository;


        public LessonsController(ICategoryRepository categoryRepository, ILessonRepository lessonRepository, ILessonClassRepository lessonClassRepository, 
            ILessonActivityRepository lessonActivityRepository, ILessonClassActivityRepository lessonClassActivityRepository, IChapterRepository chapterRepository,
            IQuizUnlockActivityRepository quizUnlockActivityRepository, IChapterActivityRepository chapterActivityRepository, IClassQuestionRepository classQuestionRepository,
            IClassQuestionAnswerRepository classQuestionAnswerRepository, IQuestionAttemptRepository questionAttemptRepository)
        {
            _categoryRepository = categoryRepository;
            _lessonRepository = lessonRepository;
            _lessonClassRepository = lessonClassRepository;
            _lessonActivityRepository = lessonActivityRepository;
            _lessonClassActivityRepository = lessonClassActivityRepository;
            _chapterRepository = chapterRepository;
            _quizUnlockActivityRepository = quizUnlockActivityRepository;
            _chapterActivityRepository = chapterActivityRepository;
            _classQuestionRepository = classQuestionRepository;
            _classQuestionAnswerRepository = classQuestionAnswerRepository;
            _questionAttemptRepository = questionAttemptRepository;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartLesson([FromBody] StartDTO dto)
        {
            try
            {
                var user = GetUser();
                if(user != null)
                {
                    var lessonActivity = _lessonActivityRepository.FindBy(c => c.Id == dto.Id && c.UserId == user.Id).FirstOrDefault();
                    if(lessonActivity == null)
                    {
                        var lesson = _lessonRepository.FindBy(c => c.Id == dto.Id).FirstOrDefault();
                        if(lesson != null)
                        {
                            var chapter = _chapterActivityRepository.FindBy(c => c.Id == lesson.ChapterId && c.UserId == user.Id).FirstOrDefault();
                            if(chapter == null)
                            {
                                var chapterActivity = new ChapterActivity
                                {
                                    ChapterId = lesson.ChapterId,
                                    CategoryId = lesson.Chapter.CategoryId,
                                    UserId = user.Id,
                                    DateStarted = DateTime.UtcNow,
                                    DateCompleted = DateTime.UtcNow,
                                    IsCompleted = false
                                };
                                _chapterActivityRepository.Add(chapterActivity);
                                await _chapterActivityRepository.Save(user.Email, _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString());
                            }
                            var lessonActivityObj = new LessonActivity
                            {
                                ChapterId = lesson.ChapterId,
                                CategoryId = lesson.Chapter.CategoryId,
                                LessonId = dto.Id,
                                UserId = user.Id,
                                DateStarted = DateTime.UtcNow,
                                DateCompleted = DateTime.UtcNow,
                                IsCompleted = false
                            };
                            _lessonActivityRepository.Add(lessonActivityObj);
                            await _lessonActivityRepository.Save(user.Email, _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString());

                            if (lesson.LessonClasses.Count > 0)
                            {
                                var mclass = lesson.LessonClasses.FirstOrDefault();
                                if(mclass != null)
                                {
                                    var classActivityObj = new LessonClassActivity
                                    {
                                        ChapterId = lesson.ChapterId,
                                        CategoryId = lesson.Chapter.CategoryId,
                                        LessonId = dto.Id,
                                        LessonClassId = mclass.Id,
                                        UserId = user.Id,
                                        DateStarted = DateTime.UtcNow,
                                        DateCompleted = DateTime.UtcNow,
                                        IsCompleted = false
                                    };
                                    _lessonClassActivityRepository.Add(classActivityObj);
                                    await _lessonClassActivityRepository.Save(user.Email, _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString());
                                }
                            }
                            
                            return Ok(new StartResponseDTO { IsStarted = true, Id = dto.Id, Name = lesson.Title });
                        }
                        return NotFound(new ErrorResponse { Details = "Record not found!" });
                    }
                    return Ok(new StartResponseDTO { IsStarted = true, Id = dto.Id });
                }
                else
                {
                    return NotFound(new ErrorResponse { Details = "We can't find your account!" });
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return InternalServerError(new ErrorResponse { Details = ex.Message });
            }
        }

        [HttpPut("complete")]
        public async Task<IActionResult> CompleteLesson([FromBody] CompleteDTO dto)
        {
            try
            {
                var user = GetUser();
                if (user != null)
                {
                    var lessonActivity = _lessonActivityRepository.FindBy(c => c.Id == dto.Id).FirstOrDefault();
                    if (lessonActivity != null)
                    {
                        lessonActivity.IsCompleted = true;
                        lessonActivity.DateCompleted = DateTime.UtcNow;
                        _lessonActivityRepository.Edit(lessonActivity);
                        await _lessonActivityRepository.Save(user.Email, _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString());
                        return Ok(new CompleteResponseDTO { IsStarted = true, Id = dto.Id });
                    }
                    return NotFound(new ErrorResponse { Details = "Record not found!" });
                }
                else
                {
                    return NotFound(new ErrorResponse { Details = "We can't find your account!" });
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return InternalServerError(new ErrorResponse { Details = ex.Message });
            }
        }

        [HttpGet("class/{Id:int}/questions")]
        [Produces(typeof(List<ClassQuestionDTO>))]
        public IActionResult GetQuestions(int Id)
        {
            try
            {
                var user = GetUser();
                if (user != null)
                {
                    var options = new List<ClassQuestionDTO>();
                    var qtns = _classQuestionRepository.FindBy(c => c.LessonClassId == Id);
                    options = _mapper.Map<List<ClassQuestionDTO>>(qtns);
                    foreach (var item in options)
                    {
                        item.Options = _mapper.Map<List<ClassQuestionOptionDTO>>(_classQuestionAnswerRepository.FindBy(c => c.ClassQuestionId == item.Id).ToList());
                        item.Attempts = _mapper.Map<List<ClassQuestionAttemptDTO>>(_questionAttemptRepository.FindBy(c => c.QuestionId == item.Id).ToList());
                        item.IsAttempted = item.Attempts.Count > 0;
                    }
                     
                    return Ok(options);
                }
                else
                {
                    return NotFound(new ErrorResponse { Details = "User not found!" });
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return InternalServerError(new ErrorResponse { Details = ex.Message });
            }
        }
    }
}
