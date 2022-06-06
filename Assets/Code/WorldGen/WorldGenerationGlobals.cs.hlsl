//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef WORLDGENERATIONGLOBALS_CS_HLSL
#define WORLDGENERATIONGLOBALS_CS_HLSL
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
// WorldGeneration.WorldGenerationGlobals:  static fields
//
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
