Shader "Hidden/DawnOfMan/TerrainTransitionAddPass" {
	Properties {
		[HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
		[HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		[HideInInspector] _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		[HideInInspector] _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		[HideInInspector] _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		[HideInInspector] [Gamma] _Metallic0 ("Metallic 0", Range(0, 1)) = 0
		[HideInInspector] [Gamma] _Metallic1 ("Metallic 1", Range(0, 1)) = 0
		[HideInInspector] [Gamma] _Metallic2 ("Metallic 2", Range(0, 1)) = 0
		[HideInInspector] [Gamma] _Metallic3 ("Metallic 3", Range(0, 1)) = 0
		[HideInInspector] _Smoothness0 ("Smoothness 0", Range(0, 1)) = 1
		[HideInInspector] _Smoothness1 ("Smoothness 1", Range(0, 1)) = 1
		[HideInInspector] _Smoothness2 ("Smoothness 2", Range(0, 1)) = 1
		[HideInInspector] _Smoothness3 ("Smoothness 3", Range(0, 1)) = 1
		[HideInInspector] _TransitionSplat3 ("Transition Layer 3 (A)", 2D) = "white" {}
		[HideInInspector] _TransitionSplat2 ("Transition Layer 2 (B)", 2D) = "white" {}
		[HideInInspector] _TransitionSplat1 ("Transition Layer 1 (G)", 2D) = "white" {}
		[HideInInspector] _TransitionSplat0 ("Transition Layer 0 (R)", 2D) = "white" {}
		[HideInInspector] _Transition ("Transition", Range(0, 1)) = 1
		[HideInInspector] _NormalTransition0 ("Normal Transition0", Range(0, 1)) = 1
		[HideInInspector] _NormalTransition1 ("Normal Transition1", Range(0, 1)) = 1
		[HideInInspector] _NormalTransition2 ("Normal Transition2", Range(0, 1)) = 1
		[HideInInspector] _NormalTransition3 ("Normal Transition3", Range(0, 1)) = 1
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
	Fallback "Hidden/TerrainEngine/Splatmap/Diffuse-AddPass"
}