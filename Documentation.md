# Unburnable Meat

A Rust uMod/Oxide plugin that prevents cooked meat and fish from burning in ovens, furnaces, and campfires.

## Features

- **Prevents Burning**: All cooked meat and fish items won't burn when left in cooking devices
- **Permission System**: Optional permission-based protection for VIP players
- **Configurable**: External configuration file for easy customization
- **Multi-language Support**: Built-in localization system
- **Performance Optimized**: Minimal server impact
- **Revert on Unload**: Automatically restores original game behavior when plugin is unloaded

## Supported Items

### Default Protected Items
- **Meat**: Bear, Chicken, Deer, Horse, Human, Pork, Wolf, Boar
- **Fish**: Basic Fish, Anchovy, Catfish, Herring, Salmon, Sardine, Small Shark, Small Trout, Yellow Perch
- **Jungle Update**: Big Cat, Crocodile, Snake
- **Other**: Cactus Flesh

## Installation

1. Download `UnburnableMeat.cs`
2. Place in your `oxide/plugins/` folder
3. Restart server or use `oxide.reload UnburnableMeat`
4. Configure via `oxide/config/UnburnableMeat.json`

## Permissions

- `unburnablemeat.use` - Allows players to have unburnable meat (when permission mode is enabled)
- `unburnablemeat.admin` - Allows access to admin commands

### Grant Permission Examples
```
oxide.grant user <steamid> unburnablemeat.use
oxide.grant group vip unburnablemeat.use
oxide.grant user <steamid> unburnablemeat.admin
```

## Commands

| Command | Permission | Description |
|---------|------------|-------------|
| `/um` | None | Show help menu |
| `/um status` | None | Show your protection status |
| `/um info` | None | Show plugin information |
| `/um reload` | `unburnablemeat.admin` | Reload configuration |
| `/um toggle` | `unburnablemeat.admin` | Instructions for toggling plugin |

## Configuration

The plugin creates `oxide/config/UnburnableMeat.json` with the following structure:

```json
{
  "Plugin Settings": {
    "Enable Logging": true,
    "Require Permission": false,
    "Permission Mode": "global",
    "Enable Chat Commands": true
  },
  "Default Cooked Items": [
    "bearmeat.cooked",
    "chicken.cooked",
    "deermeat.cooked",
    "horsemeat.cooked",
    "humanmeat.cooked",
    "meat.pork.cooked",
    "wolfmeat.cooked",
    "fish.cooked",
    "bigcatmeat.cooked",
    "crocodilemeat.cooked",
    "snakemeat.cooked",
    "fish.anchovy.cooked",
    "fish.catfish.cooked",
    "fish.herring.cooked",
    "fish.salmon.cooked",
    "fish.sardine.cooked",
    "fish.smallshark.cooked",
    "fish.troutsmall.cooked",
    "fish.yellowperch.cooked",
    "meat.boar.cooked",
    "cactusflesh.cooked"
  ],
  "Additional Items": [],
  "Excluded Items": []
}
```

### Configuration Options

- **Enable Logging**: Show detailed console output (default: `true`)
- **Require Permission**: Enable permission-based protection (default: `false`)
- **Permission Mode**: 
  - `"global"`: Server-wide protection when any player has permission
  - `"individual"`: Per-player protection (experimental)
- **Enable Chat Commands**: Allow players to use chat commands (default: `true`)
- **Additional Items**: Add custom item shortnames to protect
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
Then grant permission: `oxide.grant group vip unburnablemeat.use`

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

The plugin supports multiple languages. Create files in `oxide/lang/` folder:

### Supported Languages
- English (default)
- German (Deutsch)
- French (Français)
- Spanish (Español)
- Italian (Italiano)
- Dutch (Nederlands)
- Portuguese (Português)

Example: `oxide/lang/de/UnburnableMeat.json` for German translations.

## API

### Hooks
The plugin uses standard Oxide hooks and doesn't expose custom API methods.

### Item Detection
Items are detected by their shortname. The plugin automatically handles:
- Item definition lookup
- Cookable component validation
- Temperature range modification

## Compatibility

- **Rust Version**: Latest (tested with Protocol 2592.269.1)
- **uMod/Oxide**: v2.0.6511+
- **Dependencies**: None
- **Conflicts**: Should work with most cooking-related plugins

## Performance

- **Startup Time**: < 1 second
- **Memory Usage**: Minimal (~50KB)
- **CPU Impact**: Negligible (only runs on server initialization)
- **Network Traffic**: None

## Troubleshooting

### Plugin Not Working
1. Check console for error messages
2. Verify `"Enable Logging": true` in config
3. Ensure item shortnames are correct
4. Restart server after configuration changes

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
