# SymphonyRecomp Ultimate - Android

[Español](#español) | [English](#english)

## English

A standalone, single-screen Android adaptation for **SOTN Ultimate (PS1 Version) v1.1**. Play with Alucard, Richter (PS1/Saturn costume) or Maria (Saturn/PSP), select balance and voices, and configure 4:3 or 16:9 gameplay, decorative borders and touch controls.

**[Download APKs and guides](https://github.com/pettersdark/SymphonyRecomp-Ultimate/releases/latest)**

### Install

1. Install `SymphonyRecomp-Ultimate-0.6-android-arm64.apk` on your Android handheld. The x64 APK is for an x64 Android emulator on PC.
2. Supply your own `SOTN Ultimate (PS1 Version) v1.1.bin` (683,034,912 bytes; MD5 `e7374196845b4d5f723ba1b178d7044c`). Other disc versions are incompatible.
3. Open the app, tap **Choose BIN** and select the disc. Import verifies it and creates the CUE automatically.
4. Start a fresh game and choose your character and options. No prepared saves are required or provided.

The repository, APKs and public download package contain **no disc image or saved games**. This is independent from the DUAL edition; do not import its save states.

### Features

- Alucard: selectable Richter and Maria boss variants, PS1/Saturn balance, PSP/PS1/JP voices.
- Richter: PS1/Saturn costume, PS1/Saturn balance, PSP/PS1/JP voices.
- Maria: Saturn/PSP versions, PS1/Saturn balance, PSP/JP voices.
- **MENU > Display**: aspect ratio, three decorative borders or Off, gameplay framing, internal resolution 1x-4x (restart required for resolution).
- **Fill screen** fits the complete active gameplay picture to 4:3 without cropping its sides; proportions change slightly. Original vertical borders remain selectable. Original menus and videos retain 4:3 framing.
- **HIDE** hides game controls; **Hide all controls** also hides the top buttons. Hold one finger in the top-right corner for one second to reveal them; Android Back opens MENU.
- Game and audio pause when the app goes into the background and resume on return. Use in-game save rooms before closing the app.
- Added Ultimate disc-mod content includes new areas, items, weapons and relics. See the manuals for the available modes and options.

### Guides

- [English illustrated manual](docs/SymphonyRecomp-Ultimate-0.6-Guide-EN.pdf)
- [Manual ilustrado en español](docs/SymphonyRecomp-Ultimate-0.6-Guide-ES.pdf)
- [Build from source](BUILDING.md)
- [Validation and remaining limits](docs/VALIDATION-0.6.md)

### Credits and provenance

- **[sergiomanzur/SymphonyRecomp](https://github.com/sergiomanzur/SymphonyRecomp)**: Android port, touch controls, menus, controller support and disc-import foundation. Base revision `05bfad3fa18203477456440083165d8471683238`.
- **[BlackLabelHQ/SymphonyRecomp](https://github.com/BlackLabelHQ/SymphonyRecomp)**: original recompilation, game integration, widescreen and quality-of-life work.
- **flaffy / [BlackLabelHQ/RecompOne](https://github.com/BlackLabelHQ/RecompOne)**: PlayStation static recompiler and runtime. Android fork: **[sergiomanzur/RecompOne](https://github.com/sergiomanzur/RecompOne)**, pinned to `275a1271415f97924dcfd38a721bc7407f6baa9a`.
- **belaLugosiIsDead**: SOTN Ultimate v1.1 disc modification. [Mod information](https://www.romhacking.net/hacks/10241/).
- **The SOTN Decomp community**: reverse-engineering research and game knowledge used by the upstream projects.
- **Konami / KCET**: the original game, characters and original materials.
- **pettersdark**: this Ultimate Android adaptation, runtime adjustments, presentation, testing and documentation, with Codex assistance.

The Android base README is preserved as [UPSTREAM_README.md](UPSTREAM_README.md). Human-only statements there refer to the original upstream work. This derivative is unofficial and is not endorsed by those authors or Konami. Please report adaptation issues here.

### Modify and help it evolve

Forks, modifications and improvements are welcome. Original contributions by pettersdark to this adaptation are offered under the [MIT terms in LICENSE-CONTRIBUTIONS](LICENSE-CONTRIBUTIONS). Retain credits and comply with each component's terms. This is **not a blanket relicensing** of inherited code, generated game code or third-party assets. No repository-wide licence was identified in the reviewed SymphonyRecomp bases. RecompOne has its own MIT licence. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Español

Adaptación Android independiente y de una sola pantalla para **SOTN Ultimate (PS1 Version) v1.1**. Permite jugar con Alucard, Richter (traje PS1/Saturn) y Maria (Saturn/PSP), seleccionar balance y voces y ajustar imagen 4:3/16:9, marcos y controles.

**[Descargar APK y guías](https://github.com/pettersdark/SymphonyRecomp-Ultimate/releases/latest)**

Instala el APK ARM64 en la consola, aporta tu propio BIN de la versión indicada y selecciónalo con **Choose BIN**. El CUE se crea automáticamente. El APK x64 sirve para un emulador Android x64 en PC. Se empieza desde una instalación limpia, sin partidas preparadas. No se distribuye el BIN ni se deben importar estados de DUAL.

En **MENU > Display** se eligen formato, marcos, encuadre y resolución. **Fill screen** muestra toda la imagen activa ajustada a 4:3, sin recortar los lados y con una ligera variación de proporciones. Los menús originales y los vídeos mantienen su encuadre 4:3. Mantén un dedo un segundo en la esquina superior derecha para recuperar los controles ocultos. Al minimizar se pausan juego y audio; guarda dentro del juego antes de cerrar.

**Procedencia:** se basa en el port Android de **sergiomanzur**, derivado de **BlackLabelHQ/SymphonyRecomp**, y en **RecompOne de flaffy**, con el fork Android de sergiomanzur. El mod de disco **Ultimate v1.1** es de **belaLugosiIsDead**. Se reconoce a **SOTN Decomp** por su ingeniería inversa y a **Konami/KCET** por el juego original. Los enlaces y revisiones exactas figuran arriba. La adaptación Ultimate, los ajustes y la documentación son de **pettersdark**, con asistencia de Codex.

**Puedes modificarlo y ayudar a hacerlo evolucionar:** se invita a crear forks y aportar mejoras manteniendo los créditos. Las contribuciones originales de pettersdark se ofrecen bajo MIT; esa autorización no se extiende automáticamente al código heredado sin licencia explícita, al disco ni a materiales de terceros. RecompOne mantiene su propia licencia MIT. Consulta los avisos de cada componente.

La versión 0.6 supera 44 comprobaciones de arranque con distintas opciones y formatos. Esto no equivale a validar todos los recorridos o jefes. El rendimiento en consolas físicas ARM64 sigue pendiente de comprobación. Encontrarás los detalles y el manual completo en los enlaces anteriores.
