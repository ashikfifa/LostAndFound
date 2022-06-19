using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LostAndFound.Service.Interfaces
{
    public interface ICommonService
    {
        int GenerateOTP(int? previousOTP = null);
        void SendSMS(string commaSeparatedReceivers, string text);
        void UploadImage(IFormFile item, string fileName);
        Task<bool> IsFaceDetectedOnImageAsync(string fileName);
        Task<List<string>> GetMatch(string fileName);
        string PostNumberBuilder(int number);
    }
}
