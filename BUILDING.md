# Build from source

Requirements: Git, Python 3.11+, .NET 10 SDK with the Android workload, Android SDK and a JDK compatible with that workload. Configure your local SDK/JDK paths; no machine-specific paths or signing keys are supplied.

```sh
git clone https://github.com/pettersdark/SymphonyRecomp-Ultimate.git
cd SymphonyRecomp-Ultimate
python -m zipfile -e BuildInputs.zip .
git clone https://github.com/sergiomanzur/RecompOne.git RecompOne
git -C RecompOne checkout 275a1271415f97924dcfd38a721bc7407f6baa9a
dotnet workload install android
git -C RecompOne apply --check ../local-patches/recompone-ultimate.patch
git -C RecompOne apply ../local-patches/recompone-ultimate.patch
```

Apply the patch once on a clean pinned runtime checkout. It contains the touch/controller, rendering, resolution, background-audio and Android runtime changes used by this adaptation. Keep the pinned runtime revision unless you deliberately port the patch to another revision. The runtime is fetched separately rather than vendored in this repository.

`BuildInputs.zip` contains the editable function-map JSON files, decorative borders, language metadata and Android icons in their correct folders. Extract it before generating or building. It contains no game disc or generated game code. The source download archive also includes these inputs unpacked.

Place your own exact `SOTN Ultimate (PS1 Version) v1.1.bin` in `disc/`. The supplied CUE references that filename. Verify its size and MD5 against `disc/README.md`. Only this mod disc is needed; the original retail disc is not required for this prepared configuration.

Generate game code locally, then apply the precise native bounds adjustments:

```sh
dotnet run --project RecompOne/RecompOne.Recompiler -c Release -p:TargetFrameworks=net10.0 -- config/sotn.json
python tools/patch_native_bounds.py
dotnet publish Ultimate.Android.csproj -c Release -r android-arm64 -p:RuntimeIdentifiers=android-arm64
```

For an x64 Android emulator use `android-x64` in both runtime arguments. Build one architecture at a time. If SDK/JDK auto-discovery fails, add `-p:AndroidSdkDirectory=YOUR_SDK_PATH -p:JavaSdkDirectory=YOUR_JDK_PATH` with appropriate shell quoting. For distributable APKs configure your own signing key using .NET Android signing properties; never commit the private key or passwords. Your own key cannot update over APKs signed with another key.

`generated/` is intentionally ignored and is not published. The function maps and hooks are already prepared for Ultimate v1.1, including alternate character, selection, boss and cutscene overlays. Do not replace them with maps from the retail game. The bounds postprocessor checks the supplied disc hash and modifies only identified horizontal bounds instructions. Never distribute the supplied disc or locally generated game code in a source archive.

Release APKs retain the internal display version `0.6-test`; the public release number is 0.6. The manuals use the public version number.
