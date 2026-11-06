using AgriStream.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace AgriStream.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        public HomeController(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _httpFactory = httpFactory;
        }

        private string SqlConnectionString => 
            _config.GetConnectionString("AzureSql")
            ?? throw new Exception("AzureSql not configured");

        private string StorageConnectionString =>
            _config.GetConnectionString("AzureStorage")
            ?? throw new Exception("AzureStorage not configured");

        private HttpClient FunctionClient()
        { 
            var client = _httpFactory.CreateClient();
            var key = _config["Functions:Key"];

            if(!string.IsNullOrWhiteSpace(key))
                client.DefaultRequestHeaders.Add("x-functions-key", key);
            return client;  
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                await using var connection = new SqlConnection(SqlConnectionString);
                await connection.OpenAsync();

                ViewBag.SqlStatus = "Connected to Azure SQL Database";
            }
            catch (Exception ex)
            {
                ViewBag.SqlStatus = $"Error connecting to Azure SQL Database: {ex.Message}";
            }

            try
            {
                var url = _config["Functions:BaseUrl"];
                var client = FunctionClient();

                ViewBag.Telemetry = await client.GetStringAsync($"{url}/api/telemetrylatest");
            }
            catch (Exception ex)
            {
                ViewBag.Telemetry = $"Error calling Azure Function: {ex.Message}";
            }
            
            
        }

        
            
    }
}
