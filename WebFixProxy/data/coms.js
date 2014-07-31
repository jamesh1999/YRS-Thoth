self.port.on("getDoc", function() {
  var doc = document.body.innerHTML;
  var wsImpl = window.WebSocket || window.MozWebSocket;
  var ws = new wsImpl('ws://0.0.0.0:8181/');
  ws.onopen = function (e) {
    ws.send(doc);
};
 ws.onmessage = function(e){
   document.body.innerHTML = e.data;
   console.log(e.data);
}

  
});





