using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;

namespace AppRetryPolly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        static readonly HttpClient client = new HttpClient();

        public HomeController()
        {
            
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("RetryHelloWord")]
        public Task<string> RetryHelloWord(string cep)
        {
            var response = ConsultaCep(cep);

            return response.Result.Content.ReadAsStringAsync();
        }

        protected async Task<HttpResponseMessage> ConsultaCep(string cep)
        {
            //HttpResponseMessage response = await client.GetAsync($"https://viacep.com.br/ws/{cep}/json");

            var responseRetry = await Policy.HandleResult<HttpResponseMessage>(message => message.StatusCode == HttpStatusCode.BadRequest)
                .WaitAndRetryAsync(4, i => TimeSpan.FromSeconds(3))
                .ExecuteAsync(() => client.GetAsync($"https://viacep.com.br/ws/{cep}/json"));

            responseRetry.EnsureSuccessStatusCode();
            return responseRetry;
        }
    }
}