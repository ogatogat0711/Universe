Shader "Unlit/DistortionUnlit"
{
    Properties
    {
        _NoiseTex("Noise Texture (Gray Scale)", 2D) = "white" {}
        _DistortionStrength("Distortion Strength", Range(0, 1)) = 0.1
        _ScrollSpeed("Noise Scroll Speed", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        GrabPass
        {
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            Lighting Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            //Propertiesで宣言した変数を取ってくる
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _DistortionStrength;
            float _ScrollSpeed;

            //GrabPassで取得したテクスチャ
            sampler2D _GrabTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;//Particle Systemの色情報を受け取る
            };

            struct v2f
            {
                float2 uv : TEXCOORD1;
                float4 grabPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;//頂点カラー情報そのまま
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //GrabPass用のスクリーン座標
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //UVは時間でスクロールする
                float2 scrolledUV = i.uv;
                scrolledUV.y += _Time.y * _ScrollSpeed;

                float noiseValue = tex2D(_NoiseTex,scrolledUV).b;

                float2 distortionVector = float2(noiseValue - 0.5, noiseValue - 0.5) * _DistortionStrength;
                distortionVector *= i.color.a;//歪みに頂点カラーのAlphaをかけることでParticleの寿命で歪みが弱まる

                i.grabPos.xy += distortionVector;//サンプリング座標をずらす

                fixed4 col = tex2Dproj(_GrabTexture, i.grabPos);
                col.a = i.color.a;

                return col;
            }
            ENDCG
        }
    }
}
