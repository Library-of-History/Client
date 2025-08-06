Shader "CustomRenderTexture/Stencil Portal"
{
    Properties
    {
        [IntRange] _StencilID("Stencil ID",Range(0,255)) = 0
    }
   SubShader
   {
      Tags
      {
          "Queue" = "Geometry-1"
      }
      Pass
      {
          ZWrite Off
          ColorMask 0
          Cull Front
          Stencil
          {
              Ref [_StencilID]
              Comp Always
              
              Pass Replace
          }
      }
   }
}
