using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using RecompOne.Runtime.Config;
using RecompOne.Runtime.Hardware;
using RecompOne.Runtime.Host;
using RecompOne.Runtime.Memory;
using Silk.NET.Windowing.Sdl.Android;
using System.Security.Cryptography;

namespace RecompOne.SoTN.Android;

[Activity(Label = "SymphonyRecomp Ultimate", MainLauncher = true,
    Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden,
    ScreenOrientation = ScreenOrientation.SensorLandscape)]
public class MainActivity : SilkActivity
{
    string _disc = "";
    TouchOverlayView? _controls;
    BorderOverlayView? _border;
    BorderStyle _borderStyle;
    int _nextRenderScale, _runningRenderScale;
    readonly ManualResetEventSlim _foreground = new(false);
    readonly CancellationTokenSource _lifetime = new();
    readonly TaskCompletionSource<string> _discReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    const int PickDiscRequest = 401;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Directory.SetCurrentDirectory(FilesDir!.AbsolutePath);
        _disc = Path.Combine(GetExternalFilesDir(null)!.AbsolutePath, "disc", "ultimate.cue");
        ConfigManager.Load();
        ConfigManager.Game.CdPath = _disc;
        ConfigManager.SaveGame();
        HostWindow.CurrentAspectRatio = HostWindow.AspectRatioMode.Original_4_3;
        var displayPrefs=GetSharedPreferences("ultimate-display",FileCreationMode.Private)!;
        UltimateView.Widescreen=displayPrefs.GetBoolean("widescreen",true);
        UltimateView.FillHeight=displayPrefs.GetBoolean("fill-height",true);
        _nextRenderScale=_runningRenderScale=Math.Clamp(displayPrefs.GetInt("render-scale",1),1,4);
        HostWindow.AndroidRenderScaleOverride=_runningRenderScale;
        _borderStyle=(BorderStyle)Math.Clamp(displayPrefs.GetInt("border",(int)BorderStyle.NightStainedGlass),0,3);
        Console.WriteLine($"[Ultimate] Independent single-screen prototype 0.6-test; internal resolution {_runningRenderScale}x.");
    }

    protected override void OnPostCreate(Bundle? savedInstanceState)
    {
        base.OnPostCreate(savedInstanceState);
        _border=new BorderOverlayView(this);
        _border.SetStyle(_borderStyle);
        (Window!.DecorView as ViewGroup)!.AddView(_border,new ViewGroup.LayoutParams(-1,-1));
        _controls = new TouchOverlayView(this) { TouchOpacity = 0.35f };
        _controls.OnMenuClicked = ShowMenu;
        _controls.OnAllHiddenChanged = hidden => GetSharedPreferences("ultimate-controls", FileCreationMode.Private)!
            .Edit()!.PutBoolean("all-hidden", hidden)!.Apply();
        _controls.SetAllHidden(GetSharedPreferences("ultimate-controls", FileCreationMode.Private)!.GetBoolean("all-hidden", false));
        (Window!.DecorView as ViewGroup)!.AddView(_controls, new ViewGroup.LayoutParams(-1, -1));
    }

    void ShowMenu()
    {
        _controls?.CancelReveal();
        Controller.SetExternalState(0xffff);
        new AlertDialog.Builder(this).SetTitle("SymphonyRecomp Ultimate 0.6-test")!
            .SetItems(new[] { "Resume", "Display", _controls?.AllHidden==true ? "Show all controls" : "Hide all controls", "Touch controls: " + (_controls?.TouchVisible == true ? "On" : "Off") }, (_, args) =>
            {
                if (args.Which == 1) ShowDisplay();
                if (args.Which == 2 && _controls?.AllHidden==true) { _controls.SetAllHidden(false); return; }
                if (args.Which == 2)
                    new AlertDialog.Builder(this).SetTitle("Hide all controls")!
                        .SetMessage("MENU and HIDE will also disappear. Hold one finger in the top-right corner for 1 second to show all controls again.")!
                        .SetPositiveButton("Hide", (_, _) => _controls?.SetAllHidden(true))!
                        .SetNegativeButton("Cancel", (_, _) => { })!.Show();
                if (args.Which == 3 && _controls != null) { _controls.TouchVisible = !_controls.TouchVisible; _controls.Invalidate(); }
            })!.Show();
    }

    void ShowDisplay()
    {
        new AlertDialog.Builder(this).SetTitle("Display")!
            .SetItems(new[] { "Aspect ratio: " + (UltimateView.Widescreen ? "16:9" : "4:3"), "Borders: " + BorderName(_borderStyle), "Gameplay framing: " + (UltimateView.FillHeight ? "Fill screen" : "Original vertical borders"), "Internal resolution: " + _nextRenderScale + "x" + (_nextRenderScale!=_runningRenderScale ? " (restart)" : ""), "Back" }, (_,args) =>
            {
                if(args.Which==0)
                    new AlertDialog.Builder(this).SetTitle("Game aspect ratio")!
                        .SetSingleChoiceItems(new[]{"Original 4:3","Widescreen 16:9"},UltimateView.Widescreen?1:0,(_,choice)=>
                        {
                            UltimateView.Widescreen=choice.Which==1;
                            GetSharedPreferences("ultimate-display",FileCreationMode.Private)!.Edit()!.PutBoolean("widescreen",UltimateView.Widescreen)!.Apply();
                        })!.SetPositiveButton("Done",(_,_)=>ShowDisplay())!.Show();
                if(args.Which==1)
                    new AlertDialog.Builder(this).SetTitle("Borders")!
                        .SetSingleChoiceItems(new[]{"Off","Gothic Castle","Alucard and Dracula","Night Stained Glass"},(int)_borderStyle,(_,choice)=>
                        {
                            _borderStyle=(BorderStyle)choice.Which;
                            _border?.SetStyle(_borderStyle);
                            GetSharedPreferences("ultimate-display",FileCreationMode.Private)!.Edit()!.PutInt("border",choice.Which)!.Apply();
                        })!.SetPositiveButton("Done",(_,_)=>ShowDisplay())!.Show();
                if(args.Which==2)
                    new AlertDialog.Builder(this).SetTitle("Gameplay framing")!
                        .SetSingleChoiceItems(new[]{"Fill screen","Original vertical borders"},UltimateView.FillHeight?0:1,(_,choice)=>
                        {
                            UltimateView.FillHeight=choice.Which==0;
                            GetSharedPreferences("ultimate-display",FileCreationMode.Private)!.Edit()!.PutBoolean("fill-height",UltimateView.FillHeight)!.Apply();
                        })!.SetPositiveButton("Done",(_,_)=>ShowDisplay())!.Show();
                if(args.Which==3)
                    new AlertDialog.Builder(this).SetTitle("Internal resolution")!
                        .SetSingleChoiceItems(new[]{"1x - Original (recommended)","2x","3x","4x"},_nextRenderScale-1,(_,choice)=>
                        {
                            _nextRenderScale=choice.Which+1;
                            GetSharedPreferences("ultimate-display",FileCreationMode.Private)!.Edit()!.PutInt("render-scale",_nextRenderScale)!.Apply();
                        })!.SetPositiveButton("Done",(_,_)=>new AlertDialog.Builder(this).SetTitle("Internal resolution")!
                            .SetMessage("Close the app from Recent apps and reopen it to apply changes. 1x gives the best performance.")!
                            .SetPositiveButton("OK",(_,_)=>ShowDisplay())!.Show())!.Show();
                if(args.Which==4)ShowMenu();
            })!.Show();
    }

    static string BorderName(BorderStyle style)=>style switch
    {
        BorderStyle.GothicCastle=>"Gothic Castle",
        BorderStyle.AlucardAndDracula=>"Alucard and Dracula",
        BorderStyle.NightStainedGlass=>"Night Stained Glass",
        _=>"Off"
    };

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        if (keyCode is Keycode.Back or Keycode.Menu) { ShowMenu(); return true; }
        return base.OnKeyDown(keyCode, e);
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus && Window != null)
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation | SystemUiFlags.ImmersiveSticky);
            // SDL can attach its content view after OnPostCreate. Keep the touch
            // overlay above it for hit testing as well as for drawing.
            Window.DecorView.Post(() =>
            {
                _border?.BringToFront();
                _controls?.BringToFront();
            });
        }
    }

    void ShowDiscSetup(string? error = null)
    {
        new AlertDialog.Builder(this).SetTitle("Install SOTN Ultimate v1.1")!
            .SetMessage((error == null ? "" : error + "\n\n") + "Choose your SOTN Ultimate (PS1 Version) v1.1 BIN file. The app copies and verifies it locally; allow about 700 MB of free storage.")!
            .SetPositiveButton("Choose BIN", (_, _) =>
            {
                var intent = new Intent(Intent.ActionOpenDocument);
                intent.SetType("*/*");
                intent.AddCategory(Intent.CategoryOpenable);
                StartActivityForResult(intent, PickDiscRequest);
            })!.SetCancelable(false)!.Show();
    }

    protected override async void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode != PickDiscRequest) return;
        if (resultCode != Result.Ok || data?.Data == null) { ShowDiscSetup("No file selected."); return; }
        var progress = new ProgressDialog(this) { Indeterminate = true };
        progress.SetMessage("Copying and verifying Ultimate. Please wait...");
        progress.SetCancelable(false);
        progress.Show();
        try
        {
            var uri = data.Data;
            await Task.Run(() =>
            {
                string dir = Path.GetDirectoryName(_disc)!;
                Directory.CreateDirectory(dir);
                string partial = Path.Combine(dir, "ultimate.partial");
                try
                {
                    using var input = ContentResolver!.OpenInputStream(uri) ?? throw new IOException("Cannot open selected file.");
                    using var digest = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
                    long total = 0;
                    using (var output = File.Create(partial))
                    {
                        var buffer = new byte[1024 * 1024];
                        int count;
                        while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            total += count;
                            if (total > 683034912) throw new IOException("This is not the supported Ultimate v1.1 BIN.");
                            output.Write(buffer, 0, count);
                            digest.AppendData(buffer, 0, count);
                        }
                    }
                    if (total != 683034912 || Convert.ToHexString(digest.GetHashAndReset()) != "E7374196845B4D5F723BA1B178D7044C")
                        throw new IOException("The selected file does not match SOTN Ultimate v1.1. Select the unmodified 683,034,912-byte BIN from that release.");
                    File.Move(partial, Path.Combine(dir, "ultimate.bin"), true);
                    File.WriteAllText(_disc, "FILE \"ultimate.bin\" BINARY\n  TRACK 01 MODE2/2352\n    INDEX 01 00:00:00\n");
                }
                finally { if (File.Exists(partial)) File.Delete(partial); }
            });
            Console.WriteLine("[Ultimate] Disc imported and MD5 verified.");
            _discReady.TrySetResult(_disc);
        }
        catch (Exception ex) { ShowDiscSetup(ex.Message); }
        finally { progress.Dismiss(); }
    }

    protected override void OnRun()
    {
        try
        {
            if (!File.Exists(_disc))
            {
                RunOnUiThread(() => ShowDiscSetup());
                _discReady.Task.GetAwaiter().GetResult();
            }
            UltimateView.Start();
            RecompOne.Runtime.Runtime.OnBeforePresentFrame += WaitForForeground;
            try { Recompiled.Entry.Run(new PSMemory(0x00800000), _disc, "SymphonyRecomp Ultimate"); }
            finally
            {
                RecompOne.Runtime.Runtime.OnBeforePresentFrame -= WaitForForeground;
                UltimateView.Stop();
            }
        }
        catch (System.OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Ultimate] BOOT FAILURE: {ex}");
            RunOnUiThread(() => new AlertDialog.Builder(this).SetTitle("Ultimate: boot failed")!
                .SetMessage(ex.Message)!.SetPositiveButton("Close", (_, _) => { })!.Show());
        }
    }

    protected override void OnPause()
    {
        _controls?.CancelReveal();
        Controller.SetExternalState(0xffff);
        _foreground.Reset();
        Audio.Pause();
        Console.WriteLine("[Ultimate] Background pause: game and audio suspended.");
        base.OnPause();
    }

    void WaitForForeground() => _foreground.Wait(_lifetime.Token);

    protected override void OnResume()
    {
        base.OnResume();
        Audio.Resume();
        _foreground.Set();
        Console.WriteLine("[Ultimate] Foreground resume: game and audio resumed.");
    }

    protected override void OnDestroy()
    {
        _lifetime.Cancel();
        _foreground.Set();
        Audio.Pause();
        base.OnDestroy();
    }
}
