# (Unofficial) HotChocolate.AzureFunctionsProxy for v11

## Overview
This is a extension package for HotChocolate GraphQL framework to enable execution
within AzureFunctions using a simple Proxy so that all original functionality of functions
endpoints are unaffected.

This is **Unofficial** but working for most common use cases.

This also includes a working example of the StarWars Project running as an Azure Function
and modified only as needed to run as expected (with v11 API)!

### Nuget Package (.netcoreapp3.0)
To use this as-is in your project, add the [HotChocolate.AzureFunctionsProxy](https://www.nuget.org/packages/HotChocolate.AzureFunctionsProxy) NuGet package to your project.
 and wire up your Starup and AzureFunction endpoint as outlined below...


## Demo Site (Star Wars)
This project contains a clone of the HotChocolate GraphQL *Star Wars* example project (Pure Code First version)
running as an AzureFunctions app and mildly updated to use the new v11 API. 

HotChocolate has changed the Execution pipeline for v11 API in many ways, and existing AzureFunctions
implementation samples don't account for various common use cases like BatchRequests, etc.

### NOTES: 
1. **NOTE:** According to the HotChocolate team on Slack, they will provide an Official AzureFunctions 
middleware as part of v11. :-)
2. **WARNING: Very Limited Testing has been done on this but I am actively using it on projects, 
and will update with any findings.**

## Goals

* To provide a working approach to using the new **v11 API** until an official Middleware
is provided. 
* Keep this code fully encapsulated so that switching to the official Middleware will be as
painless and simple as possible *(with a few design assumptions aside)*.
* Keep this adaptation layer as lean and DRY as possible while also supporting as much OOTB
functionality as possible.
* Ensures that the Azure Functions paradigm and flexibility are not lost, so that all OOTB 
C# bindings, DI, and current Function invokation are maintained.

## Implementation:
This approach uses a "Middleware Proxy" pattern whereby we provide the functionality of the 
middleware via a proxy class that can be injected into the Azure Function,
but otherwise do not change the existing AzureFunctions invocation pipeline.

This Proxy exposes an "executor" interface that can process the HttpContext in an AzureFunction.
However, any pre/post logic could be added before/after the invocation of the executor proxy 
*IGraphQLAzureFunctionsExecutorProxy*.

This proxy is setup by internally configuring a Middleware Proxy that is an encapsulation of the 
existing *HttpPostMiddleware* & *HttpGetMiddleware* configred as a simple pipeline processing POST 
requests first and then defaulting back to GET requests, and erroring out if neither are able to 
handle the request.

## Key Elements:

### Startup Configuration
1. The following Middleware initializer must be added into a valid AzureFunctions Configuration Startup.cs
  * All other elements of HotChocolate initialization are the same using the v11 API. 
```csharp
        //Finally Initialize AzureFunctions Executor Proxy here...
        services.AddAzureFunctionsGraphQL();
```

  * Note: The namespace for this new middleware and proxy classes as needed is:
```csharp
using HotChocolate.AzureFunctionsProxy
```

2. Dependency Inject the new **IGraphQLAzureFunctionsExecutorProxy** into the Function Endpoint:
```csharp
using HotChocolate.AzureFunctionsProxy
using....

public class StarWarsFunctionEndpoint
{
    private readonly IGraphQLAzureFunctionsExecutorProxy _graphqlExecutorProxy;

    public StarWarsFunctionEndpoint(IGraphQLAzureFunctionsExecutorProxy graphqlExecutorProxy)
    {
        _graphqlExecutorProxy = graphqlExecutorProxy;
    }
```

3. Finally, the **IGraphQLAzureFunctionsExecutorProxy** can be invoked in the AzureFunction invocation:
```csharp
        [FunctionName(nameof(StarWarsFunctionEndpoint))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "graphql")] HttpRequest req,
            ILogger logger,
            CancellationToken cancellationToken
        )
        {
            logger.LogInformation("C# GraphQL Request processing via Serverless AzureFunctions...");

            return await _graphqlExecutorProxy.ExecuteFunctionsQueryAsync(
                req.HttpContext,
                logger,
                cancellationToken
            );
        }
```

## Disclaimers:
* Subscriptsion were disabled in the example project due to unknown supportability in a 
serverless environment. 
  * The StarWars example uses in-memory subscriptions which are incongruent with the serverless
paradigm of AzureFunctions.

## Credits:
* Initial thoughts around design were adapted from [*OneCyrus'* Repo located here](https://github.com/OneCyrus/GraphQL-AzureFunctions-HotChocolate).
  * *OneCyrus'* example is designed around HotChocolate **v10 API** and the execution & configuration pipelines
    have changed significantly in the new **v11 API**.
  * This example also did not support BatchRequests, Extension values, etc... 
  * Therefore, rather than manually processing request as this prior example did, this approach 
    is different and leverages
alot more OOTB code from **HotChocolate.AspNetCore**
* The [HotChocolage Slack channel](https://join.slack.com/t/hotchocolategraphql/shared_invite/enQtNTA4NjA0ODYwOTQ0LTViMzA2MTM4OWYwYjIxYzViYmM0YmZhYjdiNzBjOTg2ZmU1YmMwNDZiYjUyZWZlMzNiMTk1OWUxNWZhMzQwY2Q)
was helpful for searching and getting some feedback to iron this out quickly.
