using Microsoft.AspNetCore.Http;

namespace KxnPhotoStudio.Services
{
    public static class ImageUploadValidator
    {
        private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public static async Task<string?> ValidateAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "Please select an image.";
            }

            if (file.Length > MaxFileSize)
            {
                return "The image must be 20 MB or smaller.";
            }

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return "Only JPG, JPEG, PNG, and WEBP image files are allowed.";
            }

            if (!await HasValidImageSignatureAsync(file, extension))
            {
                return "The selected file is not a valid image.";
            }

            return null;
        }

        private static async Task<bool> HasValidImageSignatureAsync(
            IFormFile file,
            string extension)
        {
            var header = new byte[12];

            await using var stream = file.OpenReadStream();

            var bytesRead = await stream.ReadAsync(
                header.AsMemory(0, header.Length));

            if (bytesRead < 4)
            {
                return false;
            }

            // JPEG: FF D8 FF
            if (extension is ".jpg" or ".jpeg")
            {
                return header[0] == 0xFF &&
                       header[1] == 0xD8 &&
                       header[2] == 0xFF;
            }

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (extension == ".png")
            {
                return bytesRead >= 8 &&
                       header[0] == 0x89 &&
                       header[1] == 0x50 &&
                       header[2] == 0x4E &&
                       header[3] == 0x47 &&
                       header[4] == 0x0D &&
                       header[5] == 0x0A &&
                       header[6] == 0x1A &&
                       header[7] == 0x0A;
            }

            // WEBP: RIFF....WEBP
            if (extension == ".webp")
            {
                return bytesRead >= 12 &&
                       header[0] == (byte)'R' &&
                       header[1] == (byte)'I' &&
                       header[2] == (byte)'F' &&
                       header[3] == (byte)'F' &&
                       header[8] == (byte)'W' &&
                       header[9] == (byte)'E' &&
                       header[10] == (byte)'B' &&
                       header[11] == (byte)'P';
            }

            return false;
        }
    }
}