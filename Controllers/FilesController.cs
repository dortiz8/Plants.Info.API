using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Plants.info.API.Controllers
{
    [Route("api/files")]
    [Authorize]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public FilesController(IConfiguration config, FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _config = config;
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ?? throw new System.ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }
        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            // FileContentResult
            // FileStreamResult
            var pathToFile = _config["Paths:SampleFile"]; 
            if (!System.IO.File.Exists(pathToFile))
            {
                return NotFound();  
            }
            // Try to get the correct content type of the file

            if(!_fileExtensionContentTypeProvider.TryGetContentType(pathToFile, out var contentType))
            {
                contentType = "application/octet-stream"; // Acts as a catch all for all content types 
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile); // Creates a bytes array containing the content of the file. 
            return File(bytes, contentType, Path.GetFileName(pathToFile));  // Defined method on the ControllerBase class and acts as wrapper around all other subclasses
        }
    }
}
