﻿using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Micronetes
{
    internal class StackExchangeRedisClientFactory : IClientFactory<ConnectionMultiplexer>, IDisposable
    {
        private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _clients = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        private readonly IConfiguration _configuration;

        public StackExchangeRedisClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConnectionMultiplexer CreateClient(string name)
        {
            // REVIEW: Settings options configuration from where?

            var binding = _configuration.GetBinding(name);

            if (!string.Equals("redis", binding.Protocol, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported protocol {binding.Protocol}");
            }

            return _clients.GetOrAdd(name, n =>
            {
                var uri = new Uri(binding.Address);
                // REVIEW: What about async? Do we make sure that clients all have a an explicit Connect
                return ConnectionMultiplexer.Connect(uri.Host + ":" + uri.Port);
            });
        }

        public void Dispose()
        {
            foreach (var client in _clients)
            {
                client.Value.Dispose();
            }
        }
    }
}
