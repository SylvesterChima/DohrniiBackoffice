using DohrniiBackoffice.Domain.Abstract;
using DohrniiBackoffice.DTO.Request;
using DohrniiBackoffice.DTO.Response;
using DohrniiBackoffice.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DohrniiBackoffice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : DefaultController<UserController>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonClassRepository _lessonClassRepository;
        private readonly ILessonActivityRepository _lessonActivityRepository;
        private readonly ILessonClassActivityRepository _lessonClassActivityRepository;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly IChapterActivityRepository _chapterActivityRepository;
        private readonly IChapterRepository _chapterRepository;


        public UserController(ICategoryRepository categoryRepository, ILessonRepository lessonRepository, ILessonClassRepository lessonClassRepository, ILessonActivityRepository lessonActivityRepository, ILessonClassActivityRepository lessonClassActivityRepository, IAppSettingsRepository appSettingsRepository, IChapterActivityRepository chapterActivityRepository, IChapterRepository chapterRepository)
        {
            _categoryRepository = categoryRepository;
            _lessonRepository = lessonRepository;
            _lessonClassRepository = lessonClassRepository;
            _lessonActivityRepository = lessonActivityRepository;
            _lessonClassActivityRepository = lessonClassActivityRepository;
            _appSettingsRepository = appSettingsRepository;
            _chapterActivityRepository = chapterActivityRepository;
            _chapterRepository = chapterRepository;
        }


        [HttpGet("status")]
        [Produces(typeof(UserStausDTO))]
        public IActionResult GetUserStatus()
        {
            try
            {
                var userStaus = new UserStausDTO();
                var user = GetUser();
                if (user != null)
                {
                    userStaus.UserId = user.Id;
                    userStaus.TotalCryptoJelly = user.TotalJelly;
                    userStaus.TotalXP = user.TotalXp;
                    userStaus.TotalDHN = user.TotalDhn;
                    var xpToJelly = _appSettingsRepository.GetAll().FirstOrDefault();
                    if (xpToJelly != null)
                        userStaus.XpPerCryptojelly = xpToJelly.XpToJelly;

                    userStaus.LessonsInprogress = new List<LessonInprogress>();
                    var lessonsInProgress = _lessonActivityRepository.FindBy(c => c.UserId == user.Id && c.IsCompleted == false).ToList();
                    if (lessonsInProgress.Count > 0)
                    {
                        foreach (var item in lessonsInProgress)
                        {
                            userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = item.CategoryId, LessonName = item.Lesson.Title, ChapterId = item.ChapterId, LessonId = item.LessonId });
                        }
                    }

                    var cats = _categoryRepository.GetAll().ToList();
                    foreach (var cat in cats)
                    {
                        foreach (var chapter in cat.Chapters.OrderBy(c=>c.Sequence))
                        {
                            var ca = _chapterActivityRepository.FindBy(c=>c.UserId == user.Id && c.ChapterId == chapter.Id).FirstOrDefault();
                            if(ca == null || ca.IsCompleted == false)
                            {
                                foreach (var lesson in chapter.Lessons.OrderBy(c => c.Sequence))
                                {
                                    var la = _lessonActivityRepository.FindBy(c => c.UserId == user.Id && c.LessonId == lesson.Id).FirstOrDefault();
                                    if(la == null)
                                    {
                                        if (userStaus.LessonsInprogress.FirstOrDefault(c => c.CategoryId == lesson.Chapter.CategoryId) == null)
                                        {
                                            userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = lesson.Chapter.CategoryId, LessonName = lesson.Title, ChapterId = lesson.ChapterId, LessonId = lesson.Id, IsNotStarted = true });
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (!la.IsCompleted)
                                        {
                                            if(userStaus.LessonsInprogress.FirstOrDefault(c=>c.CategoryId == lesson.Chapter.CategoryId) == null)
                                            {
                                                userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = lesson.Chapter.CategoryId, LessonName = lesson.Title, ChapterId = lesson.ChapterId, LessonId = lesson.Id });
                                                break;
                                            }
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }
                    return Ok(userStaus);






                    //var lessonsInProgress = _lessonActivityRepository.FindBy(c => c.UserId == user.Id && c.IsCompleted == false).ToList();
                    //if(lessonsInProgress.Count > 0)
                    //{
                    //    userStaus.LessonsInprogress = new List<LessonInprogress>();
                    //    foreach (var item in lessonsInProgress)
                    //    {
                    //        userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = item.CategoryId, LessonName = item.Lesson.Title, ChapterId=item.ChapterId, LessonId=item.LessonId });
                    //    }
                    //    return Ok(userStaus);
                    //}
                    //else
                    //{
                    //    var obj = _lessonActivityRepository.FindBy(c => c.UserId == user.Id && c.IsCompleted == true).OrderByDescending(c=>c.Id).FirstOrDefault();
                    //    if(obj != null)
                    //    {
                    //        userStaus.LessonsInprogress = new List<LessonInprogress>();
                    //        var lesson = _lessonRepository.FindBy(c=>c.Sequence==obj.Lesson.Sequence + 1).FirstOrDefault();
                    //        if( lesson != null)
                    //        {
                    //            userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = lesson.Chapter.CategoryId, LessonName = lesson.Title, ChapterId = lesson.ChapterId, LessonId = lesson.Id, IsNotStarted = true });
                    //            return Ok(userStaus);
                    //        }

                    //    }
                    //}
                    //var chapter = _chapterActivityRepository.FindBy(c => c.UserId == user.Id && c.IsCompleted == true).OrderByDescending(c => c.Id).FirstOrDefault();
                    //if(chapter != null)
                    //{
                    //    userStaus.LessonsInprogress = new List<LessonInprogress>();
                    //    var cht = _chapterRepository.FindBy(c => c.Sequence == chapter.Chapter.Sequence + 1).FirstOrDefault();
                    //    if (cht != null)
                    //    {
                    //        userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = cht.CategoryId, LessonName = cht.Lessons.ToList()[0].Title, ChapterId = cht.Id, LessonId = cht.Lessons.ToList()[0].Id, IsNotStarted = true });
                    //        return Ok(userStaus);
                    //    }
                    //}
                    //userStaus.LessonsInprogress = new List<LessonInprogress>();
                    //var mchapter = _chapterRepository.GetAll().OrderBy(c => c.Sequence).FirstOrDefault();
                    //if (mchapter != null)
                    //{
                    //    userStaus.LessonsInprogress.Add(new LessonInprogress { CategoryId = mchapter.CategoryId, LessonName = mchapter.Lessons.ToList()[0].Title, ChapterId = mchapter.Id, LessonId = mchapter.Lessons.ToList()[0].Id, IsNotStarted = true });
                    //    return Ok(userStaus);
                    //}
                    //return Ok(userStaus);

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

        [HttpGet("profile")]
        [Produces(typeof(UserResp))]
        public IActionResult UpdateProfile()
        {
            try
            {
                var userStaus = new UserStausDTO();
                var user = GetUser();
                if (user != null)
                {
                    return Ok(_mapper.Map<UserResp>(user));
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

        [HttpPost("profile")]
        [Produces(typeof(UserResp))]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            try
            {
                var userStaus = new UserStausDTO();
                var user = GetUser();
                if (user != null)
                {
                    user.UserName = dto.UserName;
                    user.FirstName = dto.FirstName;
                    user.LastName = dto.LastName;
                    user.WalletAddress = dto.WalletAddress;
                    user.Phone = dto.Phone;

                    _userRepository.Edit(user);
                    await _userRepository.Save(user.Email, _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString());
                    if (string.IsNullOrEmpty(user.ProfileImage))
                    {
                        user.ProfileImage = $"{GetBasedUrl()}{AuthConstants.DefaultProfile}";
                    }
                    return Ok(_mapper.Map<UserResp>(user));
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
