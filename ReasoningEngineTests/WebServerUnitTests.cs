using NUnit.Framework;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using ReasoningEngine.GraphAccess;
using ReasoningEngine.GraphFileHandling;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json; // For CommandRequest deserialization if needed
using Microsoft.AspNetCore.Mvc; // For FromBody attribute if testing controllers

namespace ReasoningEngine.Tests
{
    [TestFixture]
    public class WebServerUnitTests
    {
        private TestServer? testServer;
        private HttpClient? client;
        private Mock<CommandProcessor>? mockCommandProcessor;
        // Need mock mapper to pass to CommandProcessor mock constructor
        private Mock<GraphObjectMapper>? mockGraphObjectMapper;
        private Mock<IGraphStorageProvider>? mockStorageProvider; // Also needed for mapper mock constructor

        [OneTimeSetUp]
        public void Setup()
        {
            // Create mocks for dependencies needed by CommandProcessor constructor
            mockStorageProvider = new Mock<IGraphStorageProvider>();
            mockGraphObjectMapper = new Mock<GraphObjectMapper>(mockStorageProvider.Object);

            // Mock the CommandProcessor directly, passing the mocked mapper to satisfy the constructor.
            // We still mock ProcessCommandAsync directly later.
            mockCommandProcessor = new Mock<CommandProcessor>(MockBehavior.Strict, mockGraphObjectMapper.Object); // Pass mock mapper

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    // Add necessary services for minimal APIs
                    services.AddRouting();
                    // Inject the mocked CommandProcessor
                    services.AddSingleton(mockCommandProcessor.Object);
                })
                .Configure(app =>
                {
                    // Middleware setup (optional for basic unit tests, but good practice)
                    app.UseRouting();

                    // Define endpoints mirroring WebServer.cs using Minimal APIs
                    app.UseEndpoints(endpoints =>
                    {
                        var processor = endpoints.ServiceProvider.GetRequiredService<CommandProcessor>();

                        // Helper to wrap processor call and handle results/errors consistently
                        async Task<IResult> ProcessAndWrap(string command, string payload) {
                            try {
                                // Use the ASYNC method which is preferred
                                var result = await processor.ProcessCommandAsync(command, payload);
                                return Results.Ok(new ApiResponse<string> { Success = true, Data = result });
                            } catch (ArgumentException ex) { // Catch specific exceptions if needed
                                return Results.BadRequest(new ApiResponse<string> { Success = false, Error = ex.Message });
                            } catch (Exception ex) { // General errors
                                // Log the exception server-side (mock doesn't log)
                                // Return the status code and the response object separately
                                return Results.Problem(
                                    detail: $"Internal server error: {ex.Message}",
                                    statusCode: StatusCodes.Status500InternalServerError);
                                    // Alternatively, could use Results.Json with StatusCode:
                                    // return Results.Json(new ApiResponse<string> { Success = false, Error = $"Internal server error: {ex.Message}" },
                                    //     statusCode: StatusCodes.Status500InternalServerError);
                                    // Using Results.Problem is often preferred for standard error responses.
                            }
                        }

                        // Map endpoints similar to WebServer.cs
                        // Health check returns ApiResponse
                        endpoints.MapGet("/api/health", () => Results.Ok(new ApiResponse<string> { Success = true, Data = "OK" }));

                        // Node operations - Handlers directly return Task<IResult> from ProcessAndWrap
                        endpoints.MapGet("/api/nodes/{id}/get", (long id) =>
                            ProcessAndWrap("node_query", id.ToString()));

                        endpoints.MapPost("/api/nodes/create", ([FromBody] CommandRequest request) =>
                            ProcessAndWrap("add_node", request.Payload));

                        endpoints.MapPut("/api/nodes/{id}/update", (long id, [FromBody] CommandRequest request) =>
                            ProcessAndWrap("edit_node", $"{id}|{request.Payload}"));

                        endpoints.MapDelete("/api/nodes/{id}/delete", (long id) =>
                            ProcessAndWrap("delete_node", id.ToString()));

                        // Edge operations - Handlers directly return Task<IResult> from ProcessAndWrap
                        endpoints.MapGet("/api/nodes/{id}/edges/outgoing/list", (long id) =>
                            ProcessAndWrap("outgoing_edge_query", id.ToString()));

                        endpoints.MapGet("/api/nodes/{id}/edges/incoming/list", (long id) =>
                            ProcessAndWrap("incoming_edge_query", id.ToString()));

                        endpoints.MapPost("/api/edges/create", ([FromBody] CommandRequest request) =>
                            ProcessAndWrap("add_edge", request.Payload));

                        endpoints.MapPut("/api/edges/update", ([FromBody] CommandRequest request) =>
                            ProcessAndWrap("edit_edge", request.Payload));

                        endpoints.MapDelete("/api/edges/delete", ([FromBody] CommandRequest request) =>
                            ProcessAndWrap("delete_edge", request.Payload));

                        // Note: Generic /api/commands/{command} endpoint is omitted as specific endpoints are preferred
                        // Note: /api/commands list endpoint is omitted as it doesn't involve CommandProcessor
                    });
                });

