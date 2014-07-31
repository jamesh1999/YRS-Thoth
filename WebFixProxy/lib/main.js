var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
var buttons = require('sdk/ui/button/action');
On = false;
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
  label: "Universal Web Translator for Grammar Nazis - Off",
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
	if(On)
	{
		button.label = "Universal Web Translator for Grammar Nazis - On";
		button.icon = {
		    "16": "./icon-16.png",
		    "32": "./icon-32.png",
		    "64": "./icon-64.png"
		  };

	}
	else
	{
		button.label = "Universal Web Translator for Grammar Nazis - Off";
		button.icon = {
		    "16": "./icon-16(off).png",
		    "32": "./icon-32(off).png",
		    "64": "./icon-64(off).png"
		  };

	}


}


