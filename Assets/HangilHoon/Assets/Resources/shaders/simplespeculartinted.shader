Shader "DawnOfMan/SimpleSpecularTinted" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal (RGB)", 2D) = "bump" {}
		_SpecGlossMap ("Specular (RGB=Specular, A=Smoothness)", 2D) = "black" {}
		_TintMap ("Tint (R=Skin/Shield1, G=Hair/Shield2, B=Outfit1, A=Outfit2)", 2D) = "black" {}
		_TintColor1 ("Skin/Shield1", Vector) = (1,1,1,1)
		_TintColor2 ("Hair/Shield2", Vector) = (0,0,0,1)
		_TintColor3 ("Outfit1", Vector) = (1,0,0,1)
		_TintColor4 ("Outfit2", Vector) = (0,1,0,1)
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

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, float2(input.uv.x, input.uv.y)) * _Color;
			}

			ENDHLSL
		}
	}
	Fallback "VertexLit"
}