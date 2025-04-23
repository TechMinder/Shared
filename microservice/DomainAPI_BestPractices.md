# Domain API - Microservice Best Practices Using Clean Architecture

## Introduction
Domain APIs represent the core business capabilities of your system as independent services. They encapsulate specific business domains, following the principles of Domain-Driven Design (DDD) and Clean Architecture to create maintainable, loosely coupled services that can evolve independently.

## Architecture Layers

### 1. Core Layers (Inside)

#### Domain Layer
- **Rich Domain Models**: Entities with behavior, not just data
- **Value Objects**: Immutable objects representing concepts with no identity
- **Domain Events**: Events representing significant occurrences within the domain
- **Aggregates**: Cluster of domain objects treated as a single unit
- **Domain Services**: Operations that don't belong to any specific entity
- **Domain Rules & Invariants**: Business rules that must always be consistent

#### Application Layer
- **Use Cases / Commands & Queries**: Application-specific business rules
- **Domain Event Handlers**: React to domain events
- **Domain-specific DTOs**: Transfer objects focused on domain concepts
- **Application Services**: Orchestration of domain operations
- **Interfaces**: Abstractions for external dependencies

### 2. Outer Layers

#### Infrastructure Layer
- **Repositories**: Persistence implementations for domain objects
- **Event Publishing**: Implementation for domain event publishing
- **External System Integration**: Adapters for external services
- **Message Queue Integration**: Implementations for async messaging
- **Unit of Work**: Transaction management

#### API Layer
- **Domain-centric Controllers**: Endpoints exposing domain operations
- **API Models**: Domain-focused request/response models
- **Domain Event API**: Endpoints for domain event subscription
- **Domain-specific Middleware**: Cross-cutting concerns for domain
- **Domain Documentation**: Swagger/OpenAPI docs for domain concepts

## Best Practices

### Domain Design Principles
1. **Bounded Context**: Clearly define the boundaries of your domain
2. **Ubiquitous Language**: Use consistent terminology throughout the code and with domain experts
3. **Domain Isolation**: Shield domain logic from infrastructure concerns
4. **Domain Events**: Use events to communicate changes within and across domains
5. **Aggregate Roots**: Define clear consistency boundaries and transaction scopes

### Domain API Patterns
1. **Domain-Driven API Design**: Structure APIs around domain concepts, not technical concerns
2. **Resource-based URLs**: Design URLs around domain resources
3. **Command/Query API Separation**: Consider separate endpoints for commands and queries
4. **Event-driven Integration**: Use events for cross-domain communication
5. **Eventual Consistency**: Embrace eventual consistency between bounded contexts

### Domain Modeling
1. **Behavior-rich Models**: Encapsulate business rules within models, not just data
2. **Value Objects**: Use value objects for concepts where identity doesn't matter
3. **Domain Services**: Extract complex operations that span multiple entities
4. **Strong Typing**: Use domain-specific types rather than primitives
5. **Immutability**: Make objects immutable where possible

### Domain Persistence
1. **Repository Abstraction**: Abstract persistence details away from domain
2. **Aggregate Persistence**: Store aggregates as a unit
3. **DDD-friendly ORM Usage**: Configure ORM to respect domain boundaries
4. **Event Sourcing**: Consider event sourcing for complex domains with audit requirements
5. **CQRS for Complex Domains**: Separate read and write models for complex domains

### Domain Integration
1. **Anti-corruption Layer**: Protect domain model from external systems
2. **Domain Events for Integration**: Use events to communicate between domains
3. **Avoid Shared Databases**: Each domain should own its data
4. **Contracts**: Define clear contracts between domains
5. **Context Mapping**: Explicitly map concepts between different bounded contexts

### Domain Testing
1. **Domain Model Unit Tests**: Test business rules in isolation
2. **Use Cases Tests**: Test application use cases with mocked dependencies
3. **Behavior-Driven Testing**: Express tests in domain language
4. **Test Domain Invariants**: Ensure domain constraints cannot be violated
5. **Event Testing**: Test domain event publication and handling

