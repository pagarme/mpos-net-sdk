using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Text.RegularExpressions;

namespace PagarMe.Bifrost.WebSocket
{
    internal class SnakeCase
    {
        internal static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new SnakeCaseNameResolver()
        };

        private class SnakeCaseNameResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                const String snake = "_";

                var upperCaseRegex = new Regex("[A-Z]");

                propertyName = upperCaseRegex.Replace(propertyName, l => snake + l.Value.ToLower());

                if (propertyName.StartsWith(snake))
                {
                    propertyName = propertyName.Substring(1);
                }

                return base.ResolvePropertyName(propertyName);
            }

        }

    }
}