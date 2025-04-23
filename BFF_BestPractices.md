# Backend For Frontend (BFF) - Microservice Best Practices Using Clean Architecture

## Introduction
The Backend For Frontend (BFF) pattern is an architectural pattern where a dedicated backend service is created for each frontend application or interface. This approach tailors the API to the specific needs of each client, optimizing performance and user experience while following clean architecture principles.

## Architecture Layers for BFF

### 1. Core Layers (Inside)

#### Application Layer
- **Client-specific Use Cases**: Operations specific to frontend needs
- **DTOs**: Data transfer objects designed to match frontend requirements
- **Aggregation Services**: Services that combine data from multiple system/domain APIs
- **Client-specific Validation**: Validation rules specific to this client
- **Mapping Profiles**: Object mappings tailored to the client's data model

### 2. Outer Layers

#### Infrastructure Layer
- **API Clients**: HTTP clients to communicate with system APIs and domain APIs
- **Caching**: Client-specific caching strategies
- **Resilience & Circuit Breakers**: Patterns to handle dependency failures
- **Authentication/Authorization**: Client-specific auth handling

#### API Layer
- **Controllers**: Endpoints optimized for specific client needs
- **Request/Response Models**: Models designed for the specific client
- **API Versioning**: Client-specific versioning
- **Documentation**: Client-targeted API documentation
- **CORS Configuration**: Cross-origin resource sharing specific to client needs

## Best Practices

### BFF Design Principles
1. **Client Ownership**: Each BFF should be dedicated to a specific frontend/client
2. **API Aggregation**: Combine multiple backend API calls into a single client-optimized response
3. **Thin Layer**: Keep the BFF as thin as possible, focus on orchestration not business logic
4. **Independent Deployment**: Each BFF should be deployable independently
5. **Client-specific Concerns**: Handle client-specific authentication, serialization, etc.

### Pattern Application
1. **One BFF per Client Type**: Separate BFFs for web, mobile, and third-party clients
2. **Avoid Cross-BFF Communication**: BFFs should communicate with APIs, not with each other
3. **Don't Share Code Between BFFs**: Resist the temptation to create shared libraries
4. **Keep Domain Logic in System/Domain APIs**: BFFs should not contain business logic

### API Design
1. **Client-optimized Endpoints**: Design endpoints around client needs, not server models
2. **Tailored Responses**: Only return data needed by the client
3. **Frontend-friendly Field Names**: Use naming conventions that make sense to the frontend
4. **Batch Operations**: Support batch operations to reduce round trips
5. **Real-time Support**: Add WebSocket or SignalR support if needed by the client

### Performance Optimization
1. **Response Shaping**: Allow clients to specify which fields they need
2. **Client-side Caching Controls**: Implement cache headers optimized for the client
3. **Request Collapsing**: Combine similar concurrent requests
4. **Intelligent Loading**: Implement progressive and lazy loading patterns
5. **Client-specific Compression**: Optimize payload compression for client capabilities

### Security Considerations
1. **Client-specific Authentication**: Implement authentication methods appropriate for the client
2. **Token Translation**: Convert between authentication mechanisms as needed
3. **Rate Limiting**: Implement client-appropriate rate limits
4. **Client-specific Data Filtering**: Only expose data relevant to client

### Error Handling
1. **Client-friendly Error Messages**: Convert technical errors to user-friendly formats
2. **Consistent Error Format**: Use consistent error structure across all endpoints
3. **Appropriate Status Codes**: Use HTTP status codes correctly
4. **Problem Details Format**: Follow RFC 7807 for error responses

### BFF Dependency Constraints and Project References
1. **Client-Focused Dependency Flow**: Dependencies should be organized to serve the client's needs
   - API Layer → Application Layer → Domain Layer (minimal or none for BFFs)
   - Client-specific concerns should not leak into lower layers

2. **Project Reference Constraints for BFFs**:
   - **Minimal Domain Logic**: BFFs should have minimal (if any) domain logic
   - **Application Project**: Should contain client-specific aggregation and transformation logic
   - **Infrastructure Project**: Should reference necessary downstream API clients and services
   - **API Project**: Should only expose endpoints relevant to specific client needs

3. **BFF-Specific Reference Rules**:
   - BFFs should not reference other BFFs
   - BFFs should not share databases with domain services
   - API layer should not directly reference System/Domain API clients

4. **Proper Class Placement in BFFs**:
   - **Client View Models**: Should be in the API project, tailored to client needs
   - **Client DTOs**: Should be in the Application layer, designed for client consumption
   - **API Client Interfaces**: Should be defined in Application layer
   - **API Client Implementations**: Should be in Infrastructure layer
   - **System/Domain API Models**: Should be isolated in Infrastructure layer

