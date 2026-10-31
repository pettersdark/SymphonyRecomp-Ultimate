using RecompOne.Runtime.Hle;
using RecompOne.Runtime;
using Recompiled;

namespace RecompOne.SoTN.Android;
public enum BorderStyle { Off, GothicCastle, AlucardAndDracula, NightStainedGlass }

public static class UltimateView
{
    public static volatile bool Widescreen = true;
    public static volatile bool FillHeight = true;
    static float _last = -1;
    public static void Start()
    {
        WidescreenPatch.Register();
        WidescreenPatch.RegisterUltimateFlashes();
        WidescreenPatch.ConfigureUltimate(Widescreen, FillHeight);
        RecompOne.Runtime.Runtime.OnBeforePresentFrame += Tick;
    }
    static void Tick()
    {
        WidescreenPatch.ConfigureUltimate(Widescreen, FillHeight);
        if (Display.WideAspect != _last)
        {
            _last=Display.WideAspect;
            Console.WriteLine($"[UltimateView] Native mode={DisplayModeHooks.Current}; gameplay aspect={(_last>0 ? "16:9" : "4:3")}; margin={WidescreenPatch.Margin}.");
        }
    }
    public static void Stop() => RecompOne.Runtime.Runtime.OnBeforePresentFrame -= Tick;
}