### Domain API Dependency Constraints and Project References
1. **Domain-Centric Dependency Flow**: Dependencies should respect domain boundaries
   - API Layer → Application Layer → Domain Layer
   - Infrastructure Layer → Application Layer
   - Infrastructure Layer must not reference other Domain APIs directly

2. **Project Reference Constraints for Domain APIs**:
   - **Domain Project (Core)**: The heart of the system, must not reference any other project
   - **Application Project (Core)**: Should only reference its own Domain project
   - **Infrastructure Project**: May reference Application and Domain projects from the same domain
   - **API Project**: Should reference only its own Domain, Application, and Infrastructure projects

3. **Cross-Domain Communication Constraints**:
   - Domain APIs should communicate with other domains via well-defined contracts
   - Communication should be through events, messages, or API calls, not direct code references
   - No shared databases between domains
   - No sharing of domain models between different domain contexts
   - Use Anti-Corruption Layers to translate between domains when necessary

4. **Proper Class Placement in Domain APIs**:
   - **Rich Domain Models**: Should reside only in the Domain layer
   - **Domain-specific DTOs**: Should be in the Application layer, not shared across domains
   - **Domain API Models**: Should be in the API layer, no framework dependencies in domain models
   - **Database Mappings**: Should be in Infrastructure layer, not in Domain models
   - **Event Messages**: Defined in Domain layer, serialization details in Infrastructure

5. **Common Domain API Architecture Violations to Avoid**:
   - Sharing domain models across different bounded contexts
   - Using domain entities directly as API response models
   - Adding ORM-specific attributes to domain classes
   - Leaking domain logic into API controllers
   - Allowing other domains to directly access another domain's database
   - Circular references between domains

6. **Domain Boundary Testing**:
   - Implement tests to verify domain boundaries are respected
   - Ensure domain events are properly published and consumed
   - Test that domain logic is isolated in domain layer
   - Verify that domain models aren't exposed directly to clients

### Example Domain API Project Structure with Reference Constraints

```
Solution
├── src
│   ├── ProductDomain.Domain (Core)
│   │   ├── Aggregates
│   │   │   └── Product, Inventory, etc.
│   │   ├── ValueObjects
│   │   │   └── ProductId, Money, etc.
│   │   ├── Events
│   │   │   └── Domain events
│   │   └── Interfaces
│   │       └── Repository interfaces
│   │
│   ├── ProductDomain.Application (Core)
│   │   ├── Commands
│   │   │   └── Create/Update/Delete product commands
│   │   ├── Queries
│   │   │   └── Product query operations
│   │   ├── EventHandlers
│   │   │   └── Domain event handlers
│   │   └── DTOs
│   │       └── Application-specific DTOs
│   │
│   ├── ProductDomain.Infrastructure
│   │   ├── Persistence
│   │   │   └── EF Core configuration for this domain
│   │   ├── ExternalServices
│   │   │   └── External API clients
│   │   └── EventPublishing
│   │       └── Event publishing implementation
│   │
│   └── ProductDomain.API
│       ├── Controllers
│       │   └── Domain-specific endpoints
│       ├── Models
│       │   └── API request/response models
│       └── Filters
│           └── Domain-specific API concerns
│
└── tests
    ├── ProductDomain.UnitTests
    └── ProductDomain.IntegrationTests
```

## Implementation Example (Basic Structure)

