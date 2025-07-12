# Burned Begone

A modern Rust Oxide/uMod plugin that prevents cooked meat and fish from burning in ovens, with comprehensive permission and configuration support.

## Features

- **Prevents burning** of all cooked meat and fish items
- **Permission-based protection** with global or individual modes  
- **Configurable item lists** with easy addition/exclusion of items
- **Automatic language file creation** - No manual setup required
- **Chat commands** for status checking and administration
- **Future-proof** with external configuration
- **Self-contained** - Creates all necessary files automatically
- **uMod compliant** - Follows all submission guidelines

## Installation

1. Download the plugin file `BurnedBegone.cs`
2. Place it in your `oxide/plugins/` directory
3. **That's it!** The plugin automatically creates:
   - Configuration file (`oxide/config/BurnedBegone.json`)
   - Language files (`oxide/lang/de/BurnedBegone.json`, `oxide/lang/fr/BurnedBegone.json`)
4. Restart your server or use `oxide.reload BurnedBegone`

## Permissions

| Permission | Description |
|------------|-------------|
| `burnedbegone.use` | Allows players to benefit from meat burning protection |
| `burnedbegone.admin` | Allows access to admin commands (reload, toggle) |

### Grant Permissions
```
oxide.grant user <username> burnedbegone.use
oxide.grant group <groupname> burnedbegone.use
oxide.grant user <admin> burnedbegone.admin
```

## Chat Commands

| Command | Permission Required | Description |
|---------|-------------------|-------------|
| `/bb` or `/burnedbegone` | None | Show help menu |
| `/bb status` | None | Check your protection status |
| `/bb info` | None | Show plugin information |
| `/bb reload` | `burnedbegone.admin` | Reload plugin configuration |
| `/bb toggle` | `burnedbegone.admin` | Instructions for toggling plugin |

## Configuration

The plugin automatically creates a configuration file at `oxide/config/BurnedBegone.json`:

```json
{
  "Plugin Settings": {
    "Enable Logging": true,
    "Require Permission": false,
    "Permission Mode": "global",
    "Enable Chat Commands": true,
    "Auto Create Language Files": true
  },
  "Protected Items": [
    "bearmeat.cooked", "chicken.cooked", "deermeat.cooked", 
    "horsemeat.cooked", "humanmeat.cooked", "meat.pork.cooked", 
    "wolfmeat.cooked", "fish.cooked", "bigcatmeat.cooked", 
    "crocodilemeat.cooked", "snakemeat.cooked", "fish.anchovy.cooked", 
    "fish.catfish.cooked", "fish.herring.cooked", "fish.salmon.cooked", 
    "fish.sardine.cooked", "fish.smallshark.cooked", "fish.troutsmall.cooked", 
    "fish.yellowperch.cooked", "meat.boar.cooked", "cactusflesh.cooked"
  ],
  "Additional Items": [],
  "Excluded Items": []
}
```

### Configuration Options

- **Enable Logging**: Enable/disable console logging
- **Require Permission**: Whether players need permission to benefit from protection
- **Permission Mode**: 
  - `"global"`: All items are protected server-wide when any player has permission
  - `"individual"`: Protection only applies when individual players with permission cook items
- **Enable Chat Commands**: Enable/disable chat command functionality
- **Auto Create Language Files**: Automatically create language files for supported languages
- **Protected Items**: List of items protected by default
- **Additional Items**: Add custom items to protect
- **Excluded Items**: Remove specific items from protection

## Protected Items

The plugin protects all current cooked meat and fish items, including:

### Original Items
- Bear Meat (Cooked)
- Chicken (Cooked)
- Deer Meat (Cooked)
- Horse Meat (Cooked)
- Human Meat (Cooked)
- Pork (Cooked)
- Wolf Meat (Cooked)
- Fish (Cooked)

### Jungle Update Items (2025)
- Big Cat Meat (Cooked)
- Crocodile Meat (Cooked)
- Snake Meat (Cooked)

### Fish Varieties
- Anchovy (Cooked)
- Catfish (Cooked)
- Herring (Cooked)
- Salmon (Cooked)
- Sardine (Cooked)
- Small Shark (Cooked)
- Small Trout (Cooked)
- Yellow Perch (Cooked)

### Other Items
- Boar Meat (Cooked)
- Cactus Flesh (Cooked)

## Localization

The plugin **automatically creates** language files when first loaded. No manual setup required!

### Automatically Created Languages
- **English** (built-in)
- **German** (`oxide/lang/de/BurnedBegone.json`)
- **French** (`oxide/lang/fr/BurnedBegone.json`)

Language files are created automatically in the correct Oxide directory structure.

### Setting Player Language
Players can set their language using:
```
oxide.lang <language_code> <username>
```

Example: `oxide.lang de SomePlayer` (sets German for SomePlayer)

## Console Commands

| Command | Description |
|---------|-------------|
| `oxide.reload BurnedBegone` | Reload the plugin |
| `oxide.unload BurnedBegone` | Unload the plugin |
| `oxide.load BurnedBegone` | Load the plugin |

## Troubleshooting

### Plugin Not Working
1. Check if the plugin is loaded: `oxide.plugins`
2. Verify configuration file exists: `oxide/config/BurnedBegone.json`
3. Check console for errors during plugin load
4. Ensure you have the latest Oxide/uMod version

### Permission Issues
1. Verify permissions are granted correctly
2. Check permission mode in configuration
3. Use `/bb status` to check protection status
4. Review console logs for permission-related messages

### Items Still Burning
1. Check if the item is in the protected items list
2. Verify the item shortname is correct
3. Check if the item is in the excluded items list
4. Ensure individual permission mode is working correctly

### Chat Commands Not Working
1. Verify "Enable Chat Commands" is true in config
2. Check if player has required permissions
3. Try reloading the plugin configuration

## Developer Information

- **Author**: MLeeM97
- **Version**: 1.3.0
- **Compatibility**: Oxide/uMod 2.0.6511+
- **Rust Protocol**: Current
- **Original Author**: S642667

## Version History

### 1.3.0
- Complete rewrite for modern Oxide/uMod compatibility
- Added permission system with global/individual modes
- Implemented external configuration system
- Added chat commands for user interaction
- Multi-language support with localization
- Added all current Rust cooked items including Jungle Update
- Future-proof design with configurable item lists

### Previous Versions
- Legacy versions by S642667

## Support

For support, issues, or feature requests, please visit the plugin page on uMod.org or create an issue in the GitHub repository.

## License

MIT License

Copyright (c) 2025 MLeeM97

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
