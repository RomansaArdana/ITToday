// ============================================================
// SanityPostProcess.shader
// URP Full Screen Pass Renderer Feature compatible shader.
//
// Tujuan desain:
// - Mempertahankan warna asli game.
// - Heartbeat hanya muncul sebagai vignette tipis di pinggir.
// - Blur, grayscale, distortion, grain, dan scanline dibuat ringan.
// - Tidak ada full-screen color tint dari shader.
//
// Catatan:
// - _BlitTexture sudah disediakan oleh Blit.hlsl.
// - Jangan deklarasikan _BlitTexture / sampler_BlitTexture lagi.
// ============================================================

Shader "Custom/SanityPostProcess"
{
    Properties
    {
        _Blur
        (
            "Blur Strength",
            Range(0, 5)
        ) = 0.0

        _Grayscale
        (
            "Grayscale",
            Range(0, 1)
        ) = 0.0

        _PulseStrength
        (
            "Heartbeat Strength",
            Range(0, 0.5)
        ) = 0.0

        _PulseSpeed
        (
            "Heartbeat Speed",
            Range(0, 2)
        ) = 0.0

        _Distortion
        (
            "Distortion",
            Range(0, 0.1)
        ) = 0.0

        _Noise
        (
            "Noise",
            Range(0, 0.2)
        ) = 0.0

        _ScanlineIntensity
        (
            "Scanline Intensity",
            Range(0, 1)
        ) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "SanityPostProcess"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)

                float _Blur;
                float _Grayscale;

                float _PulseStrength;
                float _PulseSpeed;

                float _Distortion;
                float _Noise;
                float _ScanlineIntensity;

            CBUFFER_END


            // ============================================================
            // RANDOM / GRAIN
            // ============================================================

            float Hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);

                return frac(p.x * p.y);
            }


            // ============================================================
            // BLUR
            // ============================================================

            float3 GaussianBlur(float2 uv)
            {
                // Jika blur hampir 0, langsung ambil pixel asli.
                if (_Blur < 0.001)
                {
                    return SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_LinearClamp,
                        uv
                    ).rgb;
                }

                // Sangat kecil supaya blur tetap lembut.
                float2 texel = _Blur * 0.00075;

                static const float kernel[9] =
                {
                    0.0625,
                    0.1250,
                    0.0625,

                    0.1250,
                    0.2500,
                    0.1250,

                    0.0625,
                    0.1250,
                    0.0625
                };

                static const float2 offsets[9] =
                {
                    float2(-1, -1),
                    float2( 0, -1),
                    float2( 1, -1),

                    float2(-1,  0),
                    float2( 0,  0),
                    float2( 1,  0),

                    float2(-1,  1),
                    float2( 0,  1),
                    float2( 1,  1)
                };

                float3 result = 0.0;

                for (int i = 0; i < 9; i++)
                {
                    float2 sampleUV =
                        saturate(
                            uv +
                            offsets[i] * texel
                        );

                    result +=
                        SAMPLE_TEXTURE2D_X(
                            _BlitTexture,
                            sampler_LinearClamp,
                            sampleUV
                        ).rgb * kernel[i];
                }

                return result;
            }


            // ============================================================
            // CHROMATIC ABERRATION
            // ============================================================

            float3 ChromaticAberration(
                float2 uv,
                float amount
            )
            {
                if (amount < 0.0001)
                {
                    return GaussianBlur(uv);
                }

                float2 dir =
                    (uv - 0.5) *
                    amount *
                    2.0;

                float r =
                    GaussianBlur(
                        uv + dir * 1.15
                    ).r;

                float g =
                    GaussianBlur(
                        uv
                    ).g;

                float b =
                    GaussianBlur(
                        uv - dir * 1.15
                    ).b;

                return float3(r, g, b);
            }


            // ============================================================
            // SOFT EDGE VIGNETTE
            //
            // Heartbeat hanya bekerja di pinggir.
            // Tengah layar tetap relatif bersih.
            // ============================================================

            float SoftEdgeVignette(
                float2 uv,
                float pulse
            )
            {
                float2 d =
                    (uv - 0.5) *
                    2.0;

                // Aspect ratio correction.
                d.x *=
                    _ScreenParams.x /
                    max(_ScreenParams.y, 1.0);

                float dist =
                    length(d);

                // Area pengaruh hanya di dekat tepi.
                float edge =
                    smoothstep(
                        0.72,
                        1.38,
                        dist
                    );

                // Pulse tetap sangat tipis.
                float strength =
                    _PulseStrength *
                    lerp(
                        0.35,
                        1.0,
                        pulse
                    );

                return
                    1.0 -
                    edge *
                    strength;
            }


            // ============================================================
            // SCANLINE
            // ============================================================

            float Scanline(
                float2 uv,
                float intensity
            )
            {
                if (intensity < 0.001)
                {
                    return 1.0;
                }

                float scanPos =
                    frac(
                        uv.y * 120.0 +
                        _Time.y * 0.15
                    );

                float scan =
                    smoothstep(
                        0.0,
                        0.35,
                        scanPos
                    ) *
                    smoothstep(
                        1.0,
                        0.65,
                        scanPos
                    );

                return
                    1.0 -
                    intensity *
                    (1.0 - scan) *
                    0.55;
            }


            // ============================================================
            // GRAIN
            // ============================================================

            float3 GrainNoise(
                float3 color,
                float2 uv,
                float intensity
            )
            {
                if (intensity < 0.001)
                {
                    return color;
                }

                float grain =
                    Hash(
                        uv +
                        frac(_Time.y * 0.731)
                    );

                grain =
                    (grain - 0.5) *
                    intensity;

                return color + grain;
            }


            // ============================================================
            // FRAGMENT
            // ============================================================

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv =
                    input.texcoord;


                // --------------------------------------------------------
                // 1. SUBTLE WAVE DISTORTION
                // --------------------------------------------------------

                if (_Distortion > 0.001)
                {
                    float waveX =
                        sin(
                            uv.y * 18.0 +
                            _Time.y * 1.3
                        ) *
                        _Distortion *
                        0.30;

                    float waveY =
                        cos(
                            uv.x * 14.0 +
                            _Time.y * 0.9
                        ) *
                        _Distortion *
                        0.18;

                    uv =
                        saturate(
                            uv +
                            float2(
                                waveX,
                                waveY
                            )
                        );
                }


                // --------------------------------------------------------
                // 2. BASE IMAGE
                // --------------------------------------------------------

                float3 color =
                    ChromaticAberration(
                        uv,
                        _Distortion
                    );


                // --------------------------------------------------------
                // 3. DESATURATION
                // --------------------------------------------------------

                if (_Grayscale > 0.001)
                {
                    float luminance =
                        dot(
                            color,
                            float3(
                                0.2126,
                                0.7152,
                                0.0722
                            )
                        );

                    color =
                        lerp(
                            color,
                            float3(
                                luminance,
                                luminance,
                                luminance
                            ),
                            _Grayscale
                        );
                }


                // --------------------------------------------------------
                // 4. SCANLINES
                // --------------------------------------------------------

                color *=
                    Scanline(
                        uv,
                        _ScanlineIntensity
                    );


                // --------------------------------------------------------
                // 5. GRAIN
                // --------------------------------------------------------

                color =
                    GrainNoise(
                        color,
                        uv,
                        _Noise
                    );


                // --------------------------------------------------------
                // 6. HEARTBEAT VIGNETTE
                //
                // Tidak mengubah seluruh layar.
                // Hanya pinggir layar yang berdenyut.
                // --------------------------------------------------------

                if (
                    _PulseStrength > 0.001 &&
                    _PulseSpeed > 0.001
                )
                {
                    float pulse =
                        (
                            sin(
                                _Time.y *
                                _PulseSpeed *
                                3.14159265
                            ) +
                            1.0
                        ) *
                        0.5;

                    // Soft heartbeat curve.
                    pulse =
                        smoothstep(
                            0.0,
                            1.0,
                            pulse
                        );

                    color *=
                        SoftEdgeVignette(
                            uv,
                            pulse
                        );
                }


                // --------------------------------------------------------
                // 7. OUTPUT
                // --------------------------------------------------------

                return float4(
                    saturate(color),
                    1.0
                );
            }

            ENDHLSL
        }
    }

    Fallback Off
}