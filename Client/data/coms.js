self.port.on("getDoc", function() {
    var doc = document.body.innerHTML;
    var wsImpl = window.WebSocket || window.MozWebSocket;
    var localws = new wsImpl('ws://127.0.0.1:8181');
    var globalws = new wsImpl('ws://nodejs-projectthoth.rhcloud.com:8000');
    sent = false;
    recieved = false;
    localws.onopen = function(e) {
        if (sent == false) {
            sent = true;
            localws.send(doc);
            console.log("Sent to local server");
        }
    };
    localws.onmessage = function(e) {
        if (recieved == false) {
            recieved = true;
            document.body.innerHTML = e.data;
            console.log("Got from local server");            
        }
        localws.close();
    }

    globalws.onopen = function(e) {
        if (sent == false) {
            sent = true;
            globalws.send(doc);
            console.log("Sent to global server");            
        }
    };
    globalws.onmessage = function(e) {
        if (recieved == false) {
            recieved = true;
            document.body.innerHTML = e.data;
            console.log("Got from global server");            
        }
        globalws.close();
    }



});
