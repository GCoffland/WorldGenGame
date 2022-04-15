//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef WORLDGENERATION_CS_HLSL
#define WORLDGENERATION_CS_HLSL
//
// WorldGeneration.DIRECTION:  static fields
//
#define DIRECTION_X_NEG (0)
#define DIRECTION_X_POS (1)
#define DIRECTION_Y_NEG (2)
#define DIRECTION_Y_POS (3)
#define DIRECTION_Z_NEG (4)
#define DIRECTION_Z_POS (5)

//
// WorldGeneration.VOXELTYPE:  static fields
//
#define VOXELTYPE_NONE (0)
#define VOXELTYPE_DIRT (1)
#define VOXELTYPE_GRASS (2)

//
// WorldGeneration.Constant:  static fields
//
#define NUM (5)

//
// WorldGeneration.Constants:  static fields
//
#define TNF (0.25)
#define MAX_RENDER_DISTANCE (100)
#define MAX_ACTIVE_CHUNK_COUNT (1000)

// Generated from WorldGeneration.VertexBufferStruct
// PackingRules = Exact
struct VertexBufferStruct
{
    float3 position;
    float3 normal;
    float2 texCoord;
};


#endif
