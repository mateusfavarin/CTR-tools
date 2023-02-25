﻿using CTRFramework.Shared;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CTRFramework
{
    public class Vertex : IReadWrite
    {
        public static readonly int SizeOf = 16;

        public Vector3 Position;
        public Vector4b Color;
        public Vector4b MorphColor;

        public Vector4b color_target;
        public Vector2 uv; //not used by CTR, added for convenience

        public Vertex Clone()
        {
            return new Vertex()
            {
                Position = Position,
                Color = Color,
                MorphColor = MorphColor,
                color_target = color_target,
                uv = uv
            };
        }

        public Vertex()
        {
        }

        public Vertex(BinaryReaderEx br)
        {
            Read(br);
        }

        public void SetColor(Vector4b col, Vcolor mode)
        {
            switch (mode)
            {
                case Vcolor.Default: Color = col; break;
                case Vcolor.Morph: MorphColor = col; break;
            }
        }

        public virtual void Read(BinaryReaderEx br)
        {
            Position = br.ReadVector3s(1 / 100f);
            //here's the deal, this value is always 0 in release files, but it was figured out it's some mode ranging from 0 to 4.
            short value = br.ReadInt16();
            Color = new Vector4b(br);
            MorphColor = new Vector4b(br);
        }

        public void Write(BinaryWriterEx bw, List<UIntPtr> patchTable = null)
        {
            bw.WriteVector3sPadded(Position, 1 / 100f);
            Color.Write(bw);
            MorphColor.Write(bw);
        }

        public string ToObj(float scale = 1.0f)
        {
            return $"v\t{(Position.X * scale).ToString("0.#####")} {(Position.Y * scale).ToString("0.#####")} {(Position.Z * scale).ToString("0.#####")}\t{(Color.X / 255f).ToString("0.###")} {(Color.Y / 255f).ToString("0.###")} {(Color.Z / 255f).ToString("0.###")}";
        }
    }

    public class VertexShort : Vertex
    {
        public VertexShort(BinaryReaderEx br) : base(br)
        {
        }

        public override void Read(BinaryReaderEx br)
        {
            Position = br.ReadVector3sPadded(1 / 100f);
            Color = new Vector4b(br);
        }
    }
}