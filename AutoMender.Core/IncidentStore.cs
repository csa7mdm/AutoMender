using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AutoMender.Core
{
    public class Incident
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string RootCauseAnalysis { get; set; } = string.Empty;
        public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;
    }

    public class IncidentStore
    {
        // Thread-safe collection for singleton usage
        private readonly ConcurrentBag<Incident> _incidents = new ConcurrentBag<Incident>();

        public void AddIncident(Incident incident)
        {
            _incidents.Add(incident);
        }

        public List<Incident> GetIncidents()
        {
            // Return sorted by newest first
            return _incidents.OrderByDescending(i => i.Timestamp).ToList();
        }
    }
}
