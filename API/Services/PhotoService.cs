using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var Acc = new Account(

                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecert

                );

            _cloudinary = new Cloudinary(Acc);
        }


        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile File)
        {
            var UploadResult = new ImageUploadResult();
            if (File.Length > 0)
            {
                await using var stream = File.OpenReadStream();
                var UploadParams = new ImageUploadParams
                {
                    File = new FileDescription(File.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
                };
                UploadResult = await _cloudinary.UploadAsync(UploadParams);
            }

            return UploadResult;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string PublicId)
        {
            var DeleteParams = new DeletionParams(PublicId);

            var Result = await _cloudinary.DestroyAsync(DeleteParams);

            return Result;
        }
    }
}
