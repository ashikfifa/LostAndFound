using LostAndFound.Data.Data;
using LostAndFound.Data.Models;
using LostAndFound.Data.StaticData;
using LostAndFound.Models;
using LostAndFound.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LostAndFound.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICommonService _commonService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string bucketBasePath;


        public HomeController(IConfiguration configuration, ICommonService commonService, ApplicationDbContext context)
        {
            _configuration = configuration;
            _commonService = commonService;
            _context = context;

            bucketBasePath = _configuration["AppSettings:bucketBasePath"];
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexNew()
        {
            return View();
        }

        public IActionResult Contributor()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UserPostCreate(string type, string comment, string phone)
        {
            try
            {
                var post = new UserPost()
                {
                    Comment = comment ?? string.Empty,
                    Date = DateTime.Now,
                    PhoneNo = phone,
                    PostType = type,
                    IsVerified = false
                };

                var otp = _commonService.GenerateOTP();
                post.OTP = otp.ToString();
                post.OTPCreatedAt = DateTime.Now;

                _commonService.SendSMS(phone, $"Dear ifinder User, Your OTP for the Post is {otp}. Please Use this Code to Validate Your Identity.");

                _context.UserPosts.Add(post);
                _context.SaveChanges();

                return Json(new Response<UserPost>()
                {
                    Result = post,
                    Success = true,
                    Message = "Post Created Successfully!"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<UserPost>()
                {
                    Result = null,
                    Success = false,
                    Message = "Error Occurred While Creating a Post!"
                });
            }
        }

        [HttpPost]
        public IActionResult UploadPostImages(string id)
        {
            try
            {
                List<IFormFile> files = Request.Form.Files.ToList();

                var post = _context.UserPosts.Find(id);

                List<Image> images = new List<Image>();

                int humanCount = 0;

                foreach(var file in files)
                {
                    var image = new Image
                                {
                                    UserPostId = post.Id,
                                    Status = ImageStatus.Pending,
                                };

                    _context.Images.Add(image);
                    _context.SaveChanges();

                    var extension = file.FileName.Split('.')[1];

                    image.Name = $"{image.Id}_{post.PostType}.{extension}";
                    _context.SaveChanges();

                    _commonService.UploadImage(file, image.Name);

                    var isFaceDetected = _commonService.IsFaceDetectedOnImageAsync(image.Name).Result;

                    image.IsFaceDetected = isFaceDetected;
                    _context.SaveChanges();

                    if (isFaceDetected == true) ++humanCount;
                }

                if(humanCount == 0)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "Please Upload a Photo of a Person."
                    });
                }

                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = true,
                    Message = "Image Uploaded Successfully!"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Failed to Upload Image!"
                });
            }
        }

        public IActionResult VerifyOtp(string id, string otp)
        {
            try
            {
                var post = _context.UserPosts.FirstOrDefault(m => m.Id.Equals(id));

                if(post == null)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "Post does not exist!"
                    });
                }

                if (!post.OTP.Equals(otp))
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "OTP didn't Match!"
                    });
                }

                if((DateTime.Now-post.OTPCreatedAt).TotalMinutes > 5)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "OTP Expired. Please Resend OTP!"
                    });
                }

                post.IsVerified = true;
                _context.SaveChanges();


                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = true,
                    Message = "OTP Verification Successful"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Error Occured While OTP Verification!"
                });
            }
        }

        public IActionResult GenerateOtpAgain(string id)
        {
            try
            {
                var post = _context.UserPosts.FirstOrDefault(m => m.Id.Equals(id));

                if(post == null)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "There is no post on this id"
                    });
                }

                var previousOtp = Convert.ToInt32(post.OTP);
                var otp = _commonService.GenerateOTP(previousOtp);

                post.OTP = otp.ToString();
                post.OTPCreatedAt = DateTime.Now;

                _context.SaveChanges();

                _commonService.SendSMS(post.PhoneNo, $"Dear ifinder User, Your OTP for the Post is {otp}. Please Use this Code to Validate Your Identity.");


                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = true,
                    Message = "OTP Sent Successfully!"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Failed to Send OTP!"
                });
            }
        }

        public IActionResult SearchAllImages()
        {
            try
            {
                var pendingImages = _context.Images
                                            .Include(m => m.UserPost)
                                            .Where(m =>
                                                m.IsFaceDetected == true &&
                                                m.Status.Equals(ImageStatus.Pending)
                                            )
                                            .ToList();

                foreach(var pendingImage in pendingImages)
                {
                    if (pendingImage.Status.Equals(ImageStatus.Resolved)) continue;

                    var matchedImageNames = _commonService.GetMatch(pendingImage.Name).Result;

                    if(matchedImageNames != null && matchedImageNames.Count != 0)
                    {
                        pendingImage.UserPost.IsMatched = true;
                        pendingImage.Status = ImageStatus.Resolved;

                        var pendingImageId = pendingImage.Id;
                        var matchedIds = matchedImageNames.Select(m => m.Split('_')[0]).ToList();

                        var matchedImages = pendingImages
                                                    .Where(m =>
                                                        matchedIds.Any(x => x.Equals(m.Id))
                                                    )
                                                    .ToList();

                        matchedImages.ForEach(m =>
                        {
                            m.Status = ImageStatus.Resolved;
                            m.UserPost.IsMatched = true;
                        });

                        List<SimilarImage> similarImages = new List<SimilarImage>();

                        matchedIds.ForEach(m => {
                            var similarImage = new SimilarImage();

                            if (pendingImage.UserPost.PostType.Equals(PostType.Lost))
                            {
                                similarImage.LostImageId = pendingImageId;
                                similarImage.FoundImageId = m;
                            }
                            else
                            {
                                similarImage.FoundImageId = pendingImageId;
                                similarImage.LostImageId = m;
                            }

                            similarImages.Add(similarImage);
                        });

                        _context.SimilarImages.AddRange(similarImages);
                    }
                }

                 
                _context.SaveChanges();


                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = true,
                    Message = "Successful"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Failed"
                });
            }
        }

        [HttpPost]
        public IActionResult GetPosts()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var userPosts = _context.UserPosts.Where(x => x.IsVerified == true).OrderByDescending(m => m.Date).ToList();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    userPosts = userPosts.Where(m =>
                                            m.PhoneNo.Contains(searchValue) ||
                                            m.Date.ToString("dd MMM yyyy").Contains(searchValue.ToUpper()) ||
                                            _commonService.PostNumberBuilder(m.UserPostNumber).Contains(searchValue) ||
                                            (m.Comment != null && m.Comment.ToUpper().Contains(searchValue.ToUpper())) ||
                                            m.PostType.ToUpper().Contains(searchValue.ToUpper())
                                        ).ToList();
                }

                List<PostModel> postsData = new List<PostModel>();

                userPosts.ForEach(m => {
                    var postData = new PostModel
                    {
                        Id = m.Id,
                        Comment = Regex.Replace(m.Comment, "[0-9]", "*"),
                        Date = m.Date.ToString("dd MMM yyyy"),
                        IsMatched = m.IsMatched,
                        PostType = m.PostType,
                        UserPostNumber = _commonService.PostNumberBuilder(m.UserPostNumber)
                    };

                    var images = _context.Images
                                        .Where(x => x.UserPostId.Equals(m.Id) && x.IsFaceDetected == true)
                                        .Select(x => bucketBasePath + x.Name)
                                        .ToList();

                    postData.Images = images;

                    postsData.Add(postData);
                });

                postsData = postsData.Where(m => m.Images != null && m.Images.Count() > 0).ToList();

                /*
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    postsData = postsData.OrderBy(s => sortColumn + " " + sortColumnDirection);
                }
                */

                recordsTotal = postsData.Count();
                var data = postsData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public IActionResult GetMatchedImages(string id)
        {
            try
            {
                var post = _context.UserPosts.FirstOrDefault(m => m.Id.Equals(id));

                if(post == null) {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "Invalid post"
                    });
                }

                var images = _context.Images
                                    .Where(m => m.UserPostId.Equals(id))
                                    .ToList();

                var imageIds = images.Select(m => m.Name.Split('_')[0]).ToList();


                var imageURLs = new List<MatchedImage>();

                if (post.PostType.Equals(PostType.Lost))
                {
                    imageURLs = _context.SimilarImages
                                    .Include(m => m.LostImage)
                                    .Include(m => m.FoundImage)
                                    .Where(m => imageIds.Any(x => x.Equals(m.LostImageId)))
                                    .Select(m => new MatchedImage
                                    { 
                                        ImagePath = bucketBasePath + m.FoundImage.Name,
                                        PostId = m.FoundImage.UserPostId
                                    })
                                    .ToList();
                }
                else
                {
                    imageURLs = _context.SimilarImages
                                    .Include(m => m.LostImage)
                                    .Include(m => m.FoundImage)
                                    .Where(m => imageIds.Any(x => x.Equals(m.FoundImageId)))
                                    .Select(m => new MatchedImage
                                    {
                                        ImagePath = bucketBasePath + m.LostImage.Name,
                                        PostId = m.LostImage.UserPostId
                                    })
                                    .ToList();
                }

                return Json(new Response<List<MatchedImage>>()
                {
                    Result = imageURLs,
                    Success = true,
                    Message = "Matching Image found Successfully!"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Error Occurred, Failed to Get Matching Images!"
                });
            }
        }

        public IActionResult GenerateUnlockOtp(string id)
        {
            try
            {
                var post = _context.UserPosts.FirstOrDefault(m => m.Id.Equals(id));

                if(post == null)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "Invalid post"
                    });
                }

                int unlockingOtp;
                if (!string.IsNullOrEmpty(post.UnlockOTP))
                {
                    int previousOtp = Convert.ToInt32(post.UnlockOTP);
                    unlockingOtp = _commonService.GenerateOTP(previousOtp);
                }
                else
                {
                    unlockingOtp = _commonService.GenerateOTP();
                }
                
                post.UnlockOTP = unlockingOtp.ToString();
                post.UnlockOTPCreatedAt = DateTime.Now;

                _commonService.SendSMS(post.PhoneNo, $"Dear ifinder User, Your OTP for Unlocking Post is {unlockingOtp}. Please Use this Code to Validate Your Identity.");
                _context.SaveChanges();

                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = true,
                    Message = "OTP Sent Successfully!"
                });
            }
            catch(Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Error Occurred, Failed to Send OTP!"
                });
            }
        }

        public IActionResult VerifyUnlockOtp(string id, string otp, string matchedPostIds)
        {
            try
            {
                List<string> matchedPostIdList = matchedPostIds.Split(',').ToList();
                List<string> phoneNumbers = new List<string>();

                matchedPostIdList.ForEach(x => {
                    var matchedPost = _context.UserPosts.FirstOrDefault(m => m.Id.Equals(x));

                    if(matchedPost != null) phoneNumbers.Add(matchedPost.PhoneNo);
                });


                var post = _context.UserPosts.FirstOrDefault(m => m.Id.Equals(id));

                if (post == null)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "Post did not Exist!"
                    });
                }

                if (!post.UnlockOTP.Equals(otp))
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "OTP didn't match!"
                    });
                }

                if ((DateTime.Now - post.UnlockOTPCreatedAt.Value).TotalMinutes > 5)
                {
                    return Json(new Response<dynamic>()
                    {
                        Result = null,
                        Success = false,
                        Message = "OTP Expired. Please Resend OTP!"
                    });
                }

                phoneNumbers = phoneNumbers.Distinct().ToList();

                _commonService.SendSMS(post.PhoneNo,
                    $"Dear ifinder User, Please Contact this Number {string.Join(',', phoneNumbers)} to Find the Missing Person since a Similar Post has been Made from this Number.");

                _commonService.SendSMS(string.Join(',', phoneNumbers),
                    $"Dear ifinder User, Please Contact this Number {post.PhoneNo} to Find the Missing Person since a Similar Post has been Made from this Number.");


                post.IsUnlocked = true;
                _context.SaveChanges();


                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = true,
                    Message = "Dear ifinder User, Details have been sent to your mobile. Please check it out."
                });
            }
            catch (Exception e)
            {
                return Json(new Response<dynamic>()
                {
                    Result = null,
                    Success = false,
                    Message = "Error Occured While OTP Verification!"
                });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
