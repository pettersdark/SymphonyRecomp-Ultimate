# Version 0.6 validation

## Verified

- Signed ARM64 and x64 Android APKs built from the same final source.
- 44 native startup combinations: 12 Richter, 8 Maria and 24 Alucard-option combinations. Each ran 360 gameplay VBlanks, split across 4:3 and 16:9, with attack, jump and special/subweapon input. No unmapped dispatch or exception occurred in those cases.
- This startup harness exercises native CPU/game logic and overlay dispatch; it does not rasterise game triangles and does not measure Android GPU performance or sound.
- The Alucard cases cover his prologue startup, played as Richter, not Alucard's complete castle route.
- Automated display checks cover camera, room/tile/HUD bounds, full-height 4:3 without lateral cropping, original framing, widescreen and uncropped native menus/video framing.
- Visual Android x64 checks include Richter PS1/Saturn and Maria PSP framing. Final APK Maria PSP captures confirm full-height 4:3 and 16:9.
- Final x64 APK background/resume smoke: Android audio active tracks changed 1 -> 0 -> 1; presentation pauses in the background and resumes on return.
- Added mappings for the alternate Saturn Richter selector and boss and PSP Maria cutscene/boss overlays. Static matching and generation completed; this does not mean their complete battles were played through.

## Still requiring play testing

- Full routes, later bosses, endings and all added areas.
- Physical ARM64 handheld performance, sustained effects-heavy combat and controller behaviour on each device. An x64 emulator is not evidence of console frame rate.
- Long-session saves and resuming across Android process termination. Use normal in-game save rooms.

One diagnostic attempt to skip the prologue by forcing a room transfer failed with incomplete player initialisation. It was not part of the app or the passing startup matrix and is not evidence of a normal-route crash. No workaround for that artificial state was added to production code.

## Español

Las 44 combinaciones comprueban arranque, opciones y cambios de formato; no son 44 recorridos completos. Las pruebas de Alucard terminan dentro de su prólogo con Richter. Los overlays de encuentros posteriores se han incorporado, pero sus combates completos siguen pendientes. Se ha comprobado el encuadre y la pausa de audio en Android x64. El rendimiento real en ARM64 y las zonas avanzadas necesitan pruebas en consola.
