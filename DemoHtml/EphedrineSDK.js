window.Ephedrine = {
    msgStatus: code => {
        console.log(code);
        Ephedrine.processCode(code);
    },
    request: (action, arg1, arg2, arg3, arg4, arg5) => {
        window.chrome.webview.postMessage({ Action: action, Arg1: arg1, Arg2: arg2, Arg3: arg3, Arg4: arg4, Arg5: arg5 });
    },
    processCode: code => {},
    msgAsync: (action, arg1, arg2, arg3, arg4, arg5) => {
        return new Promise(resolve => {
            Ephedrine.request(action, arg1, arg2, arg3, arg4, arg5);
            Ephedrine.processCode = code => {
                resolve(code);
            }
        })
    },
    Actions: {
        StartService: async() => {
            switch (await Ephedrine.msgAsync("StartService")) {
                case -1:
                    {
                        //Busy
                        return {
                            success: false,
                            running: false,
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 90:
                    {
                        //Start Fail 
                        return {
                            success: false,
                            running: false,
                            msg: "Fail"
                        }
                        break;
                    };
                case 91:
                    {
                        //Start Success 
                        return {
                            success: true,
                            running: true,
                            msg: "Success"
                        }
                        break;
                    };
                case 93:
                    {
                        //Start Fail 
                        return {
                            success: false,
                            running: true,
                            msg: "Already Running"
                        }
                        break;
                    }
            }
        }
    }
}