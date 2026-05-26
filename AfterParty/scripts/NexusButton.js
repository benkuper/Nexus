script.addColorParameter("Color Off", "", 0xffff0000);
script.addColorParameter("Color On", "", 0xffff0000);
script.addFloatParameter("Smooth", "", 0, 0, 1);
script.addBoolParameter("Force", "", false);
var progression = 0;

function updateColors(colors, id, resolution, time, params, prop)//, num, speed, size, color, color2, pingPong, double)
{
	if (prop.buttons.button1 == undefined) return;
	var pressed = prop.buttons.button1.pressed.get();

	var targetProg = (pressed || params.force) ? 1 : 0;
	progression = progression + (targetProg - progression) / (params.smooth*100);
	if (progression >= .99) progression = 1;
	if (progression <= .01) progression = 0;

	var color = colors.lerpColor(params.colorOff, params.colorOn, progression);
	colors.fill(color);
}