5. **Common BFF Architecture Violations to Avoid**:
   - Including domain logic in BFF services
   - Sharing code between different client BFFs
   - Directly accessing databases instead of calling APIs
   - Exposing backend service DTOs directly to clients
   - Coupling to specific downstream API implementations

6. **BFF Architecture Testing**:
   - Test that BFF only communicates with authorized System/Domain APIs
   - Verify no direct database access is occurring
   - Ensure proper separation between client models and API models

### Example BFF Project Structure with Reference Constraints

```
Solution
├── src
│   ├── BFF.Application
│   │   ├── Interfaces
│   │   │   └── IProductApiClient.cs, IOrderApiClient.cs, etc.
│   │   ├── DTOs
│   │   │   └── Client-specific DTOs
│   │   └── Services
│   │       └── Client-specific aggregation services
│   │
│   ├── BFF.Infrastructure
│   │   ├── ApiClients
│   │   │   └── ProductApiClient.cs, OrderApiClient.cs, etc.
│   │   └── Cache
│   │       └── Client-specific caching
│   │
│   └── BFF.API
│       ├── Controllers
│       │   └── Client-optimized endpoints 
│       ├── Models
│       │   └── Client-specific request/response models
│       └── Middleware
│           └── Client-specific concerns (mobile headers, etc.)
│
└── tests
    ├── BFF.UnitTests
    └── BFF.IntegrationTests
```

## Implementation Example (Basic Structure)

```csharp
// Application Layer
namespace BFF.Application.Features.Dashboard
{
    public class GetDashboardDataQuery : IRequest<DashboardDto>
    {
        public string UserId { get; set; }
    }
    
    public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, DashboardDto>
    {
        private readonly IUserApiClient _userApiClient;
        private readonly IOrderApiClient _orderApiClient;
        private readonly IProductApiClient _productApiClient;
        
        public GetDashboardDataQueryHandler(
            IUserApiClient userApiClient,
            IOrderApiClient orderApiClient,
            IProductApiClient productApiClient)
        {
            _userApiClient = userApiClient;
            _orderApiClient = orderApiClient;
            _productApiClient = productApiClient;
        }
        
        public async Task<DashboardDto> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
        {
            // Execute parallel requests to different APIs
            var userTask = _userApiClient.GetUserProfileAsync(request.UserId);
            var ordersTask = _orderApiClient.GetRecentOrdersAsync(request.UserId, limit: 5);
            var recommendationsTask = _productApiClient.GetRecommendationsAsync(request.UserId);
            
            await Task.WhenAll(userTask, ordersTask, recommendationsTask);
            
            // Aggregate the results into a single client-optimized response
            return new DashboardDto
            {
                UserProfile = _mapper.Map<UserProfileDto>(userTask.Result),
                RecentOrders = _mapper.Map<List<OrderSummaryDto>>(ordersTask.Result),
                Recommendations = _mapper.Map<List<ProductSummaryDto>>(recommendationsTask.Result)
            };
        }
    }
}

// API Layer
namespace BFF.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<DashboardViewModel>> GetDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            var result = await _mediator.Send(new GetDashboardDataQuery { UserId = userId });
            
            return Ok(result);
        }
    }
}
```

## API Client Example

```csharp
namespace BFF.Infrastructure.ApiClients
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderApiClient> _logger;
        
        public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        
        public async Task<List<OrderDto>> GetRecentOrdersAsync(string userId, int limit)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/users/{userId}/orders?limit={limit}");
                
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<List<OrderDto>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent orders for user {UserId}", userId);
                throw;
            }
        }
    }
}
```

## Deployment and DevOps Considerations
1. **Edge Deployment**: Deploy BFFs close to clients when possible
2. **Client-aligned Scaling**: Scale each BFF based on its specific client load
3. **Client-specific Monitoring**: Monitor from the client perspective
4. **Canary Releases**: Use canary deployments for safer client-side updates
5. **API Gateway Integration**: Consider using API gateways for common cross-cutting concerns

## Challenges and Considerations
1. **Duplication**: Be aware of potential code duplication across BFFs
2. **Operational Overhead**: More services to maintain and deploy
3. **Service Discovery**: Ensure BFFs can locate and communicate with needed services
4. **Cross-team Coordination**: Clear ownership and communication between client and BFF teams

## References
- Sam Newman's "Building Microservices"
- "Microservices Patterns" by Chris Richardson
- Microsoft's .NET Microservices Architecture Guidance