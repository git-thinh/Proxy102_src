
//Mozilla/5.0 (Linux) AppleWebKit/537.36 (KHTML, like Gecko) ChromiumBrowser/1.1 Chrome/67.0.3396.79 Safari/537.36 (Custom6.00.1101.01001.)
var _IS_SIM = navigator.userAgent.indexOf('(Custom6') != -1;

function proxy_SIM() {
    console.log('PROXY ON SIM ---------------------------------------------------');

    window.pertest = {
        isLogging: false,
        __queue: [],
        streamSwitch: false,
        on: function () {
        },
        off: function () {
        },
        log: function (uuid) {
        },
        chunks: function (uuid, arry) {
        },
        stream: function () {
        }
    };

    logSocketInit();
}

function proxy_PC() {
    console.log('PROXY ON PC ---------------------------------------------------');
    //logSocketInit();
}

var _WS = null;
var _LOG_AVTIVE = false;
function logSocketInit() {
    if ("WebSocket" in window) {
        alert("WebSocket is supported by your Browser!");

        // Let us open a web socket
        _WS = new WebSocket("ws://192.168.56.101:8181");

        _WS.onopen = function () {
            _LOG_AVTIVE = true;
            //// Web Socket is connected, send data using send()
            //ws.send("Message to send");
            //alert("Message is sent...");
        };

        //_WS.onmessage = function (evt) {
        //    var received_msg = evt.data;
        //    alert("Message is received...");
        //};

        _WS.onclose = function () {
            _LOG_AVTIVE = false;
            //// websocket is closed.
            //alert("Connection is closed...");
        };
    } 

    console.log = function (...args) {
        //console.log(arguments[i]);
        if (_LOG_AVTIVE && _WS) {
            _WS.send('\r\n' + JSON.stringify(args));
        }
    };
}
 


if (_IS_SIM) proxy_SIM(); else proxy_PC();

