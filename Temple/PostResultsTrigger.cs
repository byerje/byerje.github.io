using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RREleven.TempleSurvey
{
    public static class PostResultsTrigger
    {
        [FunctionName("PostResultsTrigger")]
        // public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Processing form submission on POST.");

            // Parse the incoming JSON form data
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // Add null check
            if (string.IsNullOrEmpty(requestBody))
            {
                log.LogError("Request body is null or empty.");
                return new BadRequestObjectResult("Request body cannot be null or empty.");
            }
        
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                log.LogError("Deserialized data is null.");
                return new BadRequestObjectResult("Invalid JSON data.");
            }
           
            // Extract form fields
            string name = data?.name ?? "Unknown";
            string email = data?.email ?? "Unknown";

            // Connection to Azure Table Storage
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            if (string.IsNullOrEmpty(connectionString))
            {
                log.LogError("Azure Table Storage connection string is not set in environment variables.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            string tableName = "resultstable";

            var tableClient = new TableClient(connectionString, tableName);
            tableClient.CreateIfNotExists(); // Ensure the table exists

            // Create a new entity
            var formData = new TableEntity("FormSubmissions", Guid.NewGuid().ToString())
            {
                { "Name", name },
                { "Email", email },
                { "Timestamp", DateTime.UtcNow }
            };

            // Insert entity into Table Storage
            await tableClient.AddEntityAsync(formData);

            log.LogInformation("Form data saved successfully.");
            return new OkObjectResult("Form data saved to Azure Table Storage.");
        }
    }
}
