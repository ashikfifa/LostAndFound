using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using LostAndFound.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Service.Common
{
    public class CommonService : ICommonService
    {
        private readonly IConfiguration _configuration;
        private readonly string bucketName;
        private readonly string awsS3AccessKeyId;
        private readonly string awsS3SecretAccessKey;
        private readonly string APIBaseURL;

        public CommonService(IConfiguration configuration)
        {
            _configuration = configuration;

            bucketName = _configuration["AppSettings:bucketName"];
            awsS3AccessKeyId = _configuration["AppSettings:awsS3AccessKeyId"];
            awsS3SecretAccessKey = _configuration["AppSettings:awsS3SecretAccessKey"];
            APIBaseURL = _configuration["AppSettings:APIBaseURL"];
        }

        public int GenerateOTP(int? previousOTP = null)
        {
            int _min = 1000;
            int _max = 9999;

            Random _rdm = new Random();

            int otp = _rdm.Next(_min, _max);

            while (previousOTP != null && previousOTP.Value == otp)
            {
                otp = _rdm.Next(_min, _max);
            }
            
            return otp;
        }

        public void UploadImage(IFormFile item, string fileName)
        {
            using (var client = new AmazonS3Client(awsS3AccessKeyId, awsS3SecretAccessKey, RegionEndpoint.USEast1))
            {
                using (var newMemoryStream = new MemoryStream())
                {
                    item.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = fileName,
                        BucketName = bucketName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var fileTransferUtility = new TransferUtility(client);
                    fileTransferUtility.UploadAsync(uploadRequest).Wait();
                }
            }
        }

        public void SendSMS(string commaSeparatedReceivers, string text)
        {
            string result = "";
            WebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                String to = commaSeparatedReceivers; //Recipient Phone Number multiple number must be separated by comma
                String token = "994fe8b4addba87762f90ff903e44007"; //generate token from the control panel
                String message = System.Uri.EscapeUriString(text); //do not use single quotation (') in the message to avoid forbidden result
                String url = "http://api.greenweb.com.bd/api.php?token=" + token + "&to=" + to + "&message=" + message;
                request = WebRequest.Create(url);

                // Send the 'HttpWebRequest' and wait for response.
                response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding ec = System.Text.Encoding.GetEncoding("utf-8");
                StreamReader reader = new
                System.IO.StreamReader(stream, ec);
                result = reader.ReadToEnd();
                Console.WriteLine(result);
                reader.Close();
                stream.Close();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.ToString());
            }
            finally
            {
                if (response != null) response.Close();
            }
        }

        public async Task<bool> IsFaceDetectedOnImageAsync(string fileName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(APIBaseURL);

                var fileNameContent = new
                {
                    fileName = fileName
                };

                var fileNameContentJson = JsonConvert.SerializeObject(fileNameContent);

                var httpContent = new StringContent(fileNameContentJson, Encoding.UTF8, "application/json");

                var resultContent = await client.PostAsync("IsFaceDetected", httpContent);

                var result = await resultContent.Content.ReadAsStringAsync();

                var boolResult =  Convert.ToInt32(result) == 1? true : false;

                return boolResult;
            }
        }

        public async Task<List<string>> GetMatch(string fileName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(APIBaseURL);

                var fileNameContent = new
                {
                    fileName = fileName
                };

                var fileNameContentJson = JsonConvert.SerializeObject(fileNameContent);

                var httpContent = new StringContent(fileNameContentJson, Encoding.UTF8, "application/json");

                var resultContent = await client.PostAsync("GetMatchingPersonList", httpContent);

                var jsonResult = await resultContent.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<List<string>>(jsonResult);

                return result;
            }
        }

        public string PostNumberBuilder(int number)
        {
            return "#PN-" + number.ToString().PadLeft(5, '0');
        }
    }
}
