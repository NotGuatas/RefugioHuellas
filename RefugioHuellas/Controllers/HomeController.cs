using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RefugioHuellas.Models;
using RefugioHuellas.Services;

namespace RefugioHuellas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EscaladaECService _escaladaService;

        public HomeController(ILogger<HomeController> logger, EscaladaECService escaladaService)
        {
            _logger = logger;
            _escaladaService = escaladaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SincronizarConEscalada()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var datos = new
            {
                origen = "RefugioHuellas",
                mensaje = "Sincronización de datos",
                timestamp = DateTime.UtcNow
            };

            var respuesta = await _escaladaService.EnviarDatosCifradosAsync(datos, accessToken!);
            ViewBag.Respuesta = respuesta;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
    