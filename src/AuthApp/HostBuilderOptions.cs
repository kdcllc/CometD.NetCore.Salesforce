namespace AuthApp
{
    internal class HostBuilderOptions
    {
        /// <summary>
        /// Sets Configuration file besides appsettings.json.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Provides ability to get troubleshooting information.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Ability to use Web project secrets.
        /// </summary>
        public bool UserSecrets { get; set; }

        /// <summary>
        /// Url for the azure key vault i.e. https://{vaultname}.vault.azure.net/
        /// </summary>
        public string AzureVault { get; set; }

        /// <summary>
        /// Prefix is based on Environment i.e. Development = dev, Production = prod.
        /// </summary>
        public bool UseAzureKeyPrefix { get; set; }

        /// <summary>
        /// Pass Hosting environment for the context of the application.
        /// </summary>
        public string HostingEnviroment { get; set; }
    }
}
