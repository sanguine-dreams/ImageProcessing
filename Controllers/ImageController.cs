using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ImageProcessing.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private const long MaxFileSizeBytes = 300000; // 

        private static int CompressionQuality = 95;
        
        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            MemoryStream memoryStream = new MemoryStream(); //Create MemoryStream
            await file.CopyToAsync(memoryStream); //Copy the file onto the MemoryStream
            memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position to the beginning

            // Load the image
            using (Image image = Image.Load(memoryStream))
            {
                // Check if resizing is required
                while (memoryStream.Length > MaxFileSizeBytes)
                {
                    // Clear memoryStream to write new image data
                    memoryStream.SetLength(0);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Save the modified image back to the memory stream
                    var jpegEncoder = new JpegEncoder { Quality = CompressionQuality };
                    image.Save(memoryStream, jpegEncoder);

                    // Reset stream position to the beginning to get the updated size
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // If the compressed image size is still larger than MaxFileSizeBytes, reduce quality and compress again
                    if (memoryStream.Length > MaxFileSizeBytes)
                    {
                        CompressionQuality -= 5;
                    }
                }

                // Reset stream position to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Determine the content type based on the file extension
                var contentTypeProvider = new FileExtensionContentTypeProvider();
                if (!contentTypeProvider.TryGetContentType(file.FileName, out var contentType))
                {
                    contentType = "application/octet-stream"; // Default content type
                }

                // Return the modified image
                return new FileStreamResult(memoryStream, contentType);
            }
        }

    }
}
