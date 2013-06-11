Shader "Custom/Transparent Unlit Diffuse" {

 

    Properties {

 

        _Color ("Main Color", Color) = (1,1,1,1)

 

        _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "white" {}

 

    }

 

 

 

    SubShader {

 

        Blend SrcAlpha OneMinusSrcAlpha

 

        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

 

        LOD 200

 

        

 

        Pass {

 

            CGPROGRAM

 

                #pragma vertex vert

 

                #pragma fragment frag

 

                #pragma multi_compile_builtin

 

                #pragma fragmentoption ARB_precision_hint_fastest

 

                

 

                #include "UnityCG.cginc"

 

                

 

                struct v2f

 

                {

 

                    float4  pos : SV_POSITION;

 

                    float2  uv : TEXCOORD0;

 

                }; 

 

 

 

                v2f vert (appdata_base v)

 

                {

 

                    v2f o;

 

                    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

 

                    o.uv = v.texcoord.xy;

 

                    return o;

 

                }

 

                

 

                sampler2D _MainTex;

 

                float4 _Color;

 

 

 

                fixed4 frag(v2f i) : COLOR

 

                {

 

                    fixed4 result = tex2D(_MainTex, i.uv) * _Color;

 

                    return result;

 

                }

 

            ENDCG

 

        }

 

    }

 

    FallBack "Transparent/Cutout/VertexLit"

 

}