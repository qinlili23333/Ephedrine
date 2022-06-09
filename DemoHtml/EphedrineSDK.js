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
        StartService: async(elevated) => {
            switch (await Ephedrine.msgAsync("StartService", elevated ? "Admin" : "User")) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
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
                            code: 90,
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
                            code: 91,
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
                            code: 93,
                            success: false,
                            running: true,
                            msg: "Already Running"
                        }
                        break;
                    }
            }
        },
        Install: {
            zip: async(link, location) => {
                switch (await Ephedrine.msgAsync("Install", "zip", link, location)) {
                    case -1:
                        {
                            //Busy
                            return {
                                code: -1,
                                success: false,
                                msg: "Client Busy"
                            }
                            break;
                        };
                    case 10:
                        {
                            //Zip Patch Fail
                            return {
                                code: 10,
                                success: false,
                                msg: "Success"
                            }
                            break;
                        };
                    case 11:
                        {
                            //Success Zip Patch
                            return {
                                code: 11,
                                success: true,
                                msg: "Success"
                            }
                            break;
                        };
                    case 13:
                        {
                            //Zip Patch Permission Denied
                            return {
                                code: 13,
                                success: false,
                                msg: "Permission Denied"
                            }
                            break;
                        }
                }
            },
            save: async(link, name) => {
                switch (await Ephedrine.msgAsync("Install", "save", link, name)) {
                    case -1:
                        {
                            //Busy
                            return {
                                code: -1,
                                success: false,
                                msg: "Client Busy"
                            }
                            break;
                        };
                    case 15:
                        {
                            //Download Success Without Verification
                            return {
                                code: 15,
                                success: true,
                                msg: "Success With No Verification"
                            }
                            break;
                        };
                }
            }
        },
        KillProcess: async(name) => {
            switch (await Ephedrine.msgAsync("KillProcess", name.replace(".exe", ""))) {
                case -1:
                    {
                        //Busy
                        return {
                            code: -1,
                            success: false,
                            msg: "Client Busy"
                        }
                        break;
                    };
                case 70:
                    {
                        //Nothing To Kill 
                        return {
                            code: 70,
                            success: false,
                            running: false,
                            msg: "Nothing Found"
                        }
                        break;
                    };
                case 71:
                    {
                        //Kill Success 
                        return {
                            code: 71,
                            success: true,
                            running: false,
                            msg: "Success"
                        }
                        break;
                    }

            }
        }
    }
}