Shader "DawnOfMan/TreeTransitionBillboard" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_HueVariation ("Hue Variation", Vector) = (1,0.5,0,0.1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_VariationFactor ("Color variation multiplier", Range(0, 1)) = 1
		_FakeFacingNormals ("Fake Facing Normals", Range(0, 1)) = 0.5
		[MaterialEnum(None,0,Fastest,1)] _WindQuality ("Wind Quality", Range(0, 1)) = 0
		_Transition ("Transition", Range(0, 1)) = 0
		_MainTexTransition ("Base (RGB) Trans (A) Transition", 2D) = "white" {}
		_BumpMapTransition ("Normalmap Transition", 2D) = "bump" {}
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
	Fallback "Transparent/Cutout/VertexLit"
}