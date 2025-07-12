Shader "DawnOfMan/Water" {
	Properties {
		_Color ("Water Color", Vector) = (1,1,1,1)
		_Specular ("Specular", Vector) = (1,1,1,1)
		_WaveSize ("WaveSize", Range(0.1, 5)) = 1
		_BorderWaveFrequency ("BorderWaveFrequency", Range(1, 100)) = 10
		_BorderWaveScaleNormal ("BorderWaveScaleNormal", Range(0, 10)) = 0.375
		_BorderWaveScaleEmission ("BorderWaveScaleEmission", Range(0, 10)) = 1.5
		_BaseAlpha ("BaseAlpha", Range(0, 1)) = 0.9
		_BorderAlpha ("BorderAlpha", Range(0, 1)) = 0.7
		_BorderAlphaDistance ("BorderAlphaDistance", Range(-1, 1)) = 0
		_Emmission ("Emission", Range(0, 1)) = 1
		[NoScaleOffset] _BorderDirectionTex ("Border (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BumpMap ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _ReflectionCube ("Reflection Cube", Cube) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_MatrixMVP;

			struct Vertex_Stage_Input
			{
				float3 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixMVP, float4(input.pos, 1.0));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
}