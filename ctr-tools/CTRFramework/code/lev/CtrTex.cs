﻿using CTRFramework.Shared;
using System;
using System.Collections.Generic;

namespace CTRFramework
{
    public class CtrTex : IRead
    {
        public TextureLayout[] midlods = new TextureLayout[3];
        public TextureLayout[] hi = new TextureLayout[4];
        public List<TextureLayout> animframes = new List<TextureLayout>();

        public uint ptrHi;
        public bool isAnimated = false;

        public CtrTex(BinaryReaderEx br)
        {
            Read(br);
        }

        public void Read(BinaryReaderEx br)
        {
            int pos = (int)br.BaseStream.Position;

            //this apparently defines animated texture, really
            if ((pos & 1) == 1)
            {
                isAnimated = true;

                br.BaseStream.Position -= 1;

                uint texpos = br.ReadUInt32();
                int numFrames = br.ReadInt16();
                int whatsthat = br.ReadInt16();

                if (br.ReadUInt32() != 0)
                    Console.WriteLine("ctrtex panic, not 0!");

                uint[] ptrs = br.ReadArrayUInt32(numFrames);

                foreach (uint ptr in ptrs)
                {
                    br.Jump(ptr);
                    animframes.Add(new TextureLayout(br));
                }

                br.Jump(texpos);
            }

            for (int i = 0; i < 3; i++)
                midlods[i] = new TextureLayout(br);

            ptrHi = br.ReadUInt32();

            if (ptrHi != 0)
            {
                if (ptrHi > br.BaseStream.Length)
                {
                    //there must be some flag that defines hi tex existence
                    Helpers.Panic(this, "hi tex ptr overflow at " + pos.ToString("X8"));
                }
                else
                {
                    br.Jump(ptrHi);

                    for (int i = 0; i < 4; i++)
                    {
                        hi[i] = new TextureLayout(br);
                    }
                }
            }
        }
    }
}