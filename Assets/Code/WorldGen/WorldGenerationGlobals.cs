using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime;
using System.Linq;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using System.Threading.Tasks.Dataflow;

namespace WorldGeneration
{
    [GenerateHLSL(PackingRules.Exact, false)]
    public struct VertexBufferStruct
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texCoord;
    }

    [GenerateHLSL(PackingRules.Exact, false)]
    public static class WorldGenerationGlobals
    {
        public const string blockTexturePath = "BlockData/";

        public static readonly Texture2D atlas;
        public static readonly Vector2 TNF;
        public static readonly Dictionary<int, RuntimeBlockData> blockData = new Dictionary<int, RuntimeBlockData>();
        public static readonly Dictionary<string, int> blockIdByName = new Dictionary<string, int>();

        static WorldGenerationGlobals()
        {
            BlockData[] data = Resources.LoadAll<BlockData>(blockTexturePath);

            List<Cubemap> maps = new List<Cubemap>();
            for(int i = 0; i < data.Length; i++)
            {
                if(data[i].Cubemap != null)
                {
                    maps.Add(data[i].Cubemap);
                }
            }

            int width = 0;
            int height = 0;
            foreach (Cubemap c in maps)
            {
                width += c.width;
                height = Math.Max(height, c.height * 6);
            }

            atlas = new Texture2D(width, height, DefaultFormat.LDR, TextureCreationFlags.None);
            Rect[][] rects = PackCubemaps(atlas, maps.ToArray());

            int k = 0;
            for(int i = 0; i < data.Length; i++)
            {
                RuntimeBlockData temp = new RuntimeBlockData()
                {
                    blockData = data[i]
                };
                if (data[i].Cubemap != null)
                {
                    temp.atlasRect = rects[k++];
                }
                blockData.Add(data[i].Id, temp);
                blockIdByName.Add(data[i].Name, data[i].Id);
            }

            TNF = new Vector2(1f / maps.Count, 1f / 6f);

            Debug.Log("Initialized constants");
        }

        private static Rect[][] PackCubemaps(Texture2D tex, Cubemap[] to_pack)
        {
            Rect[][] rects = new Rect[to_pack.Length][];
            Vector2 current_pos = Vector2.zero;
            for (int i = 0; i < to_pack.Length; i++)
            {
                rects[i] = new Rect[6];
                for (int j = 0; j < 6; j++)
                {
                    rects[i][j] = new Rect(current_pos + (j * Vector2Int.up * to_pack[i].height) * tex.texelSize,
                        new Vector2Int(to_pack[i].width, to_pack[i].height) * tex.texelSize);
                    Color[] pixels = to_pack[i].GetPixels((CubemapFace)j);
                    pixels.ReverseInGroups(to_pack[i].width);
                    tex.SetPixels((int)(rects[i][j].position.x / tex.texelSize.x),
                        (int)(rects[i][j].position.y / tex.texelSize.y),
                        (int)(rects[i][j].size.x / tex.texelSize.x),
                        (int)(rects[i][j].size.y / tex.texelSize.y),
                        pixels);
                }
                current_pos += new Vector2(rects[i][0].size.x, 0);
            }
            tex.Apply();
            return rects;
        }

        public struct RuntimeBlockData
        {
            public BlockData blockData;
            public Rect[] atlasRect;
        }

        public const float MaxRenderDistance = 100f;

        public const int MaxActiveChunkCount = 1000;

        public static readonly VertexAttributeDescriptor[] VertexBufferLayout = new VertexAttributeDescriptor[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

        public static readonly Vector3Int ChunkSize = new Vector3Int()
        {
            x = 64,
            y = 64,
            z = 64
        };

        public static int[] ChunkSideAreas = new int[6]
        {
            ChunkSize.y * ChunkSize.z,
            ChunkSize.y * ChunkSize.z,
            ChunkSize.x * ChunkSize.z,
            ChunkSize.x * ChunkSize.z,
            ChunkSize.y * ChunkSize.x,
            ChunkSize.y * ChunkSize.x,
        };

        public static int BlockMapLength
        {
            get
            {
                return (ChunkSize.x + 2) * (ChunkSize.y + 2) * (ChunkSize.z + 2);
            }
        }


        public static int MaxPossibleVerticies
        {
            get
            {
                return (ChunkSize.x * ChunkSize.y * ChunkSize.z) / 2;
            }
        }
    }
}