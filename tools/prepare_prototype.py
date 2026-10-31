"""Prepare the independent Ultimate experiment from local user-supplied discs."""
from pathlib import Path
import hashlib
import json
import struct

def canonical(data):
    """Ignore relocated jump targets and pointer materialisation, retaining opcodes/registers."""
    words = list(struct.unpack('<'+'I'*(len(data)//4), data[:len(data)//4*4]))
    pointer_regs = {}
    for i, word in enumerate(words):
        op, rs, rt = word >> 26, (word >> 21)&31, (word >> 16)&31
        if op in (2, 3):
            words[i] = word & 0xfc000000
        elif op == 15 and 0x8000 <= (word & 0xffff) <= 0x8020:
            pointer_regs[rt] = i
            words[i] = word & 0xffff0000
        elif op in (9, 13, 32, 33, 35, 36, 37, 40, 41, 43) and rs in pointer_regs and i-pointer_regs[rs] <= 6:
            words[i] = word & 0xffff0000
        if op in (2, 3) or (op == 0 and (word&63) in (8, 9)):
            pointer_regs.clear()
    return struct.pack('<'+'I'*len(words), *words)

ROOT = Path(__file__).resolve().parents[1]
WORKSPACE = ROOT.parent

class Disc:
    def __init__(self, path):
        self.handle = path.open('rb')
        pvd = self.read(16, 2048)
        if pvd[1:6] != b'CD001':
            raise ValueError('Not a supported raw MODE2/2352 ISO9660 disc')
        self.files = {}
        self.walk(struct.unpack_from('<I', pvd, 158)[0], struct.unpack_from('<I', pvd, 166)[0])

    def read(self, lba, size):
        result = bytearray()
        while len(result) < size:
            self.handle.seek(lba * 2352 + 24)
            part = self.handle.read(min(2048, size-len(result)))
            if not part:
                raise EOFError('Disc is truncated')
            result.extend(part)
            lba += 1
        return bytes(result)

    def walk(self, lba, size, prefix=''):
        data = self.read(lba, size)
        pos = 0
        while pos < len(data):
            length = data[pos]
            if not length:
                pos = (pos//2048 + 1)*2048
                continue
            name = data[pos+33:pos+33+data[pos+32]].decode('ascii')
            extent = struct.unpack_from('<I', data, pos+2)[0]
            count = struct.unpack_from('<I', data, pos+10)[0]
            if name not in ('\0', '\1'):
                path = (prefix+name).split(';')[0]
                if data[pos+25] & 2:
                    self.walk(extent, count, path+'/')
                else:
                    self.files[path] = (extent, count)
            pos += length

    def file(self, name):
        return self.read(*self.files[name])

