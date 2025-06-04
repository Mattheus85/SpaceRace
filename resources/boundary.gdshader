shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
    vec2 coord = FRAGCOORD.xy / 100.0; // Scale for star spacing
    float noise = fract(sin(dot(coord, vec2(12.9898, 78.233))) * 43758.5453);
    COLOR = vec4(vec3(step(0.999, noise)), 1.0); // Fixed stars where noise > number
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
