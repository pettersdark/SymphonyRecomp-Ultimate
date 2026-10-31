using RecompOne.Runtime.Context;
using RecompOne.Runtime.Memory;

namespace Recompiled;

// Move native HUD primitives after Ultimate has built/updated them. Do not
// replace its HUD implementation or its added-character logic.
public static partial class WidescreenPatch
{
    const uint UltimateHud=0x8013796C;
    static int _alucardHudMargin, _richterHudMargin;
    static bool RichterHud(IMemory m) => m.ReadU32(0x800974A0)==0x1F || m.ReadU32(0x8003C9A0)!=0;
    static uint HudHead(IMemory m,uint slot)
    {
        uint index=m.ReadU32(slot);
        return index<1280 ? PrimBufAddr+index*PrimStride : 0;
    }
    static bool HudPointer(uint p)=>p>=PrimBufAddr && p<PrimBufAddr+1280*PrimStride && (p-PrimBufAddr)%PrimStride==0;
    static List<uint> HudChain(IMemory m,uint slot,int maximum)
    {
        var result=new List<uint>();uint p=HudHead(m,slot);
        while(HudPointer(p) && result.Count<maximum && !result.Contains(p)) { result.Add(p);p=m.ReadU32(p); }
        return result;
    }
    static void HudShift(IMemory m,uint p,int delta)
    {
        if(!HudPointer(p) || delta==0)return;
        HudShiftField(m,p+0x08,delta);HudShiftField(m,p+0x14,delta);
        HudShiftField(m,p+0x20,delta);HudShiftField(m,p+0x2C,delta);
    }
    static void HudShiftField(IMemory m,uint address,int delta) =>
        m.WriteU16(address,unchecked((ushort)((short)m.ReadU16(address)+delta)));
    public static void UltimateHudInit(CpuContext c,IMemory m)
    {
        if(RichterHud(m))return;
        int margin=StageMargin();
        foreach(uint p in HudChain(m,UltimateHud+4,32))HudShift(m,p,-margin);
        _alucardHudMargin=margin;
    }
    public static void UltimateHudUpdate(CpuContext c,IMemory m)
    {
        if(RichterHud(m))return;
        int margin=StageMargin();var chain=HudChain(m,UltimateHud+4,32);
        for(int i=0;i<Math.Min(6,chain.Count);i++)HudShift(m,chain[i],_alucardHudMargin-margin);
        for(int i=6;i<Math.Min(14,chain.Count);i++)HudShift(m,chain[i],-margin);
        _alucardHudMargin=margin;
    }
    public static void UltimateRichterHudInit(CpuContext c,IMemory m)
    {
        if(m.ReadU32(0x8003C9A0)>1)return;
        int margin=StageMargin();var chain=HudChain(m,UltimateHud+4,32);
        for(int i=0;i<chain.Count;i++)HudShift(m,chain[i],i is 1 or 3 or 4 ? margin : -margin);
        foreach(uint p in HudChain(m,UltimateHud+8,128))HudShift(m,p,margin);
        _richterHudMargin=margin;
    }
    public static void UltimateRichterHudUpdate(CpuContext c,IMemory m)
    {
        if(m.ReadU32(0x8003C9A0)>1)return;
        int margin=StageMargin(),delta=margin-_richterHudMargin;
        var chain=HudChain(m,UltimateHud+4,32);
        if(m.ReadU32(0x8003C744)==5)
        {
            for(int i=0;i<chain.Count;i++)HudShift(m,chain[i],i is 1 or 3 or 4 ? delta : -delta);
        }
        else
        {
            uint crash=m.ReadU32(UltimateHud+0x24);
            bool crashMoves=crash-10<=3 || crash-60<=3;
            for(int i=0;i<chain.Count;i++)
            {
                int shift=i switch
                {
                    1 or 3 or 4=>margin, // these coordinates are rebuilt by the native updater
                    6=>m.ReadU16(chain[i]+0x32)!=8 ? -margin : 0,
                    7=>crashMoves ? -margin : -delta,
                    _=>-delta
                };
                HudShift(m,chain[i],shift);
            }
        }
        foreach(uint p in HudChain(m,UltimateHud+8,128))HudShift(m,p,delta);
        // Native pause/crash text animation sometimes rebuilds this rectangle at
        // x=33. Anchor the label after that write as well as after aspect changes.
        if(chain.Count>8)HudShift(m,chain[8],33-margin-(short)m.ReadU16(chain[8]+8));
        _richterHudMargin=margin;
    }
}
