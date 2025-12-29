using Xunit;
using AutoMender.Core;
using System.Linq;
using System;

namespace AutoMender.Tests
{
    public class IncidentStoreTests
    {
        [Fact]
        public void AddIncident_ShouldStoreIncident()
        {
            // Arrange
            var store = new IncidentStore();
            var incident = new Incident
            {
                Id = "1",
                Title = "Test Incident",
                Severity = "Warning",
                Timestamp = DateTime.UtcNow
            };

            // Act
            store.AddIncident(incident);

            // Assert
            var incidents = store.GetIncidents();
            Assert.Single(incidents);
            Assert.Equal("Test Incident", incidents.First().Title);
        }

        [Fact]
        public void GetIncidents_ShouldReturnRecentFirst()
        {
            // Arrange
            var store = new IncidentStore();
            store.AddIncident(new Incident { Id = "1", Timestamp = DateTime.UtcNow.AddMinutes(-5) });
            store.AddIncident(new Incident { Id = "2", Timestamp = DateTime.UtcNow });

            // Act
            var incidents = store.GetIncidents().ToList();

            // Assert
            Assert.Equal("2", incidents[0].Id);
            Assert.Equal("1", incidents[1].Id);
        }
    }
}
