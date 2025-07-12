Shader "DawnOfMan/SkyProcedural" {
	Properties {
		[KeywordEnum(None, Simple, High Quality)] _SunDisk ("Sun", Float) = 2
		_SunSize ("Sun Size", Range(0, 1)) = 0.04
		_AtmosphereThickness ("Atmosphere Thickness", Range(0, 5)) = 1
		_SkyTint ("Sky Tint", Vector) = (0.5,0.5,0.5,1)
		_GroundColor ("Ground", Vector) = (0.369,0.349,0.341,1)
		_Exposure ("Exposure", Range(0, 8)) = 1.3
		_CloudsTex ("Clouds Texture", 2D) = "black" {}
		_CloudsTiling ("Clouds tiling", Range(1, 10)) = 1
		_TimeStretch ("Time Stretch", Range(0, 2)) = 0.04
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
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

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
}