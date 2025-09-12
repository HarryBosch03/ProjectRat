half3 rgbToOklab(half3 rgb)
{
    half3 lms;
    lms.r = 0.4122214708 * rgb.r + 0.5363325363 * rgb.g + 0.0514459929 * rgb.b;
    lms.g = 0.2119034982 * rgb.r + 0.6806995451 * rgb.g + 0.1073969566 * rgb.b;
    lms.b = 0.0883024619 * rgb.r + 0.2817188376 * rgb.g + 0.6299787005 * rgb.b;

    
    // Math.crb (cube root) here is the equivalent of the C++ cbrtf function here: https://bottosson.github.io/posts/oklab/#converting-from-linear-srgb-to-oklab
    lms = pow(lms, 1.0 / 3.0);
    
    half3 lab;
    lab.r = lms.r * +0.2104542553 + lms.g * +0.7936177850 + lms.b * -0.0040720468;
    lab.g = lms.r * +1.9779984951 + lms.g * -2.4285922050 + lms.b * +0.4505937099;
    lab.b = lms.r * +0.0259040371 + lms.g * +0.7827717662 + lms.b * -0.8086757660;

    return lab;
}

half3 oklabToRgb(half3 lab)
{
    half3 lms;
    
    lms.r = lab.r + lab.g * +0.3963377774 + lab.b * +0.2158037573;
    lms.g = lab.r + lab.g * -0.1055613458 + lab.b * -0.0638541728;
    lms.b = lab.r + lab.g * -0.0894841775 + lab.b * -1.2914855480;

    lms = lms * lms * lms;

    half3 rgb;
    
    rgb.r = lms.r * +4.0767416621 + lms.g * -3.3077115913 + lms.b * +0.2309699292;
    rgb.g = lms.r * -1.2684380046 + lms.g * +2.6097574011 + lms.b * -0.3413193965;
    rgb.b = lms.r * -0.0041960863 + lms.g * -0.7034186147 + lms.b * +1.7076147010;
    
    return rgb;
}