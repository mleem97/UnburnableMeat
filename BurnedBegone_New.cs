using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Burned Begone", "MLeeM97", "1.3.0")]
    [Description("Prevent cooked meat from burning with comprehensive permission and configuration support")]
    class BurnedBegone : RustPlugin
    {
        #region Fields and Constants
        
        private const string PERMISSION_USE = "burnedbegone.use";
        private const string PERMISSION_ADMIN = "burnedbegone.admin";
        
        private Dictionary<string, int> originalLowTemps = new Dictionary<string, int>();
        private Dictionary<string, int> originalHighTemps = new Dictionary<string, int>();
        private Configuration config;
        
        #endregion
        
        #region Configuration Classes
        
        private class Configuration
        {
            [JsonProperty("Plugin Settings")]
            public PluginSettings Settings { get; set; } = new PluginSettings();
            
            [JsonProperty("Protected Items")]
            public string[] ProtectedItems { get; set; } = new string[]
            {
                "bearmeat.cooked", "chicken.cooked", "deermeat.cooked", "horsemeat.cooked",
                "humanmeat.cooked", "meat.pork.cooked", "wolfmeat.cooked", "fish.cooked",
                "bigcatmeat.cooked", "crocodilemeat.cooked", "snakemeat.cooked",
                "fish.anchovy.cooked", "fish.catfish.cooked", "fish.herring.cooked",
                "fish.salmon.cooked", "fish.sardine.cooked", "fish.smallshark.cooked",
                "fish.troutsmall.cooked", "fish.yellowperch.cooked", "meat.boar.cooked",
                "cactusflesh.cooked"
            };
            
            [JsonProperty("Additional Items")]
            public string[] AdditionalItems { get; set; } = new string[0];
            
            [JsonProperty("Excluded Items")]
            public string[] ExcludedItems { get; set; } = new string[0];
        }

        private class PluginSettings
        {
            [JsonProperty("Enable Logging")]
            public bool EnableLogging { get; set; } = true;
            
            [JsonProperty("Require Permission")]
            public bool RequirePermission { get; set; } = false;
            
            [JsonProperty("Permission Mode")]
            public string PermissionMode { get; set; } = "global";
            
            [JsonProperty("Enable Chat Commands")]
            public bool EnableChatCommands { get; set; } = true;
            
            [JsonProperty("Auto Create Language Files")]
            public bool AutoCreateLanguageFiles { get; set; } = true;
        }

        #endregion
        
        #region Configuration Management

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    throw new JsonException();
                }
            }
            catch
            {
                LogWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
                SaveConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            config = new Configuration();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        #endregion
        
        #region Plugin Lifecycle
        
        void Init()
        {
            permission.RegisterPermission(PERMISSION_USE, this);
            permission.RegisterPermission(PERMISSION_ADMIN, this);
        }
        
        void OnServerInitialized()
        {
            LoadConfig();
            
            if (config.Settings.AutoCreateLanguageFiles)
            {
                SetupLanguageFiles();
            }
            
            if (config.Settings.EnableChatCommands)
            {
                cmd.AddChatCommand("burnedbegone", this, "CmdBurnedBegone");
                cmd.AddChatCommand("bb", this, "CmdBurnedBegone");
            }
            
            CheckForConflictingPlugins();
            ApplyItemProtection();
            
            if (config.Settings.EnableLogging)
            {
                Puts($"[BurnedBegone] v1.3.0 loaded successfully - {originalLowTemps.Count} items protected");
            }
        }
        
        void Unload()
        {
            RestoreOriginalTemperatures();
        }

        #endregion
        
        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = "You don't have permission to use this command.",
                ["ChatCommandsDisabled"] = "Chat commands are disabled.",
                ["HelpHeader"] = "<color=#FFA500>[BurnedBegone]</color> Available commands:",
                ["HelpStatus"] = "<color=#87CEEB>/bb status</color> - Show your protection status",
                ["HelpInfo"] = "<color=#87CEEB>/bb info</color> - Show plugin information",
                ["HelpReload"] = "<color=#FF6347>/bb reload</color> - Reload plugin configuration",
                ["HelpToggle"] = "<color=#FF6347>/bb toggle</color> - Toggle plugin on/off",
                ["StatusProtected"] = "<color=#FFA500>[BurnedBegone]</color> Your meat burning protection: <color=#90EE90>PROTECTED</color>",
                ["StatusNotProtected"] = "<color=#FFA500>[BurnedBegone]</color> Your meat burning protection: <color=#FF6347>NOT PROTECTED</color>",
                ["StatusNeedPermission"] = "You need permission: <color=#87CEEB>{0}</color>",
                ["StatusProtectedItems"] = "Protected items: <color=#87CEEB>{0}</color>",
                ["StatusPermissionMode"] = "Permission mode: <color=#87CEEB>{0}</color>",
                ["InfoHeader"] = "<color=#FFA500>[BurnedBegone]</color> Plugin Information:",
                ["InfoVersion"] = "Version: <color=#87CEEB>{0}</color>",
                ["InfoProtectedItems"] = "Protected items: <color=#87CEEB>{0}</color>",
                ["InfoPermissionRequired"] = "Permission required: <color=#87CEEB>{0}</color>",
                ["InfoPermissionMode"] = "Permission mode: <color=#87CEEB>{0}</color>",
                ["ConfigReloaded"] = "<color=#FFA500>[BurnedBegone]</color> Configuration reloaded!",
                ["ToggleUseConsole"] = "<color=#FFA500>[BurnedBegone]</color> Use server console: oxide.reload BurnedBegone",
                ["ConflictWarning"] = "COMPATIBILITY WARNING: Detected conflicting plugin '{0}' v{1}",
                ["ConflictRecommendation"] = "Recommendation: oxide.unload {0} - to prevent compatibility issues"
            }, this);
        }

        private string Lang(string key, string userId = null, params object[] args)
        {
            return string.Format(lang.GetMessage(key, this, userId), args);
        }

        #endregion
        
        #region Core Functions

        void ApplyItemProtection()
        {
            var itemsToProcess = config.ProtectedItems.ToList();
            
            if (config.AdditionalItems?.Length > 0)
            {
                itemsToProcess.AddRange(config.AdditionalItems);
            }
            
            if (config.ExcludedItems?.Length > 0)
            {
                itemsToProcess = itemsToProcess.Except(config.ExcludedItems).ToList();
            }
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (var shortname in itemsToProcess.Distinct())
            {
                var cookable = GetCookable(shortname);
                if (cookable == null)
                {
                    failCount++;
                    continue;
                }
                
                if (!originalLowTemps.ContainsKey(shortname))
                {
                    originalLowTemps[shortname] = cookable.lowTemp;
                    originalHighTemps[shortname] = cookable.highTemp;
                    
                    if (!config.Settings.RequirePermission || config.Settings.PermissionMode == "global")
                    {
                        cookable.lowTemp = -1;
                        cookable.highTemp = -1;
                    }
                    
                    successCount++;
                }
            }
            
            if (config.Settings.EnableLogging && failCount > 0)
            {
                Puts($"[BurnedBegone] Warning: {failCount} items failed to load (may not exist in this Rust version)");
            }
        }
        
        void RestoreOriginalTemperatures()
        {
            if (originalLowTemps == null || originalLowTemps.Count == 0)
            {
                return;
            }
            
            int restoredCount = 0;
            
            foreach (var kvp in originalLowTemps)
            {
                var cookable = GetCookable(kvp.Key);
                if (cookable != null)
                {
                    cookable.lowTemp = kvp.Value;
                    cookable.highTemp = originalHighTemps[kvp.Key];
                    restoredCount++;
                }
            }
            
            if (config?.Settings?.EnableLogging == true)
            {
                Puts($"[BurnedBegone] Restored original temperatures for {restoredCount} items");
            }
        }
        
        ItemModCookable GetCookable(string shortname)
        {
            var definition = ItemManager.FindItemDefinition(shortname);
            if (definition == null) return null;
            
            return definition.GetComponent<ItemModCookable>();
        }
        
        void CheckForConflictingPlugins()
        {
            try
            {
                var loadedPlugins = Interface.Oxide.RootPluginManager.GetPlugins();
                
                foreach (var plugin in loadedPlugins)
                {
                    if (plugin == null || plugin == this) continue;
                    
                    if (plugin.Name.ToLower().Contains("unburnablemeat") || 
                        plugin.Name.ToLower().Contains("unburnable") ||
                        plugin.Title.ToLower().Contains("unburnablemeat") ||
                        plugin.Title.ToLower().Contains("unburnable"))
                    {
                        PrintWarning($"[BurnedBegone] COMPATIBILITY WARNING: Detected conflicting plugin '{plugin.Name}' v{plugin.Version}");
                        PrintWarning($"[BurnedBegone] Recommendation: oxide.unload {plugin.Name}");
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (config?.Settings?.EnableLogging == true)
                {
                    Puts($"[BurnedBegone] Error checking for conflicting plugins: {ex.Message}");
                }
            }
        }
        
        void SetupLanguageFiles()
        {
            try
            {
                var languages = new Dictionary<string, Dictionary<string, string>>
                {
                    ["de"] = new Dictionary<string, string>
                    {
                        ["NoPermission"] = "Du hast keine Berechtigung für diesen Befehl.",
                        ["ChatCommandsDisabled"] = "Chat-Befehle sind deaktiviert.",
                        ["HelpHeader"] = "<color=#FFA500>[BurnedBegone]</color> Verfügbare Befehle:",
                        ["HelpStatus"] = "<color=#87CEEB>/bb status</color> - Zeigt deinen Schutzstatus an",
                        ["HelpInfo"] = "<color=#87CEEB>/bb info</color> - Zeigt Plugin-Informationen an",
                        ["HelpReload"] = "<color=#FF6347>/bb reload</color> - Konfiguration neu laden",
                        ["HelpToggle"] = "<color=#FF6347>/bb toggle</color> - Plugin ein-/ausschalten",
                        ["StatusProtected"] = "<color=#FFA500>[BurnedBegone]</color> Dein Fleischbrand-Schutz: <color=#90EE90>GESCHÜTZT</color>",
                        ["StatusNotProtected"] = "<color=#FFA500>[BurnedBegone]</color> Dein Fleischbrand-Schutz: <color=#FF6347>NICHT GESCHÜTZT</color>",
                        ["StatusNeedPermission"] = "Du benötigst die Berechtigung: <color=#87CEEB>{0}</color>",
                        ["StatusProtectedItems"] = "Geschützte Gegenstände: <color=#87CEEB>{0}</color>",
                        ["StatusPermissionMode"] = "Berechtigungsmodus: <color=#87CEEB>{0}</color>",
                        ["InfoHeader"] = "<color=#FFA500>[BurnedBegone]</color> Plugin-Informationen:",
                        ["InfoVersion"] = "Version: <color=#87CEEB>{0}</color>",
                        ["InfoProtectedItems"] = "Geschützte Gegenstände: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionRequired"] = "Berechtigung erforderlich: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionMode"] = "Berechtigungsmodus: <color=#87CEEB>{0}</color>",
                        ["ConfigReloaded"] = "<color=#FFA500>[BurnedBegone]</color> Konfiguration neu geladen!",
                        ["ToggleUseConsole"] = "<color=#FFA500>[BurnedBegone]</color> Verwende Server-Konsole: oxide.reload BurnedBegone"
                    },
                    ["fr"] = new Dictionary<string, string>
                    {
                        ["NoPermission"] = "Vous n'avez pas la permission d'utiliser cette commande.",
                        ["ChatCommandsDisabled"] = "Les commandes de chat sont désactivées.",
                        ["HelpHeader"] = "<color=#FFA500>[BurnedBegone]</color> Commandes disponibles:",
                        ["HelpStatus"] = "<color=#87CEEB>/bb status</color> - Afficher votre statut de protection",
                        ["HelpInfo"] = "<color=#87CEEB>/bb info</color> - Afficher les informations du plugin",
                        ["HelpReload"] = "<color=#FF6347>/bb reload</color> - Recharger la configuration",
                        ["HelpToggle"] = "<color=#FF6347>/bb toggle</color> - Activer/désactiver le plugin",
                        ["StatusProtected"] = "<color=#FFA500>[BurnedBegone]</color> Votre protection anti-brûlure: <color=#90EE90>PROTÉGÉ</color>",
                        ["StatusNotProtected"] = "<color=#FFA500>[BurnedBegone]</color> Votre protection anti-brûlure: <color=#FF6347>NON PROTÉGÉ</color>",
                        ["StatusNeedPermission"] = "Vous avez besoin de la permission: <color=#87CEEB>{0}</color>",
                        ["StatusProtectedItems"] = "Objets protégés: <color=#87CEEB>{0}</color>",
                        ["StatusPermissionMode"] = "Mode de permission: <color=#87CEEB>{0}</color>",
                        ["InfoHeader"] = "<color=#FFA500>[BurnedBegone]</color> Informations du plugin:",
                        ["InfoVersion"] = "Version: <color=#87CEEB>{0}</color>",
                        ["InfoProtectedItems"] = "Objets protégés: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionRequired"] = "Permission requise: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionMode"] = "Mode de permission: <color=#87CEEB>{0}</color>",
                        ["ConfigReloaded"] = "<color=#FFA500>[BurnedBegone]</color> Configuration rechargée!",
                        ["ToggleUseConsole"] = "<color=#FFA500>[BurnedBegone]</color> Utilisez la console: oxide.reload BurnedBegone"
                    }
                };

                foreach (var kvp in languages)
                {
                    lang.RegisterMessages(kvp.Value, this, kvp.Key);
                    Interface.Oxide.DataFileSystem.WriteObject($"lang/{kvp.Key}/{Name}", kvp.Value);
                }
                
                if (config.Settings.EnableLogging)
                {
                    Puts($"[BurnedBegone] Created language files for {languages.Count} languages");
                }
            }
            catch (System.Exception ex)
            {
                PrintError($"[BurnedBegone] Error setting up language files: {ex.Message}");
            }
        }

        #endregion
        
        #region Hooks

        object OnItemCook(Item item, BaseOven oven)
        {
            if (!config.Settings.RequirePermission || config.Settings.PermissionMode != "individual")
                return null;

            var player = oven.GetOwnerPlayer();
            if (player == null || !permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
                return null;

            if (originalLowTemps.ContainsKey(item.info.shortname))
            {
                var cookable = GetCookable(item.info.shortname);
                if (cookable != null)
                {
                    cookable.lowTemp = -1;
                    cookable.highTemp = -1;
                }
            }

            return null;
        }

        #endregion
        
        #region Chat Commands

        [ChatCommand("burnedbegone")]
        [ChatCommand("bb")]
        void CmdBurnedBegone(BasePlayer player, string command, string[] args)
        {
            if (!config.Settings.EnableChatCommands)
            {
                SendReply(player, Lang("ChatCommandsDisabled", player.UserIDString));
                return;
            }

            if (args.Length == 0)
            {
                ShowHelp(player);
                return;
            }

            switch (args[0].ToLower())
            {
                case "status":
                    ShowStatus(player);
                    break;
                case "info":
                    ShowInfo(player);
                    break;
                case "reload":
                    if (!permission.UserHasPermission(player.UserIDString, PERMISSION_ADMIN))
                    {
                        SendReply(player, Lang("NoPermission", player.UserIDString));
                        return;
                    }
                    ReloadPlugin(player);
                    break;
                case "toggle":
                    if (!permission.UserHasPermission(player.UserIDString, PERMISSION_ADMIN))
                    {
                        SendReply(player, Lang("NoPermission", player.UserIDString));
                        return;
                    }
                    TogglePlugin(player);
                    break;
                default:
                    ShowHelp(player);
                    break;
            }
        }

        void ShowHelp(BasePlayer player)
        {
            SendReply(player, Lang("HelpHeader", player.UserIDString));
            SendReply(player, Lang("HelpStatus", player.UserIDString));
            SendReply(player, Lang("HelpInfo", player.UserIDString));
            
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_ADMIN))
            {
                SendReply(player, Lang("HelpReload", player.UserIDString));
                SendReply(player, Lang("HelpToggle", player.UserIDString));
            }
        }

        void ShowStatus(BasePlayer player)
        {
            bool hasPermission = !config.Settings.RequirePermission || 
                               permission.UserHasPermission(player.UserIDString, PERMISSION_USE);

            if (hasPermission)
            {
                SendReply(player, Lang("StatusProtected", player.UserIDString));
            }
            else
            {
                SendReply(player, Lang("StatusNotProtected", player.UserIDString));
                SendReply(player, Lang("StatusNeedPermission", player.UserIDString, PERMISSION_USE));
            }

            SendReply(player, Lang("StatusProtectedItems", player.UserIDString, originalLowTemps.Count));
            SendReply(player, Lang("StatusPermissionMode", player.UserIDString, config.Settings.PermissionMode));
        }

        void ShowInfo(BasePlayer player)
        {
            SendReply(player, Lang("InfoHeader", player.UserIDString));
            SendReply(player, Lang("InfoVersion", player.UserIDString, "1.3.0"));
            SendReply(player, Lang("InfoProtectedItems", player.UserIDString, originalLowTemps.Count));
            SendReply(player, Lang("InfoPermissionRequired", player.UserIDString, config.Settings.RequirePermission));
            SendReply(player, Lang("InfoPermissionMode", player.UserIDString, config.Settings.PermissionMode));
        }

        void ReloadPlugin(BasePlayer player)
        {
            LoadConfig();
            SendReply(player, Lang("ConfigReloaded", player.UserIDString));
        }

        void TogglePlugin(BasePlayer player)
        {
            SendReply(player, Lang("ToggleUseConsole", player.UserIDString));
        }

        #endregion
    }
}
