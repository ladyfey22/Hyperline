#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(text, 0);
uniform float hue;


float3 HUEtoRGB(in float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

float3 HSVtoRGB(in float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float3 GetHSVColor(float2 pos)
{
    float3 input = float3(hue, pos.x, pos.y);
    return HSVtoRGB(input);
}

float4 PS_HSV(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float2 hs = float2(uv.x, 1.0 - uv.y); //invert the y, since value at the bottom should be zero
    float3 rgb = GetHSVColor(hs);

    float r = rgb.x;
    float g = rgb.y;
    float b = rgb.z;
    float4 returnV = float4(r,g,b,1.0);
    return returnV;
}

technique Rainbow
{
    pass pass0
    {
        PixelShader = compile ps_2_0 PS_HSV();
    }
}