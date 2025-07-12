Shader "DawnOfMan/Tree" {
	Properties {
		_Color ("Main Color", Vector) = (1,1,1,1)
		_GeometryColor ("Geometry Color", Vector) = (1,1,1,1)
		_HueVariation ("Hue Variation", Vector) = (1,0.5,0,0.1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_DetailTex ("Detail", 2D) = "black" {}
		_AmbientOcclussionFactor ("Ambient Occlussion Factor", Range(0, 1)) = 1
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.333
		_FakeFacingNormals ("Fake Facing Normals", Range(0, 1)) = 0
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Float) = 2
		[MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality ("Wind Quality", Range(0, 5)) = 0
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
             float2 uv : TEXCOORD0; // <--- 이 줄 추가! (버텍스 입력에 UV 추가)
          };

          struct Vertex_Stage_Output
          {
             float4 pos : SV_POSITION;
             float2 uv : TEXCOORD0; // <--- 이 줄 추가! (버텍스 출력에 UV 추가)
          };

          Vertex_Stage_Output vert(Vertex_Stage_Input input)
          {
             Vertex_Stage_Output output;
             output.pos = mul(unity_MatrixMVP, float4(input.pos, 1.0));
             output.uv = input.uv; // <--- 이 줄 추가! (입력 UV를 출력 UV로 복사)
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
	//CustomEditor "SpeedTreeMaterialInspector"
}