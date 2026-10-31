using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;
using RecompOne.Runtime.Hle;
using RecompOne.Runtime.Events;

namespace Recompiled;
public static partial class WidescreenPatch
{
    public static void RegisterUltimateFlashes() => Event.AddListener<RenderPrimEvent>(e =>
    {
        int margin=StageMargin();
        if (OriginalAspect || !DisplayModeHooks.IsStage || margin==0 || e.Textured || e.Gouraud || e.Count!=2) return;
        if(e.X[1]-e.X[0]!=256 || e.Y[1]-e.Y[0]!=240)return;
        e.X[0]-=margin;e.X[1]+=margin;
    });
    static bool Wide(Action<CpuContext, IMemory> apply, CpuContext c, IMemory m)
    {
        if (OriginalAspect || Display.WideAspect <= 0) return true;
        apply(c,m); return false;
    }
    public static bool ConfigStageClipWide(CpuContext c,IMemory m) => Wide(ConfigStageClip,c,m);
    public static bool RenderTilemapWideHook(CpuContext c,IMemory m) => Wide(RenderTilemapWide,c,m);
    public static bool InitFadeWide(CpuContext c,IMemory m) => Wide(InitFade,c,m);
    public static bool SetFadeWidthWide(CpuContext c,IMemory m) => Wide(SetFadeWidth,c,m);
    public static bool CamColWide(CpuContext c,IMemory m) => Wide(CamCol,c,m);
    static bool WideExt(Action<CpuContext,IMemory> apply,CpuContext c,IMemory m,params string[] needs)
    {
        if (!Extended || !StageHas(m,needs)) return true;
        apply(c,m); return false;
    }
    public static bool UpdateRoomPositionWide(CpuContext c,IMemory m) => WideExt(UpdateRoomPosition,c,m,"CreateEntitiesToTheRight","CreateEntitiesToTheLeft","CreateEntitiesAbove","CreateEntitiesBelow");
    public static bool CreateEntityWhenInHorizontalRangeWide(CpuContext c,IMemory m) => WideExt(CreateEntityWhenInHorizontalRange,c,m,"CreateEntityFromLayout");
    public static bool CreateEntityWhenInVerticalRangeWide(CpuContext c,IMemory m) => WideExt(CreateEntityWhenInVerticalRange,c,m,"CreateEntityFromLayout");
    public static bool UltimateFillHeight;
    public static void ConfigureUltimate(bool enabled, bool fillHeight = false)
    {
        EnsureInitialized();
        OriginalAspect = !enabled;
        UltimateFillHeight = fillHeight;
        Unstretch = false;
        PillarBoxing = true;
        Display.TargetAspect = 16f/9f;
        Refresh();
    }
}
