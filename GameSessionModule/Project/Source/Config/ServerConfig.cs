namespace RedGaint.Network.GameSessionModule
{
    public static class ServerConfig
    {
        public const string ProjectId = "52b8288e-8da7-4625-a2a3-32a577389bd1";
        public const string EnvironmentId = "aacaf31c-924c-4dee-b713-e99e306445b9";

        public const int BuildConfigId = 1293114;
        public const string RegionId = "f1697338-ae9d-4f27-b6b6-22c6e4458ae1";

        public const string ClientId = "844fe6c8-3c8a-4e78-b244-2858e34c1985";
        public const string ClientSecret = "SQmJFTv_tmhz9w4Yq4ikzMjeknOPhpKp";
        public static string FleetId = "20c760c6-0ff7-445a-b4bd-d9dc18ff6ff1";
        public static object BuildConfigurationId = "1293114";

        public static readonly string TokenExchangeUrl =
            $"https://services.api.unity.com/auth/v1/token-exchange?projectId={ProjectId}&environmentId={EnvironmentId}";

        public static readonly string ServerListUrl =
            $"https://services.api.unity.com/multiplay/servers/v1/projects/{ProjectId}/environments/{EnvironmentId}/servers";

        public const string multiplayAllocUrl = "https://services.unity.com/multiplay/allocations";
        
        public static string allocationUrl =>
            $"https://multiplay.services.api.unity.com/v1/allocations/projects/{ProjectId}/environments/{EnvironmentId}/fleets/{FleetId}/allocations";
        
        // public static string AllocationDetailsUrl =>
        //     $"https://multiplay.services.api.unity.com/v1/allocations/projects/{ProjectId}/environments/{EnvironmentId}/fleets/{FleetId}/allocations/{allocationId}";


    }
}