```csharp
// Domain Layer
namespace ProductDomain.Domain.Models
{
    public class Product : AggregateRoot
    {
        public ProductId Id { get; private set; }
        public ProductName Name { get; private set; }
        public Money Price { get; private set; }
        public Inventory Inventory { get; private set; }
        
        private Product() { } // For ORM
        
        public Product(ProductId id, ProductName name, Money price)
        {
            Id = id;
            Name = name;
            Price = price;
            Inventory = Inventory.Empty;
            
            AddDomainEvent(new ProductCreatedEvent(id, name));
        }
        
        public void UpdatePrice(Money newPrice)
        {
            if (newPrice.Amount < 0)
                throw new DomainException("Price cannot be negative");
                
            var oldPrice = Price;
            Price = newPrice;
            
            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, newPrice));
        }
        
        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be positive");
                
            Inventory = Inventory.Add(quantity);
            
            AddDomainEvent(new ProductStockAddedEvent(Id, quantity));
        }
        
        public bool CanFulfillOrder(int quantity)
        {
            return Inventory.Available >= quantity;
        }
    }
    
    // Value Object examples
    public class ProductName
    {
        public string Value { get; }
        
        public ProductName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Product name cannot be empty");
                
            if (value.Length > 100)
                throw new DomainException("Product name too long");
                
            Value = value;
        }
    }
    
    public class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }
        
        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
    }
}

// Application Layer
namespace ProductDomain.Application.Commands
{
    public class CreateProductCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
    }
    
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var productId = new ProductId(Guid.NewGuid());
            var productName = new ProductName(request.Name);
            var productPrice = new Money(request.Price, request.Currency);
            
            var product = new Product(productId, productName, productPrice);
            
            _productRepository.Add(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return productId.Value;
        }
    }
}

// API Layer
namespace ProductDomain.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductRequest request)
        {
            var command = new CreateProductCommand
            {
                Name = request.Name,
                Price = request.Price,
                Currency = request.Currency
            };
            
            var productId = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetProduct), new { id = productId }, null);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var query = new GetProductByIdQuery { ProductId = id };
            var product = await _mediator.Send(query);
            
            if (product == null)
                return NotFound();
                
            return Ok(product);
        }
    }
}
```

## Domain Events Example

```csharp
// Domain Events
namespace ProductDomain.Domain.Events
{
    public class ProductCreatedEvent : DomainEvent
    {
        public ProductId ProductId { get; }
        public ProductName ProductName { get; }
        
        public ProductCreatedEvent(ProductId productId, ProductName productName)
        {
            ProductId = productId;
            ProductName = productName;
        }
    }
    
    public class ProductPriceChangedEvent : DomainEvent
    {
        public ProductId ProductId { get; }
        public Money OldPrice { get; }
        public Money NewPrice { get; }
        
        public ProductPriceChangedEvent(ProductId productId, Money oldPrice, Money newPrice)
        {
            ProductId = productId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
        }
    }
}

// Domain Event Handler
namespace ProductDomain.Application.EventHandlers
{
    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<ProductCreatedEventHandler> _logger;
        
        public ProductCreatedEventHandler(ICatalogService catalogService, ILogger<ProductCreatedEventHandler> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }
        
        public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Product created: {ProductId} - {ProductName}", 
                notification.ProductId.Value, notification.ProductName.Value);
                
            await _catalogService.AddProductToCatalogAsync(
                notification.ProductId.Value, 
                notification.ProductName.Value);
        }
    }
}
```

## Deployment and DevOps Considerations
1. **Domain-aligned Deployment**: Deploy services based on domain boundaries
2. **Database per Domain Service**: Each domain service owns its database
3. **Independent Versioning**: Version services independently
4. **Domain-focused Monitoring**: Monitor based on domain concepts and SLAs
5. **Domain-focused Testing**: Integration tests within domain boundaries

## Challenges and Considerations
1. **Domain Boundaries**: Correctly identifying domain boundaries is challenging
2. **Consistency Across Domains**: Managing consistency between domains
3. **Evolving Domains**: Handling domain evolution over time
4. **Domain Expert Collaboration**: Ensuring ongoing collaboration with domain experts
5. **Learning Curve**: DDD concepts have a learning curve for teams

## References
- "Domain-Driven Design" by Eric Evans
- "Implementing Domain-Driven Design" by Vaughn Vernon
- "Clean Architecture" by Robert C. Martin
- "Patterns, Principles, and Practices of Domain-Driven Design" by Scott Millett