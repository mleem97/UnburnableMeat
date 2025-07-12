# Burned Begone

A Rust uMod/Oxide plugin that prevents cooked meat and fish from burning in ovens, furnaces, and campfires.

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
   - Language support for German and French
4. Restart your server or use `oxide.reload BurnedBegone`

## Permissions

| Permission | Description |
|------------|-------------|
| `burnedbegone.use` | Allows players to benefit from meat burning protection |
| `burnedbegone.admin` | Allows access to admin commands (reload, toggle) |

### Grant Permission Examples
```
oxide.grant user <username> burnedbegone.use
oxide.grant group <groupname> burnedbegone.use
oxide.grant user <admin> burnedbegone.admin
```

## Commands

| Command | Permission Required | Description |
|---------|-------------------|-------------|
| `/bb` or `/burned_begone` | None | Show help menu |
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

- **Enable Logging**: Show detailed console output (default: `true`)
- **Require Permission**: Enable permission-based protection (default: `false`)
- **Permission Mode**: 
  - `"global"`: All items are protected server-wide when any player has permission
  - `"individual"`: Protection only applies when individual players with permission cook items
- **Enable Chat Commands**: Enable/disable chat command functionality
- **Auto Create Language Files**: Automatically create language files for supported languages
- **Protected Items**: List of items protected by default
- **Additional Items**: Add custom items to protect
- **Excluded Items**: Remove specific items from protection

## Usage Examples

### For All Players (Default)
Set `"Require Permission": false` - all players get protection automatically.

### VIP/Donor Only
```json
{
  "Plugin Settings": {
    "Require Permission": true,
    "Permission Mode": "global"
  }
}
```
Then grant permission: `oxide.grant group vip burnedbegone.use`

### Adding Custom Items
```json
{
  "Additional Items": [
    "customfood.cooked",
    "moddedfish.cooked"
  ]
}
```

### Excluding Specific Items
```json
{
  "Excluded Items": [
    "humanmeat.cooked"
  ]
}
```

## Localization

The plugin **automatically registers** language support when first loaded. No manual setup required!

### Supported Languages
- **English** (built-in)
- **German** (automatically available)
- **French** (automatically available)

Language messages are automatically registered with Oxide's language system.

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
1. Check if item is in "Excluded Items" list
2. Verify permission settings if using permission mode
3. Confirm item shortname exists in current Rust version

### Permission Issues
1. Verify permission is granted: `oxide.show user <steamid>`
2. Check permission mode setting in config
3. Ensure `"Require Permission": true` if using permissions

## Support

- **GitHub**: [Report Issues](https://github.com/your-repo/UnburnableMeat/issues)
- **Discord**: Join uMod Discord for support
- **Forum**: Post on uMod forums

## Changelog

### v1.3.0
- Added permission system
- External configuration file
- Multi-language support
- Chat commands
- Performance optimizations
- uMod guidelines compliance

### v1.2.0
- Updated for uMod/Oxide v2.0.6511
- Added Jungle Update items
- Improved error handling

### v1.1.0
- Added fish varieties
- Updated for modern Rust versions

### v1.0.1
- Original version by S642667

## Credits

- **Original Author**: S642667
- **Updated by**: MLeeM97
- **Inspired by**: Community feedback and requests

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