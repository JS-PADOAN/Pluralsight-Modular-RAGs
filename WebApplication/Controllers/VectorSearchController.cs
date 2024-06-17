using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class VectorSearchController : Controller
    {
        private readonly ILogger<VectorSearchController> _logger;
        private readonly IConfiguration configuration;
        private readonly ILLMService llmservice;

        public VectorSearchController(ILogger<VectorSearchController> logger, IConfiguration configuration, ILLMService llmservice)
        {
            _logger = logger;
            this.configuration = configuration;
            this.llmservice = llmservice;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost()]
        public async Task<IActionResult> DoSearch(string searchPrompt)
        {
            var res = await VectorRepository.DoVectorSearch(searchPrompt, this.configuration);

            // last part of the RAG

            //generate a user friendly message
            var sentence = await this.llmservice.GenerateSummarySentence(searchPrompt, res);

            this.ViewBag.Sentence = sentence;

            return View(res);
        }
    }
}
