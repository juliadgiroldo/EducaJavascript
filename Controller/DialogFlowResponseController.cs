using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ChatbotEducacionalApi.UseCases;

namespace ChatbotEducacionalApi.Controllers
{
    [ApiController]
    [Route("api/dialogflow")]
    public class WebhookController : ControllerBase
    {
        private readonly DialogflowUseCase _useCase = new();

        [HttpPost("webhook")]
        public IActionResult ProcessarResposta([FromBody] JsonElement payload)
        {
            var resposta = _useCase.Executar(payload);
            return Ok(resposta);
        }
    }
}