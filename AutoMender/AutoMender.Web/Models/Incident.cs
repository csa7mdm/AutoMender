namespace AutoMender.Web.Models
{
    public class Incident
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning"; // 'Crash' or 'Warning'
        public string RootCauseAnalysis { get; set; } = string.Empty;
    }
}
