Shader "DawnOfMan/SkydomeTitle" {
	Properties {
		_SkyTexture ("SkyTexture", 2D) = "white" {}
		_TimeStretch ("Time Stretch", Range(0, 2)) = 0.05
		_SunDir ("Sun Direction", Vector) = (0,0,-1,1)
		_SunSize ("Sun Size", Range(0.1, 5)) = 0.8
		_SunColor ("Sun Color", Vector) = (1,1,0.9,1)
		_HorizonColor ("HorizonColor", Vector) = (1,0,0,1)
		_SkyColor ("SkyColor", Vector) = (0,0,1,1)
		_GradientFactor ("Gradient Factor", Float) = 1
		_GradientOffset ("Gradient Offset", Float) = 0
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