﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;

namespace GraphQL.AzureFunctionsProxy.Tests.GraphQL
{
    [ExtendObjectType(Name = "Query")]
    public class HelloWorldResolver
    {
        [GraphQLName("hello")]
        public Task<string> GetHelloAsync(string name = "")
        {
            var result = string.IsNullOrWhiteSpace(name)
                ? "Hello World!"
                : $"Hello {name}!";

            return Task.FromResult(result);
        }

        [GraphQLName("helloWorld")]
        public Task<List<HelloWorldTranslation>> GetHelloWorldAsync()
        {
            var results = new List<HelloWorldTranslation>()
            {
                new HelloWorldTranslation("English", "Hello World"),
                new HelloWorldTranslation("Serbian", "Здраво Свете"),
                new HelloWorldTranslation("French", "Bonjour le monde"),
                new HelloWorldTranslation("Spanish", "Hola Mundo"),
                new HelloWorldTranslation("Belarusian", "Прывітанне Сусвет"),
                new HelloWorldTranslation("German", "Hallo Welt"),
                new HelloWorldTranslation("Hawaiian", "Aloha Honua"),
                new HelloWorldTranslation("Japanese", "Kon'nichiwa sekai"),
                new HelloWorldTranslation("Chinese", "Nǐ hǎo, shìjiè")
            };

            return Task.FromResult(results);
        }

        public class HelloWorldTranslation
        {
            public HelloWorldTranslation(string language, string translation)
            {
                Language = language;
                Translation = translation;
            }
            public string Language { get; set; }
            public string Translation { get; set; }
        }
    }
}
