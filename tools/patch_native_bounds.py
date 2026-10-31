"""Widen ONLY native horizontal screen tests; retain all Ultimate combat/balance code."""
from prepare_prototype import ROOT,WORKSPACE,Disc,canonical
import json,struct,re

image=ROOT/'disc/SOTN Ultimate (PS1 Version) v1.1.bin'
import hashlib
with image.open('rb') as stream:
    assert hashlib.file_digest(stream,'md5').hexdigest()=='e7374196845b4d5f723ba1b178d7044c', 'Wrong disc version'
disc=Disc(ROOT/'disc/SOTN Ultimate (PS1 Version) v1.1.bin')
cfg=json.loads((ROOT/'config/sotn.json').read_text())
report=[]
for ov in cfg['overlays']:
    if ov.get('base')!='0x80180000' or not ov.get('file'):continue
    data=disc.file(ov['file']);base=int(ov['base'],16)
    update,collision=struct.unpack_from('<II',data)
    if not all(base<=a<base+len(data) and a%4==0 for a in (update,collision)):continue
    edits={};profiles={}
    for kind,start,length,expected in [('update',update,0x300,3),('collision',collision,0x300,2)]:
        words=struct.unpack_from('<'+'I'*(length//4),data,start-base)
        found=[]
        for i,w in enumerate(words):
            op,rs,rt,imm=w>>26,(w>>21)&31,(w>>16)&31,w&65535
            allowed=(0x181,0x201) if kind=='update' else (0x141,0x13f)
            if op!=11 or rs!=rt or imm not in allowed:continue
            offset={0x181:64,0x201:128,0x141:32,0x13f:31}[imm]
            previous=next((j for j in range(i-1,max(-1,i-9),-1)
                if words[j]>>26==9 and (words[j]>>16)&31==rs and words[j]&65535==offset),None)
            if previous is None:continue
            found.append((start+previous*4,offset,1));found.append((start+i*4,imm,2))
        profiles[kind]=len(found)//2
        if len(found)//2==expected:
            for addr,imm,factor in found:edits[addr]=(imm,factor)
    path=ROOT/'generated'/f"{ov['name']}.cs"
    lines=path.read_text(encoding='utf-8-sig').splitlines()
    seen=set();changed=0
    for i,line in enumerate(lines):
        address=re.search(r'/\* 0x([0-9A-F]{8}) \*/',line)
        if not address or int(address[1],16) not in edits:continue
        addr=int(address[1],16);imm,factor=edits[addr]
        if 'WidescreenPatch.Margin' in line:seen.add(addr);continue
        literal=rf'0x0*{imm:X}u\b'
        replacement=f'(uint)(0x{imm:X} + WidescreenPatch.Margin'+(' * 2' if factor==2 else '')+')'
        newline,n=re.subn(literal,replacement,line,flags=re.I)
        if n!=1:raise RuntimeError(f'Unexpected generated instruction at {ov["name"]}:{addr:X}: {line}')
        lines[i]=newline;seen.add(addr);changed+=1
    if seen!=set(edits):raise RuntimeError(f'Missing instruction addresses in {path}: {set(edits)-seen}')
    path.write_text('\n'.join(lines)+'\n',encoding='utf-8')
    report.append(dict(overlay=ov['name'],profiles=profiles,addresses=len(edits),generated_lines=changed))
assert next(r for r in report if r['overlay']=='no3')['addresses']==10
assert next(r for r in report if r['overlay']=='st0')['addresses']==10
# Ultimate has two copies of the player-entity screen test (its added characters).
# Match the complete native block around X/Y checks and DestroyEntity, not a
# global search-and-replace of numeric constants.
# Verified blocks for the exact MD5-checked Ultimate v1.1 disc.
hits=[0x80119200,0x801194B0]
edits={a+0x1C:(32,1) for a in hits}
edits.update({a+0x24:(321,2) for a in hits})
path=ROOT/'generated/dra.cs';lines=path.read_text(encoding='utf-8-sig').splitlines();seen=set()
for i,line in enumerate(lines):
    address=re.search(r'/\* 0x([0-9A-F]{8}) \*/',line)
    if not address or int(address[1],16) not in edits:continue
    addr=int(address[1],16);imm,factor=edits[addr]
    if 'WidescreenPatch.Margin' in line:seen.add(addr);continue
    replacement=f'(uint)(0x{imm:X} + WidescreenPatch.Margin'+(' * 2' if factor==2 else '')+')'
    lines[i],count=re.subn(rf'0x0*{imm:X}u\b',replacement,line,flags=re.I)
    assert count==1,(addr,line)
    seen.add(addr)
assert seen==set(edits)
path.write_text('\n'.join(lines)+'\n',encoding='utf-8')
report.append(dict(overlay='dra-player-cull',blocks=[hex(a) for a in hits],addresses=len(edits)))
(ROOT/'generated/native-bounds-patches.json').write_text(json.dumps(report,indent=2))
print('Native horizontal bounds:',len(report),'stage overlays;',sum(r['addresses'] for r in report),'instructions')
