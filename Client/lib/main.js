var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
var buttons = require('sdk/ui/button/action');
var pref = require('sdk/simple-prefs').prefs['On'];
On = pref;
pageMod.PageMod({
  include: "*",
  contentScriptFile: data.url("coms.js"),
  onAttach: function(worker) {
	if(On)
    worker.port.emit("getDoc");
	
  }
});


var button = buttons.ActionButton({
  id: "filter",
  label: "Thoth - Off",
  icon: {
    "16": "./icon-16(off).png",
    "32": "./icon-32(off).png",
    "64": "./icon-64(off).png"
  },
  onClick: handleClick
});
function handleClick(state) {


      On = !On;
	changeIcon();

   

  
}
function changeIcon()
{
	pref = On
	if(On)
	{
		button.label = "Thoth - On";
		button.icon = {
		    "16": "./icon-16.png",
		    "32": "./icon-32.png",
		    "64": "./icon-64.png"
		  };

	}
	else
	{
		button.label = "Thoth - Off";
		button.icon = {
		    "16": "./icon-16(off).png",
		    "32": "./icon-32(off).png",
		    "64": "./icon-64(off).png"
		  };

	}


}


changeIcon();