            testServer = new TestServer(webHostBuilder);
            client = testServer.CreateClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
            testServer?.Dispose();
        }

        [Test]
        public async Task HealthCheck_ReturnsOK()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");

            var response = await client!.GetAsync("/api/health");
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert: Check the response content (should be ApiResponse<string>)
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.Success, Is.True);
            Assert.That(apiResponse.Data, Is.EqualTo("OK")); // Corrected assertion
        }

        [Test]
        public async Task NodeQuery_ValidInput_ReturnsExpectedResult()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            Assert.That(mockCommandProcessor, Is.Not.Null, "Command processor mock should be initialized");
            mockCommandProcessor.Reset(); // Reset mock before setup

            string command = "node_query";
            string payload = "1";
            string expectedResultData = "Node 1: Test Content";

            // Setup the mock for the ASYNC method
            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, payload))
                .ReturnsAsync(expectedResultData);

            // Act: Call the specific REST endpoint
            var response = await client!.GetAsync($"/api/nodes/{payload}/get"); // Use the specific route
            response.EnsureSuccessStatusCode(); // Throw if not success

            // Assert: Check the response content
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.Success, Is.True);
            Assert.That(apiResponse.Data, Is.EqualTo(expectedResultData));
            Assert.That(apiResponse.Error, Is.Null);

            // Verify the mock was called
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, payload), Times.Once);
        }

        // Remove tests for the old generic endpoint structure
        // [Test]
        // public async Task InvalidCommand_ReturnsBadRequest() { ... }
        // [Test]
        // public async Task EmptyPayload_ReturnsBadRequest() { ... }

        [Test]
        public async Task ExceptionInProcessor_ReturnsInternalServerError_WithApiResponse()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            Assert.That(mockCommandProcessor, Is.Not.Null, "Command processor mock should be initialized");

            string command = "node_query";
            string payload = "1";
            string exceptionMessage = "Test error from processor";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, payload))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act: Call the specific REST endpoint
            var response = await client!.GetAsync($"/api/nodes/{payload}/get");

            // Assert: Check status code and structured error response
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));

            var responseString = await response.Content.ReadAsStringAsync();
            // Assert: Check ProblemDetails structure
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problemDetails.Detail, Does.Contain(exceptionMessage)); // Check if detail contains the original message

            // Verify the mock was called
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, payload), Times.Once);
        }

        // --- Add Tests for other REST endpoints ---

        [Test]
        public async Task CreateNode_ValidRequest_CallsProcessorAndReturnsOK()
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(mockCommandProcessor, Is.Not.Null);

            string command = "add_node";
            string requestPayload = "10|New Node|Role=Variable";
            string expectedResultData = "Node 10 added successfully.";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, requestPayload))
                .ReturnsAsync(expectedResultData);

            var request = new CommandRequest { Payload = requestPayload };
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client!.PostAsync("/api/nodes/create", jsonContent);
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse?.Success, Is.True);
            Assert.That(apiResponse?.Data, Is.EqualTo(expectedResultData));
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, requestPayload), Times.Once);
        }

        [Test]
        public async Task UpdateNode_ValidRequest_CallsProcessorAndReturnsOK()
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(mockCommandProcessor, Is.Not.Null);

            long nodeId = 5;
            string command = "edit_node";
            string requestPayloadContent = "Updated Content|Role=Function";
            string expectedProcessorPayload = $"{nodeId}|{requestPayloadContent}"; // CommandProcessor expects ID prepended
            string expectedResultData = "Node 5 updated successfully.";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, expectedProcessorPayload))
                .ReturnsAsync(expectedResultData);

            var request = new CommandRequest { Payload = requestPayloadContent };
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client!.PutAsync($"/api/nodes/{nodeId}/update", jsonContent);
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse?.Success, Is.True);
            Assert.That(apiResponse?.Data, Is.EqualTo(expectedResultData));
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, expectedProcessorPayload), Times.Once);
        }

        [Test]
        public async Task DeleteNode_ValidRequest_CallsProcessorAndReturnsOK()
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(mockCommandProcessor, Is.Not.Null);

            long nodeId = 7;
            string command = "delete_node";
            string expectedProcessorPayload = nodeId.ToString();
            string expectedResultData = "Node 7 deleted successfully.";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, expectedProcessorPayload))
                .ReturnsAsync(expectedResultData);

            // Act
            var response = await client!.DeleteAsync($"/api/nodes/{nodeId}/delete");
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse?.Success, Is.True);
            Assert.That(apiResponse?.Data, Is.EqualTo(expectedResultData));
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, expectedProcessorPayload), Times.Once);
        }

        [Test]
        public async Task CreateEdge_ValidRequest_CallsProcessorAndReturnsOK()
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(mockCommandProcessor, Is.Not.Null);

            string command = "add_edge";
            string requestPayload = "1|2|0.5|Edge Content";
            string expectedResultData = "Edge from 1 to 2 added successfully.";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, requestPayload))
                .ReturnsAsync(expectedResultData);

            var request = new CommandRequest { Payload = requestPayload };
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client!.PostAsync("/api/edges/create", jsonContent);
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse?.Success, Is.True);
            Assert.That(apiResponse?.Data, Is.EqualTo(expectedResultData));
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, requestPayload), Times.Once);
        }

         [Test]
        public async Task UpdateEdge_ValidRequest_CallsProcessorAndReturnsOK()
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(mockCommandProcessor, Is.Not.Null);

            string command = "edit_edge";
            string requestPayload = "3|4|0.9|Updated Edge Content";
            string expectedResultData = "Edge from 3 to 4 updated successfully.";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, requestPayload))
                .ReturnsAsync(expectedResultData);

            var request = new CommandRequest { Payload = requestPayload };
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client!.PutAsync("/api/edges/update", jsonContent);
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse?.Success, Is.True);
            Assert.That(apiResponse?.Data, Is.EqualTo(expectedResultData));
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, requestPayload), Times.Once);
        }

        [Test]
        public async Task DeleteEdge_ValidRequest_CallsProcessorAndReturnsOK()
        {
            Assert.That(client, Is.Not.Null);
            Assert.That(mockCommandProcessor, Is.Not.Null);

            string command = "delete_edge";
            string requestPayload = "5|6";
            string expectedResultData = "Edge from 5 to 6 deleted successfully.";

            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(command, requestPayload))
                .ReturnsAsync(expectedResultData);

            var request = new CommandRequest { Payload = requestPayload };
            var jsonContent = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");

            // Act
            // Need to use SendAsync for DELETE with body
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/edges/delete")
            {
                Content = jsonContent
            };
            var response = await client!.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.That(apiResponse?.Success, Is.True);
            Assert.That(apiResponse?.Data, Is.EqualTo(expectedResultData));
            mockCommandProcessor.Verify(x => x.ProcessCommandAsync(command, requestPayload), Times.Once);
        }

    }
}
