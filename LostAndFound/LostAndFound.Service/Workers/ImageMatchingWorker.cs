using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LostAndFound.Data.Data;
using LostAndFound.Data.Models;
using LostAndFound.Data.StaticData;
using LostAndFound.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LostAndFound.Service.Workers
{
    public class ImageMatchingWorker : IImageMatchingWorker
    {
        private readonly ILogger<ImageMatchingWorker> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ICommonService _commonService;
        private readonly int MinutesInMillisecond = 2 * 60 * 1000 ;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public ImageMatchingWorker(IServiceScopeFactory serviceScopeFactory, ILogger<ImageMatchingWorker> logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            _context = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _commonService = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ICommonService>();
        }


        public async Task DoWork(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ImageMatching();

                _logger.LogInformation("ImageMatchingWorker Executed");

                await Task.Delay(MinutesInMillisecond);
            }
        }

        private bool ImageMatching()
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

                List<Tuple<string, string>> matchedListOfPhoneWithPostNumbers = new List<Tuple<string, string>>();
                

                foreach (var pendingImage in pendingImages)
                {
                    if (pendingImage.Status.Equals(ImageStatus.Resolved)) continue;

                    var matchedImageNames = _commonService.GetMatch(pendingImage.Name).Result;

                    if (matchedImageNames != null && matchedImageNames.Count != 0)
                    {
                        var tupleForPendingImage = new Tuple<string, string>(
                                            pendingImage.UserPost.PhoneNo,
                                            _commonService.PostNumberBuilder(pendingImage.UserPost.UserPostNumber)
                                        );

                        matchedListOfPhoneWithPostNumbers.Add(tupleForPendingImage);


                        pendingImage.UserPost.IsMatched = true;
                        pendingImage.Status = ImageStatus.Resolved;

                        var pendingImageId = pendingImage.Id;
                        var matchedIds = matchedImageNames.Select(m => m.Split('_')[0]).ToList();

                        var matchedImages = pendingImages
                                                    .Where(m =>
                                                        matchedIds.Any(x => x.Equals(m.Id))
                                                    )
                                                    .ToList();

                        var matchedPostId = matchedImages.Select(x => x.UserPostId)
                                                        .Distinct()
                                                        .ToList();

                        var matchedPost = pendingImages.Where(x => matchedPostId.Any(m => m.Equals(x.UserPostId)))
                                                        .Select(x => x.UserPost)
                                                        .ToList();

                        var listTupleForMatchedImage = matchedPost.Select(x =>
                                        new Tuple<string, string>(
                                            x.PhoneNo,
                                            _commonService.PostNumberBuilder(x.UserPostNumber)
                                        )
                                      ).ToList();


                        matchedListOfPhoneWithPostNumbers.AddRange(listTupleForMatchedImage);


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


                matchedListOfPhoneWithPostNumbers.ForEach(x => {
                    string message = $"Dear ifinder User, A Match for the Missing Person has been Found by ifinder. Please search on ifinder. Post No: {x.Item2}";
                    message = message.Replace("#", string.Empty);

                    _commonService.SendSMS(x.Item1, message);

                    Task.Delay(1000).Wait();
                });

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
