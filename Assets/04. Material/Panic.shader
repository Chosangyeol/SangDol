Shader "Custom/Panic"
{
    Properties
    {
        // ⭐️ 유니티 UI(Canvas)가 에러를 뿜지 않도록 강제로 요구하는 더미 속성입니다.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} 
        
        _Color ("Fear Color", Color) = (0,0,0,1)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 1)) = 0.2
        _Softness ("Softness", Range(0, 1)) = 0.1
        _AspectRatio ("Aspect Ratio", Float) = 1.77
    }
    SubShader
    {
        // UI 컴포넌트용 필수 태그 셋업
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        ZTest [unity_GUIZTestMode] // UI가 다른 물체에 파묻히지 않게 함

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            // ⭐️ Properties에서 선언한 _MainTex를 받기 위한 변수 (사용은 안 함)
            sampler2D _MainTex; 
            
            float4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;
            float _AspectRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // UI Image 컴포넌트의 Color(알파값 등)를 받음
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 화면 비율 보정
                float2 centeredUV = i.uv - _Center.xy;
                centeredUV.x *= _AspectRatio;

                // 중심점으로부터의 거리 계산
                float dist = length(centeredUV);

                // 부드러운 경계선 만들기
                float alpha = smoothstep(_Radius, _Radius + _Softness, dist);

                // 최종 색상 계산 (에디터에 지정한 색상에 알파값 적용)
                fixed4 finalColor = fixed4(_Color.rgb, _Color.a * alpha);
                
                // Image 컴포넌트 자체의 Color 값(i.color)도 곱해줘서 
                // 스크립트로 투명도 조절 시 정상 작동하게 만듦
                return finalColor * i.color; 
            }
            ENDCG
        }
    }
}
