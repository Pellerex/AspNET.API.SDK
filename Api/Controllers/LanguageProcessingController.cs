using Business.Common;
using Business.Services.Abstract;
using Business.ViewModels;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecosystem.ML.LanguageProcessingApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    public class LanguageProcessingController : ControllerBase
    {
        private readonly ILanguageProcessingService languageProcessingService;

        public LanguageProcessingController(ILanguageProcessingService languageProcessingService)
        {
            this.languageProcessingService = languageProcessingService;
        }

        [HttpPost("language-processing")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<LanguageProcessingResponeViewModel>> Process(LanguageProcessingRequestViewModel request)
        {
            var (accuracy, tokens, errorCode) = languageProcessingService.Predict(request.Context, request.Question);

            if (!errorCode.IsEmpty())
            {
                switch (errorCode)
                {
                    default:
                        return StatusCode(StatusCodes.Status500InternalServerError, new Error()
                        {
                            Code = LanguageProcessingConstants.ErrorCodes.InternalServerError
                        });
                }
            }

            return Ok(new LanguageProcessingResponeViewModel
            {
                Accuracy = accuracy,
                Tokens = tokens
            });
        }
    }
}