﻿using CTRFramework;
using CTRFramework.Shared;

namespace levTool
{
    class TexMap : IRead
    {
        public string name; //16 bytes
        public uint id;
        public TextureLayout tl;

        public TexMap()
        {
        }
        public TexMap(BinaryReaderEx br)
        {
            Read(br);
        }
        public void Read(BinaryReaderEx br)
        {
            name = System.Text.Encoding.ASCII.GetString(br.ReadBytes(16)).Split('\0')[0];
            id = br.ReadUInt32();
            tl = new TextureLayout(br);
        }
    }
}