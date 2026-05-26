script.addColorParameter("Color Off", "", 0xffff0000);
script.addColorParameter("Color On", "", 0xffff0000);
script.addFloatParameter("Smooth", "", 0, 0, 1);
script.addBoolParameter("Force", "", false);
var progression = [];

function updateColors(colors, id, resolution, time, params, prop)//, num, speed, size, color, color2, pingPong, double)
{
	if (prop.buttons.button1 == undefined) return;
	var pressed = prop.buttons.button1.pressed.get();
	var targetProg = (pressed || params.force) ? 1 : 0;
	if(progression[id] == undefined) progression[id] = 0;
	progression[id] = progression[id] + (targetProg - progression[id]) / (params.smooth * 100);
	if (progression[id] >= .99) progression[id] = 1;
	if (progression[id] <= .01) progression[id] = 0;

	var color = colors.lerpColor(params.colorOff, params.colorOn, progression[id]);
	colors.fill(color);
}