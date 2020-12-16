using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Functions.Tests;
using GraphQL.AzureFunctionsProxy.Tests.AzureFunctionsTestFramework;
using GraphQL.AzureFunctionsProxy.Tests.GraphQL;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.AzureFunctionsProxy;
using HotChocolate.Execution;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarWars.AzureFunctions;

namespace GraphQL.AzureFunctionsProxy.Tests
{
    [TestClass]
    public class GraphQLAzureFuncProxyHelloWorldTests : AzureFunctionGraphQLTestBase
    {
        [TestMethod]
        public async Task TestQueryNoSelections()
        {   //arrange
            using var functionTestCtx = AzureFunctionTestFactory.CreateFunctionTestContext<HelloWorldFunction>(new HelloWorldFunctionsStartup());

            var graphQLQuery = "{ hello }";
            var httpRequest = CreateGraphQLQueryRequest(functionTestCtx, graphQLQuery);

            var helloWorldFunction = functionTestCtx.ResolveFunctionWithDependencies<HelloWorldFunction>();

            //act
            var functionResult = await helloWorldFunction.Run(httpRequest, functionTestCtx.Logger, CancellationToken.None);
            var graphQLResult = functionTestCtx.ReadResponseJson<GraphQLQueryResult>();

            //assert
            Assert.IsNotNull(functionResult);
            Assert.IsNotNull(graphQLResult?.Data, "Query Execution Failed");

            var helloResult = graphQLResult.Data["hello"]?.ToString();
            Assert.AreEqual(helloResult, "Hello World!");
            WriteLine(helloResult);
        }

        [TestMethod]
        public async Task TestQueryNoSelectionsWithArgument()
        {   //arrange
            using var functionTestCtx = AzureFunctionTestFactory.CreateFunctionTestContext<HelloWorldFunction>(new HelloWorldFunctionsStartup());

            var graphQLQuery = @"{ hello(name: ""CajunCoding"") }";
            var httpRequest = CreateGraphQLQueryRequest(functionTestCtx, graphQLQuery);

            var helloWorldFunction = functionTestCtx.ResolveFunctionWithDependencies<HelloWorldFunction>();

            //act
            var functionResult = await helloWorldFunction.Run(httpRequest, functionTestCtx.Logger, CancellationToken.None);
            var graphQLResult = functionTestCtx.ReadResponseJson<GraphQLQueryResult>();

            //assert
            Assert.IsNotNull(functionResult);
            Assert.IsNotNull(graphQLResult?.Data, "Query Execution Failed");

            var helloResult = graphQLResult.Data["hello"]?.ToString();
            Assert.AreEqual(helloResult, "Hello CajunCoding!");
            WriteLine(helloResult);
        }

        [TestMethod]
        public async Task TestQueryWithSelections()
        {   //arrange
            using var functionTestCtx = AzureFunctionTestFactory.CreateFunctionTestContext<HelloWorldFunction>(new HelloWorldFunctionsStartup());

            var graphQLQuery = @"{ 
                helloWorld {
                    language
                    translation
                }
            }";

            var httpRequest = CreateGraphQLQueryRequest(functionTestCtx, graphQLQuery);

            var helloWorldFunction = functionTestCtx.ResolveFunctionWithDependencies<HelloWorldFunction>();

            //act
            var functionResult = await helloWorldFunction.Run(httpRequest, functionTestCtx.Logger, CancellationToken.None);
            var graphQLResult = functionTestCtx.ReadResponseJson<GraphQLQueryResult>();

            //assert
            Assert.IsNotNull(functionResult);
            Assert.IsNotNull(graphQLResult?.Data, "Query Execution Failed");

            var json = (JToken)graphQLResult.Data["helloWorld"];
            
            Assert.AreEqual(
                json.FirstOrDefault(t => t["language"]?.ToString() == "English")?["translation"]?.ToString(),
                "Hello World"
            );

            Assert.AreEqual(
                json.FirstOrDefault(t => t["language"]?.ToString() == "Hawaiian")?["translation"]?.ToString(),
                "Aloha Honua"
            );

            WriteLine(json.ToString());
        }
    }
}
