# System API - Microservice Best Practices Using Clean Architecture

## Introduction
A System API in a microservice architecture acts as the foundation for your domain-specific operations. It provides core system functionalities that other services can utilize, following clean architecture principles to ensure maintainable, testable, and scalable code.

## Architecture Layers

### 1. Core Layers (Inside)

#### Domain Layer
- **Entity Models**: Represent your business objects with their properties and behaviors
- **Value Objects**: Immutable objects that represent descriptive aspects of the domain
- **Domain Events**: Important occurrences within your domain
- **Domain Exceptions**: Custom exceptions specific to the domain
- **Interfaces**: Abstractions for infrastructure components

#### Application Layer
- **Use Cases / Commands & Queries (CQRS)**: Implementation of specific business operations
- **DTOs (Data Transfer Objects)**: Objects used for data transportation between layers
- **Interfaces**: Definitions for external dependencies
- **Validators**: Business rule validation
- **Mapping Profiles**: Object-to-object mapping definitions

### 2. Outer Layers

#### Infrastructure Layer
- **Persistence**: Database implementation (Entity Framework Core)
- **External Services**: Communication with other systems
- **Logging**: Logging implementation
- **Authentication/Authorization**: Security implementation
- **Message Brokers**: Implementation for event publishing/subscribing

#### API Layer
- **Controllers**: Handle HTTP requests
- **Middleware**: Cross-cutting concerns
- **Filters**: Action filters for pre/post processing
- **API Models**: Request/response models
- **Configuration**: App settings and service registrations

## Best Practices

### Design Principles
1. **Separation of Concerns**: Each layer has a specific responsibility
2. **Dependency Rule**: Dependencies only point inward, with outer layers depending on inner layers
3. **Dependency Injection**: Use DI to manage dependencies
4. **CQRS Pattern**: Separate read and write operations for better scaling and maintenance
5. **Mediator Pattern**: Use MediatR to decouple request handlers from controllers

### Code Organization
1. **Feature Folders**: Organize code by feature rather than by type
2. **Vertical Slices**: Group related functionality across layers
3. **Naming Conventions**: Consistent and clear naming across the codebase

### API Design
1. **RESTful Principles**: Follow REST conventions for resource management
2. **Consistent Response Format**: Standard format for all responses
3. **Versioning**: Implement API versioning from the start
4. **Documentation**: Use Swagger/OpenAPI for documentation
5. **Health Checks**: Implement comprehensive health checks

### Database Considerations
1. **Repository Pattern**: Abstract data access
2. **Unit of Work**: Manage transactions
3. **Database Migrations**: Plan for database changes
4. **Optimistic Concurrency**: Handle concurrent data modifications

### Logging and Monitoring
1. **Structured Logging**: Use structured logging for better querying
2. **Correlation IDs**: Track requests across services
3. **Performance Metrics**: Monitor key performance indicators
4. **Exception Handling**: Comprehensive error management strategy

### Testing
1. **Unit Tests**: Test business rules in isolation
2. **Integration Tests**: Test interaction between components
3. **Test Data Builders**: Create test data efficiently
4. **Mocking**: Use mocking for external dependencies

### Security
1. **Authentication**: Implement robust authentication
2. **Authorization**: Proper permission checks
3. **Input Validation**: Validate all incoming data
4. **Sensitive Data**: Protect sensitive information
5. **Security Headers**: Implement proper security headers

### Performance
1. **Caching**: Implement appropriate caching strategies
2. **Pagination**: Paginate large result sets
3. **Asynchronous Operations**: Use async/await where appropriate
4. **Resource Optimization**: Optimize resource usage

### Dependency Constraints and Project References
1. **Follow the Dependency Rule**: Dependencies must only point inward
   - API Layer → Application Layer → Domain Layer
   - Infrastructure Layer → Application Layer
   - Infrastructure Layer ↛ Domain Layer (should use interfaces defined in the Domain or Application layer)
   - API Layer ↛ Infrastructure Layer (should use dependency injection)

2. **Project Reference Constraints**:
   - **Domain Project**: Should not reference any other project
   - **Application Project**: Should only reference Domain project
   - **Infrastructure Project**: Should reference Application and Domain projects
   - **API Project**: Should reference Application, Domain, and Infrastructure projects

3. **Prohibited References**:
   - No direct reference from Application to Infrastructure
   - No direct reference from Domain to any layer
   - No circular references between projects

4. **Proper Class Placement**:
   - **API Models** should be defined in the API project, not in Core/Domain
   - **DTOs** belong in the Application layer, not in the Domain layer
   - **Database Entities** should be in the Infrastructure layer, mapped from Domain entities
   - **External Service Contracts** should be in the Infrastructure layer

5. **Common Violations to Avoid**:
   - Placing API models in the Domain/Core project
   - Putting Entity Framework entities in the Domain layer
   - Adding infrastructure-specific logic to Domain classes
   - Referencing Infrastructure services directly in Application handlers
   - Using framework-specific attributes (like [Required]) in Domain models

6. **Enforce with Architecture Tests**:
   - Implement architecture tests to prevent prohibited dependencies
   - Use tools like NDepend or NetArchTest to verify architectural constraints

### Example of Project Structure with Reference Constraints

```
Solution
├── src
│   ├── SystemAPI.Domain (Core)
│   │   └── No external project references
│   │
│   ├── SystemAPI.Application (Core)
│   │   └── References: SystemAPI.Domain only
│   │
│   ├── SystemAPI.Infrastructure
│   │   └── References: SystemAPI.Domain, SystemAPI.Application
│   │
│   └── SystemAPI.API
│       └── References: SystemAPI.Domain, SystemAPI.Application, SystemAPI.Infrastructure
│
└── tests
    ├── SystemAPI.UnitTests
    ├── SystemAPI.IntegrationTests
    └── SystemAPI.ArchitectureTests
```

## Implementation Example (Basic Structure)

```csharp
// Domain Layer
namespace SystemAPI.Domain.Entities
{
    public class SampleEntity
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        
        // Domain behavior methods
        public void UpdateName(string newName)
        {
            // Domain validation
            if (string.IsNullOrEmpty(newName))
                throw new DomainException("Name cannot be empty");
                
            Name = newName;
        }
    }
}

// Application Layer
namespace SystemAPI.Application.Features.SampleFeature.Commands
{
    public class CreateSampleCommand : IRequest<int>
    {
        public string Name { get; set; }
    }
    
    public class CreateSampleCommandHandler : IRequestHandler<CreateSampleCommand, int>
    {
        private readonly IApplicationDbContext _context;
        
        public CreateSampleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<int> Handle(CreateSampleCommand request, CancellationToken cancellationToken)
        {
            var entity = new SampleEntity();
            entity.UpdateName(request.Name);
            
            _context.Samples.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            return entity.Id;
        }
    }
}

// API Layer
namespace SystemAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SamplesController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public SamplesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateSampleCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
```

## Deployment and DevOps Considerations
1. **Containerization**: Use Docker for consistency across environments
2. **Orchestration**: Kubernetes for container orchestration
3. **CI/CD**: Automated pipelines for building, testing, and deployment
4. **Configuration Management**: Externalize configuration
5. **Secret Management**: Secure handling of secrets and credentials

## References
- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- Microsoft .NET Architecture Guides