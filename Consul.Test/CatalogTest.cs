﻿// -----------------------------------------------------------------------
//  <copyright file="CatalogTest.cs" company="PlayFab Inc">
//    Copyright 2015 PlayFab Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//  </copyright>
// -----------------------------------------------------------------------

using Xunit;

namespace Consul.Test
{
    public class CatalogTest
    {
        [Fact]
        public void Catalog_Datacenters()
        {
            var client = new ConsulClient();
            var datacenterList = client.Catalog.Datacenters();

            Assert.NotEqual(0, datacenterList.Response.Length);
        }

        [Fact]
        public void Catalog_Nodes()
        {
            var client = new ConsulClient();
            var nodeList = client.Catalog.Nodes();


            Assert.NotEqual((ulong)0, nodeList.LastIndex);
            Assert.NotEqual(0, nodeList.Response.Length);
            // make sure deserialization is working right
            Assert.NotNull(nodeList.Response[0].Address);
            Assert.NotNull(nodeList.Response[0].Name);
        }

        [Fact]
        public void Catalog_Services()
        {
            var client = new ConsulClient();
            var servicesList = client.Catalog.Services();


            Assert.NotEqual((ulong)0, servicesList.LastIndex);
            Assert.NotEqual(0, servicesList.Response.Count);
        }

        [Fact]
        public void Catalog_Service()
        {
            var client = new ConsulClient();
            var serviceList = client.Catalog.Service("consul");

            Assert.NotEqual((ulong)0, serviceList.LastIndex);
            Assert.NotEqual(0, serviceList.Response.Length);
        }

        [Fact]
        public void Catalog_Node()
        {
            var client = new ConsulClient();

            var node = client.Catalog.Node(client.Agent.NodeName);

            Assert.NotEqual((ulong)0, node.LastIndex);
            Assert.NotNull(node.Response.Services);
        }

        [Fact]
        public void Catalog_RegistrationDeregistration()
        {
            var client = new ConsulClient();
            var service = new AgentService()
            {
                ID = "redis1",
                Service = "redis",
                Tags = new[] { "master", "v1" },
                Port = 8000
            };

            var check = new AgentCheck()
            {
                Node = "foobar",
                CheckID = "service:redis1",
                Name = "Redis health check",
                Notes = "Script based health check",
                Status = CheckStatus.Passing,
                ServiceID = "redis1"
            };

            var registration = new CatalogRegistration()
            {
                Datacenter = "dc1",
                Node = "foobar",
                Address = "192.168.10.10",
                Service = service,
                Check = check
            };

            client.Catalog.Register(registration);

            var node = client.Catalog.Node("foobar");
            Assert.True(node.Response.Services.ContainsKey("redis1"));

            var health = client.Health.Node("foobar");
            Assert.Equal("service:redis1", health.Response[0].CheckID);

            var dereg = new CatalogDeregistration()
            {
                Datacenter = "dc1",
                Node = "foobar",
                Address = "192.168.10.10",
                CheckID = "service:redis1"
            };

            client.Catalog.Deregister(dereg);

            health = client.Health.Node("foobar");
            Assert.Equal(0, health.Response.Length);

            dereg = new CatalogDeregistration()
            {
                Datacenter = "dc1",
                Node = "foobar",
                Address = "192.168.10.10"
            };

            client.Catalog.Deregister(dereg);

            node = client.Catalog.Node("foobar");
            Assert.Null(node.Response);
        }
    }
}