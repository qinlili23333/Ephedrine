window.Ephedrine = {
    msgStatus: code => {
        console.log(code);
        Ephedrine.processCode(code);
    },
    msgResult: result => {
        console.log(result);
        Ephedrine.processCode(code);
    },
    request: (action, arg1, arg2, arg3, arg4, arg5) => {
        window.chrome.webview.postMessage({ Action: action, Arg1: arg1, Arg2: arg2, Arg3: arg3, Arg4: arg4, Arg5: arg5 });
    },
    processCode: code => {},
    processResult: result => {},
    msgAsync: (action, arg1, arg2, arg3, arg4, arg5, forResult) => {
        return new Promise(resolve => {
            Ephedrine.request(action, arg1, arg2, arg3, arg4, arg5);
            if (forResult) {
                let result1 = "";
                let code1 = 0;
                const triggerCheck = () => {
                    if (!(result1 === "") && !(code1 === 0)) {
                        resolve({
                            result: result1,
                            code: code1
                        })
                    }
                }
                Ephedrine.processResult = result => {
                    result1 = result;
                    triggerCheck();
                }
                Ephedrine.processCode = code => {
                    code1 = code;
                    triggerCheck();
                }
            } else {
                Ephedrine.processCode = code => {
                    resolve(code);
                }
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
        },
        Verify: {
            MD5: async(file) => {
                let result = await Ephedrine.msgAsync("Verify", "MD5", file, null, null, null, true)
                switch (result.code) {
                    case -1:
                        {
                            //Busy
                            return {
                                code: -1,
                                success: false,
                                result: "",
                                msg: "Client Busy"
                            }
                            break;
                        };
                    case 80:
                        {
                            //No File
                            return {
                                code: 80,
                                success: false,
                                result: result.result,
                                msg: "No Such File"
                            }
                            break;
                        }
                    case 81:
                        {
                            //Verify Finish
                            return {
                                code: 81,
                                success: true,
                                result: "",
                                msg: "Equal"
                            }
                            break;
                        };
                }
            },
        },
    }
}