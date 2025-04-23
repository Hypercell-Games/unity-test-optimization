Shader "Unlit/HoleShader"
{
    
    SubShader
    {
        Tags {"Queue" = "Geometry+1"}


        Pass
        {
            ZWrite On           // Записываем глубину
            ColorMask 0     
        }
    }
}
