
namespace VstsRestAPI
{
    public class Configuration : IConfiguration
    {
        public string UriString { get; set; }
        public string UriParams { get; set; }
        public string RequestBody { get; set; }
        public string AccountName { get; set; }
        public string PersonalAccessToken { get; set; }
        public string Project { get; set; }
        public string ProjectId { get; set; }
        public string VersionNumber { get; set; }
        public string Id { get; set; }
        public string Team { get; set; }
        public string GitCredential { get; set; }
        public string GitBaseAddress { get; set; }
        public string MediaType { get; set; }
        public string Scheme { get; set; }
        public string UserName { get; set; }
    }
    public class ProjectConfigurations
    {
        public Configuration AgentQueueConfig { get; set; }
        public Configuration WorkItemConfig { get; set; }
        public Configuration BuildDefinitionConfig { get; set; }
        public Configuration ReleaseDefinitionConfig { get; set; }
        public Configuration RepoConfig { get; set; }
        public Configuration BoardConfig { get; set; }
        public Configuration Config { get; set; }
        public Configuration GetReleaseConfig { get; set; }
        public Configuration ExtensionConfig { get; set; }
        public Configuration EndpointConfig { get; set; }
        public Configuration QueriesConfig { get; set; }
        public Configuration VariableGroupConfig { get; set; }

    }

    public enum TemplateType
    {
        Agile,
        Scrum,
        Cmmi,
        Basic
    }
}
