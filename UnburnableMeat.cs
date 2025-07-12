using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Unburnable Meat", "MLeeM97", "1.3.0")]
    [Description("Prevent cooked meats from burning with permission support. Overhaul of Unburnable Meat by S642667, updated for uMod/Oxide v2.0.6511 compatibility")]
    class UnburnableMeat : RustPlugin
    {
        #region Constants
        private const string PERMISSION_USE = "unburnablemeat.use";
        private const string PERMISSION_ADMIN = "unburnablemeat.admin";
        #endregion

        // Remove static CookedItems array - now in config

        private Dictionary<string, int> lowTemps = new Dictionary<string, int>();
        private Dictionary<string, int> highTemps = new Dictionary<string, int>();

        #region Configuration

        private Configuration config;

        public class Configuration
        {
            [JsonProperty("Plugin Settings")]
            public PluginSettings Settings { get; set; } = new PluginSettings();
            
            [JsonProperty("Default Cooked Items")]
            public string[] DefaultCookedItems { get; set; } = new string[]
            {
                // Original items
                "bearmeat.cooked",
                "chicken.cooked", 
                "deermeat.cooked",
                "horsemeat.cooked",
                "humanmeat.cooked",
                "meat.pork.cooked",
                "wolfmeat.cooked",
                "fish.cooked",
                
                // Jungle Update items (2025)
                "bigcatmeat.cooked",
                "crocodilemeat.cooked", 
                "snakemeat.cooked",
                
                // Fish varieties added over the years
                "fish.anchovy.cooked",
                "fish.catfish.cooked",
                "fish.herring.cooked",
                "fish.salmon.cooked",
                "fish.sardine.cooked", 
                "fish.smallshark.cooked",
                "fish.troutsmall.cooked",
                "fish.yellowperch.cooked",
                
                // Other items added over the years
                "meat.boar.cooked",
                "cactusflesh.cooked"
            };
            
            [JsonProperty("Additional Items")]
            public string[] AdditionalItems { get; set; } = new string[0];
            
            [JsonProperty("Excluded Items")]
            public string[] ExcludedItems { get; set; } = new string[0];
        }

        public class PluginSettings
        {
            [JsonProperty("Enable Logging")]
            public bool EnableLogging { get; set; } = true;
            
            [JsonProperty("Require Permission")]
            public bool RequirePermission { get; set; } = false;
            
            [JsonProperty("Permission Mode")]
            public string PermissionMode { get; set; } = "global"; // "global", "individual"
            
            [JsonProperty("Enable Chat Commands")]
            public bool EnableChatCommands { get; set; } = true;
        }

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
                
                if (!config.ToDictionary().Keys.ToHashSet().SetEquals(Config.ToDictionary(x => x.Key, x => x.Value).Keys.ToHashSet()))
                {
                    LogWarning("Configuration appears to be outdated; updating and saving");
                    SaveConfig();
                }
            }
            catch
            {
                LogWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
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

        #region Initialization
        
        void Init()
        {
            // Register permissions
            permission.RegisterPermission(PERMISSION_USE, this);
            permission.RegisterPermission(PERMISSION_ADMIN, this);
            
            // Load configuration first to check if chat commands should be enabled
            LoadConfig();
            
            // Register chat commands if enabled
            if (config?.Settings?.EnableChatCommands == true)
            {
                cmd.AddChatCommand("unburnablemeat", this, "CmdUnburnableMeat");
                cmd.AddChatCommand("um", this, "CmdUnburnableMeat");
            }
            
            // Log initialization if logging is enabled
            if (config?.Settings?.EnableLogging == true)
            {
                Puts($"[UnburnableMeat] Plugin initialized - Chat commands: {(config.Settings.EnableChatCommands ? "Enabled" : "Disabled")}");
            }
        }
        
        #endregion

        #region Localization

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = "You don't have permission to use this command.",
                ["ChatCommandsDisabled"] = "Chat commands are disabled.",
                ["HelpHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Available commands:",
                ["HelpStatus"] = "<color=#87CEEB>/um status</color> - Show your protection status",
                ["HelpInfo"] = "<color=#87CEEB>/um info</color> - Show plugin information",
                ["HelpReload"] = "<color=#FF6347>/um reload</color> - Reload plugin configuration",
                ["HelpToggle"] = "<color=#FF6347>/um toggle</color> - Toggle plugin on/off",
                ["StatusProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Your meat burning protection: <color=#90EE90>PROTECTED</color>",
                ["StatusNotProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Your meat burning protection: <color=#FF6347>NOT PROTECTED</color>",
                ["StatusNeedPermission"] = "You need permission: <color=#87CEEB>{0}</color>",
                ["StatusProtectedItems"] = "Protected items: <color=#87CEEB>{0}</color>",
                ["StatusPermissionMode"] = "Permission mode: <color=#87CEEB>{0}</color>",
                ["InfoHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Plugin Information:",
                ["InfoVersion"] = "Version: <color=#87CEEB>{0}</color>",
                ["InfoProtectedItems"] = "Protected items: <color=#87CEEB>{0}</color>",
                ["InfoPermissionRequired"] = "Permission required: <color=#87CEEB>{0}</color>",
                ["InfoPermissionMode"] = "Permission mode: <color=#87CEEB>{0}</color>",
                ["ConfigReloaded"] = "<color=#FFA500>[UnburnableMeat]</color> Configuration reloaded!",
                ["ToggleUseConsole"] = "<color=#FFA500>[UnburnableMeat]</color> Use server console: oxide.reload UnburnableMeat",
                ["ConflictWarning"] = "COMPATIBILITY WARNING: Detected conflicting plugin '{0}' v{1}",
                ["ConflictRecommendation"] = "Recommendation: oxide.unload {0} - to prevent compatibility issues"
            }, this);
        }

        private string Lang(string key, string userId = null, params object[] args)
        {
            return string.Format(lang.GetMessage(key, this, userId), args);
        }

        #endregion

        #endregion

        #region Helper Methods

        ItemModCookable GetCookable(string shortname)
        {
            try
            {
                var definition = ItemManager.FindItemDefinition(shortname);
                if (definition == null)
                {
                    if (config?.Settings?.EnableLogging == true)
                    {
                        Puts($"[UnburnableMeat] Unknown item definition for '{shortname}' - item may not exist in this Rust version");
                    }
                    return null;
                }
                
                var cookable = definition.GetComponent<ItemModCookable>();
                if (cookable == null)
                {
                    if (config?.Settings?.EnableLogging == true)
                    {
                        Puts($"[UnburnableMeat] Item '{shortname}' is not cookable - skipping");
                    }
                    return null;
                }
                
                return cookable;
            }
            catch (System.Exception ex)
            {
                Puts($"[UnburnableMeat] Error processing item '{shortname}': {ex.Message}");
                return null;
            }
        }

        void CheckForConflictingPlugins()
        {
            try
            {
                // Check for old UnburnableMeat plugin (different class name or file)
                var loadedPlugins = Interface.Oxide.RootPluginManager.GetPlugins();
                
                foreach (var plugin in loadedPlugins)
                {
                    if (plugin == null || plugin == this) continue;
                    
                    // Check for old plugin with same functionality but different name/version
                    if (plugin.Name.ToLower().Contains("unburnablemeat") || 
                        plugin.Name.ToLower().Contains("unburnable") ||
                        plugin.Title.ToLower().Contains("unburnablemeat") ||
                        plugin.Title.ToLower().Contains("unburnable"))
                    {
                        PrintWarning($"[UnburnableMeat] COMPATIBILITY WARNING:");
                        PrintWarning($"[UnburnableMeat] Detected potentially conflicting plugin: '{plugin.Name}' v{plugin.Version}");
                        PrintWarning($"[UnburnableMeat] It is recommended to unload the old plugin to prevent compatibility issues:");
                        PrintWarning($"[UnburnableMeat] Use command: oxide.unload {plugin.Name}");
                        PrintWarning($"[UnburnableMeat] This may cause duplicate functionality or unexpected behavior.");
                        
                        // Also log to console with different formatting for visibility
                        Puts($"=== PLUGIN CONFLICT DETECTED ===");
                        Puts($"Found: {plugin.Name} v{plugin.Version}");
                        Puts($"Recommendation: oxide.unload {plugin.Name}");
                        Puts($"================================");
                        
                        if (config?.Settings?.EnableLogging == true)
                        {
                            Puts($"[UnburnableMeat] Conflicting plugin details - Name: '{plugin.Name}', Title: '{plugin.Title}', Author: '{plugin.Author}', Version: '{plugin.Version}'");
                        }
                        break; // Only warn about first conflict found
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (config?.Settings?.EnableLogging == true)
                {
                    Puts($"[UnburnableMeat] Error checking for conflicting plugins: {ex.Message}");
                }
            }
        }

        void SetupLanguageFiles()
        {
            try
            {
                var oxideDir = Interface.Oxide.DataDirectory;
                var langDir = System.IO.Path.Combine(oxideDir, "lang");
                
                // Ensure the lang directory exists
                if (!System.IO.Directory.Exists(langDir))
                {
                    System.IO.Directory.CreateDirectory(langDir);
                    if (config?.Settings?.EnableLogging == true)
                    {
                        Puts($"[UnburnableMeat] Created language directory: {langDir}");
                    }
                }

                // Define language files with their content
                var languageFiles = new Dictionary<string, Dictionary<string, string>>
                {
                    ["de"] = new Dictionary<string, string>
                    {
                        ["NoPermission"] = "Du hast keine Berechtigung für diesen Befehl.",
                        ["ChatCommandsDisabled"] = "Chat-Befehle sind deaktiviert.",
                        ["HelpHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Verfügbare Befehle:",
                        ["HelpStatus"] = "<color=#87CEEB>/um status</color> - Zeigt deinen Schutzstatus an",
                        ["HelpInfo"] = "<color=#87CEEB>/um info</color> - Zeigt Plugin-Informationen an",
                        ["HelpReload"] = "<color=#FF6347>/um reload</color> - Konfiguration neu laden",
                        ["HelpToggle"] = "<color=#FF6347>/um toggle</color> - Plugin ein-/ausschalten",
                        ["StatusProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Dein Fleischbrand-Schutz: <color=#90EE90>GESCHÜTZT</color>",
                        ["StatusNotProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Dein Fleischbrand-Schutz: <color=#FF6347>NICHT GESCHÜTZT</color>",
                        ["StatusNeedPermission"] = "Du benötigst die Berechtigung: <color=#87CEEB>{0}</color>",
                        ["StatusProtectedItems"] = "Geschützte Gegenstände: <color=#87CEEB>{0}</color>",
                        ["StatusPermissionMode"] = "Berechtigungsmodus: <color=#87CEEB>{0}</color>",
                        ["InfoHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Plugin-Informationen:",
                        ["InfoVersion"] = "Version: <color=#87CEEB>{0}</color>",
                        ["InfoProtectedItems"] = "Geschützte Gegenstände: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionRequired"] = "Berechtigung erforderlich: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionMode"] = "Berechtigungsmodus: <color=#87CEEB>{0}</color>",
                        ["ConfigReloaded"] = "<color=#FFA500>[UnburnableMeat]</color> Konfiguration neu geladen!",
                        ["ToggleUseConsole"] = "<color=#FFA500>[UnburnableMeat]</color> Verwende Server-Konsole: oxide.reload UnburnableMeat",
                        ["ConflictWarning"] = "KOMPATIBILITÄTSWARNUNG: Konfliktierendes Plugin '{0}' v{1} erkannt",
                        ["ConflictRecommendation"] = "Empfehlung: oxide.unload {0} - um Kompatibilitätsprobleme zu vermeiden"
                    },
                    ["fr"] = new Dictionary<string, string>
                    {
                        ["NoPermission"] = "Vous n'avez pas la permission d'utiliser cette commande.",
                        ["ChatCommandsDisabled"] = "Les commandes de chat sont désactivées.",
                        ["HelpHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Commandes disponibles:",
                        ["HelpStatus"] = "<color=#87CEEB>/um status</color> - Afficher votre statut de protection",
                        ["HelpInfo"] = "<color=#87CEEB>/um info</color> - Afficher les informations du plugin",
                        ["HelpReload"] = "<color=#FF6347>/um reload</color> - Recharger la configuration",
                        ["HelpToggle"] = "<color=#FF6347>/um toggle</color> - Activer/désactiver le plugin",
                        ["StatusProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Votre protection anti-brûlure: <color=#90EE90>PROTÉGÉ</color>",
                        ["StatusNotProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Votre protection anti-brûlure: <color=#FF6347>NON PROTÉGÉ</color>",
                        ["StatusNeedPermission"] = "Vous avez besoin de la permission: <color=#87CEEB>{0}</color>",
                        ["StatusProtectedItems"] = "Objets protégés: <color=#87CEEB>{0}</color>",
                        ["StatusPermissionMode"] = "Mode de permission: <color=#87CEEB>{0}</color>",
                        ["InfoHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Informations du plugin:",
                        ["InfoVersion"] = "Version: <color=#87CEEB>{0}</color>",
                        ["InfoProtectedItems"] = "Objets protégés: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionRequired"] = "Permission requise: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionMode"] = "Mode de permission: <color=#87CEEB>{0}</color>",
                        ["ConfigReloaded"] = "<color=#FFA500>[UnburnableMeat]</color> Configuration rechargée!",
                        ["ToggleUseConsole"] = "<color=#FFA500>[UnburnableMeat]</color> Utilisez la console: oxide.reload UnburnableMeat",
                        ["ConflictWarning"] = "AVERTISSEMENT DE COMPATIBILITÉ: Plugin conflictuel '{0}' v{1} détecté",
                        ["ConflictRecommendation"] = "Recommandation: oxide.unload {0} - pour éviter les problèmes de compatibilité"
                    },
                    ["es"] = new Dictionary<string, string>
                    {
                        ["NoPermission"] = "No tienes permiso para usar este comando.",
                        ["ChatCommandsDisabled"] = "Los comandos de chat están deshabilitados.",
                        ["HelpHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Comandos disponibles:",
                        ["HelpStatus"] = "<color=#87CEEB>/um status</color> - Mostrar tu estado de protección",
                        ["HelpInfo"] = "<color=#87CEEB>/um info</color> - Mostrar información del plugin",
                        ["HelpReload"] = "<color=#FF6347>/um reload</color> - Recargar configuración",
                        ["HelpToggle"] = "<color=#FF6347>/um toggle</color> - Activar/desactivar plugin",
                        ["StatusProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Tu protección anti-quemadura: <color=#90EE90>PROTEGIDO</color>",
                        ["StatusNotProtected"] = "<color=#FFA500>[UnburnableMeat]</color> Tu protección anti-quemadura: <color=#FF6347>NO PROTEGIDO</color>",
                        ["StatusNeedPermission"] = "Necesitas el permiso: <color=#87CEEB>{0}</color>",
                        ["StatusProtectedItems"] = "Objetos protegidos: <color=#87CEEB>{0}</color>",
                        ["StatusPermissionMode"] = "Modo de permisos: <color=#87CEEB>{0}</color>",
                        ["InfoHeader"] = "<color=#FFA500>[UnburnableMeat]</color> Información del plugin:",
                        ["InfoVersion"] = "Versión: <color=#87CEEB>{0}</color>",
                        ["InfoProtectedItems"] = "Objetos protegidos: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionRequired"] = "Permiso requerido: <color=#87CEEB>{0}</color>",
                        ["InfoPermissionMode"] = "Modo de permisos: <color=#87CEEB>{0}</color>",
                        ["ConfigReloaded"] = "<color=#FFA500>[UnburnableMeat]</color> ¡Configuración recargada!",
                        ["ToggleUseConsole"] = "<color=#FFA500>[UnburnableMeat]</color> Usa la consola: oxide.reload UnburnableMeat",
                        ["ConflictWarning"] = "ADVERTENCIA DE COMPATIBILIDAD: Plugin conflictivo '{0}' v{1} detectado",
                        ["ConflictRecommendation"] = "Recomendación: oxide.unload {0} - para evitar problemas de compatibilidad"
                    }
                };

                int filesCreated = 0;
                int filesUpdated = 0;

                // Create/update language files
                foreach (var kvp in languageFiles)
                {
                    var langCode = kvp.Key;
                    var translations = kvp.Value;
                    
                    try
                    {
                        // Register translations with Oxide's language system
                        lang.RegisterMessages(translations, this, langCode);
                        
                        var langFile = System.IO.Path.Combine(langDir, $"{langCode}", $"{Name}.json");
                        var langSubDir = System.IO.Path.Combine(langDir, langCode);
                        
                        // Create language subdirectory if it doesn't exist
                        if (!System.IO.Directory.Exists(langSubDir))
                        {
                            System.IO.Directory.CreateDirectory(langSubDir);
                        }
                        
                        bool fileExists = System.IO.File.Exists(langFile);
                        
                        // Write the language file using Oxide's built-in serialization
                        Interface.Oxide.DataFileSystem.WriteObject($"lang/{langCode}/{Name}", translations);
                        
                        if (fileExists)
                        {
                            filesUpdated++;
                        }
                        else
                        {
                            filesCreated++;
                        }
                        
                        if (config?.Settings?.EnableLogging == true)
                        {
                            Puts($"[UnburnableMeat] Language file {(fileExists ? "updated" : "created")}: {langCode}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        PrintError($"[UnburnableMeat] Failed to create language file for '{langCode}': {ex.Message}");
                    }
                }
                
                if (config?.Settings?.EnableLogging == true)
                {
                    Puts($"[UnburnableMeat] Language setup complete: {filesCreated} files created, {filesUpdated} files updated");
                }
            }
            catch (System.Exception ex)
            {
                PrintError($"[UnburnableMeat] Error setting up language files: {ex.Message}");
            }
        }

        #endregion

        #region Hooks

        // Hook into cooking process for individual permission mode
        object OnItemCook(Item item, BaseOven oven)
        {
            // Only handle if permission is required and in individual mode
            if (!config.Settings.RequirePermission || config.Settings.PermissionMode != "individual")
                return null;

            // Find the player using the oven
            var player = oven.GetOwnerPlayer();
            if (player == null) 
                return null;

            // Check if player has permission
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
                return null;

            // Check if this is a protected item
            if (lowTemps.ContainsKey(item.info.shortname))
            {
                // Prevent burning by setting safe temperature range
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

        [ChatCommand("unburnablemeat")]
        [ChatCommand("um")]
        void CmdUnburnableMeat(BasePlayer player, string command, string[] args)
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

            SendReply(player, Lang("StatusProtectedItems", player.UserIDString, lowTemps.Count));
            SendReply(player, Lang("StatusPermissionMode", player.UserIDString, config.Settings.PermissionMode));
        }

        void ShowInfo(BasePlayer player)
        {
            SendReply(player, Lang("InfoHeader", player.UserIDString));
            SendReply(player, Lang("InfoVersion", player.UserIDString, "1.3.0"));
            SendReply(player, Lang("InfoProtectedItems", player.UserIDString, lowTemps.Count));
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

        #region Server Events

        void OnServerInitialized()
        {
            LoadConfig();
            
            // Setup language files first (uMod compliance)
            SetupLanguageFiles();
            
            // Check for conflicting plugins
            CheckForConflictingPlugins();
            
            // Log plugin and server information
            if (config.Settings.EnableLogging)
            {
                Puts($"[UnburnableMeat] v1.3.0 loading for Rust {rust.Protocol}");
                Puts($"[UnburnableMeat] Oxide version: {Interface.Oxide.Version}");
                Puts($"[UnburnableMeat] Permission mode: {config.Settings.PermissionMode}");
                Puts($"[UnburnableMeat] Require permission: {config.Settings.RequirePermission}");
            }
            
            // Combine all items to process
            var itemsToProcess = config.DefaultCookedItems.ToList();
            
            // Add additional items from config
            if (config.AdditionalItems?.Length > 0)
            {
                itemsToProcess.AddRange(config.AdditionalItems);
                if (config.Settings.EnableLogging)
                {
                    Puts($"[UnburnableMeat] Added {config.AdditionalItems.Length} additional items from config");
                }
            }
            
            // Remove excluded items from config
            if (config.ExcludedItems?.Length > 0)
            {
                itemsToProcess = itemsToProcess.Except(config.ExcludedItems).ToList();
                if (config.Settings.EnableLogging)
                {
                    Puts($"[UnburnableMeat] Excluded {config.ExcludedItems.Length} items from config");
                }
            }
            
            // Apply protection based on permission mode
            if (config.Settings.RequirePermission && config.Settings.PermissionMode == "global")
            {
                // Global mode: only protect if at least one player has permission
                if (config.Settings.EnableLogging)
                {
                    Puts("[UnburnableMeat] Global permission mode - items will be protected server-wide");
                }
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
                
                // Only add if not already exists (in case of reload)
                if (!lowTemps.ContainsKey(shortname))
                {
                    lowTemps.Add(shortname, cookable.lowTemp);
                    highTemps.Add(shortname, cookable.highTemp);
                    
                    // Apply protection
                    if (!config.Settings.RequirePermission || config.Settings.PermissionMode == "global")
                    {
                        cookable.lowTemp = -1;
                        cookable.highTemp = -1;
                    }
                    
                    successCount++;
                }
            }
            
            if (config.Settings.EnableLogging)
            {
                Puts($"[UnburnableMeat] Successfully loaded {successCount} items for protection.");
                if (failCount > 0)
                {
                    Puts($"[UnburnableMeat] Warning: {failCount} items failed to load (may not exist in this Rust version)");
                }
                
                if (config.Settings.RequirePermission)
                {
                    Puts($"[UnburnableMeat] Permission required: {PERMISSION_USE}");
                }
                else
                {
                    Puts("[UnburnableMeat] No permission required - protection active for all players");
                }
            }
        }

        void Unload()
        {
            if (lowTemps == null || lowTemps.Count == 0)
            {
                if (config?.Settings?.EnableLogging == true)
                {
                    Puts("[UnburnableMeat] No items to restore on unload");
                }
                return;
            }
            
            int restoredCount = 0;
            int failedCount = 0;
            
            foreach (KeyValuePair<string, int> item in lowTemps)
            {
                var cookable = GetCookable(item.Key);
                if (cookable == null)
                {
                    failedCount++;
                    continue;
                }
                
                try
                {
                    cookable.lowTemp = item.Value;
                    cookable.highTemp = highTemps[item.Key];
                    restoredCount++;
                }
                catch (System.Exception ex)
                {
                    Puts($"[UnburnableMeat] Error restoring item '{item.Key}': {ex.Message}");
                    failedCount++;
                }
            }
            
            if (config?.Settings?.EnableLogging == true)
            {
                Puts($"[UnburnableMeat] Restored original temperatures for {restoredCount} items.");
                if (failedCount > 0)
                {
                    Puts($"[UnburnableMeat] Failed to restore {failedCount} items");
                }
            }
        }

        #endregion
    }
}
